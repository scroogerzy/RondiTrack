using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry;
using OpenTelemetry.Resources;

using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using RondiTrack.Domain.Repositories;
using RondiTrack.Infrastructure.Data;
using RondiTrack.API.Data;
using RondiTrack.Middleware;
using RondiTrack.Domain.Models;
using RondiTrack.Services;
using RondiTrack.Validators;
using RondiTrack.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using RondiTrack.Common;
using RondiTrack.API.Services.BackgroundJobs;

var builder = WebApplication.CreateBuilder(args);

// Authentication settings: the signing key must come from user-secrets or environment configuration.
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey) || Encoding.UTF8.GetByteCount(jwtOptions.SigningKey) < 32)
    throw new InvalidOperationException("JWT signing key is missing or too short. Set Jwt:SigningKey in user-secrets (at least 32 bytes). Tests must override Jwt:SigningKey in the test factory.");
if (jwtOptions.AccessTokenMinutes is < 1 or > 15)
    throw new InvalidOperationException("Jwt:AccessTokenMinutes must be between 1 and 15.");

builder.Services.Configure<JwtOptions>(jwtSection);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true, ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.StokvelMember, policy => policy.AddRequirements(new StokvelAccessRequirement()))
    .AddPolicy(Policies.StokvelManager, policy => policy.AddRequirements(new StokvelAccessRequirement(managerRequired: true)))
    .AddPolicy(Policies.RecordContribution, policy => policy.AddRequirements(new RecordContributionRequirement()))
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddScoped<IAuthorizationHandler, StokvelAccessHandler>();
builder.Services.AddScoped<IAuthorizationHandler, RecordContributionHandler>();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IRefreshTokenRepository, EfRefreshTokenRepository>();
builder.Services.AddScoped<AuthService>();

// Register MVC controllers.
builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
})
.ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = false;

    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => JsonNamingPolicy.CamelCase.ConvertName(entry.Key),
                entry => entry.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "The supplied value is invalid."
                        : error.ErrorMessage)
                    .ToArray());

        var problem = new ValidationProblemDetails(errors)
        {
            Type = "https://api.ronditrack.co.za/errors/validation",
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Detail = "Correct the supplied fields and try again.",
            Instance = context.HttpContext.Request.Path
        };
        problem.Extensions["correlationId"] =
            context.HttpContext.Items["X-Correlation-ID"]?.ToString()
            ?? context.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(problem)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});

// Also configure minimal-API/HTTP JSON options so OpenAPI and runtime
// use the same strict numeric contract.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

// Register automatic request validation.
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();

// Register API documentation.
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Configure EF Core to use PostgreSQL.
builder.Services.AddDbContext<RondiTrackDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("RondiTrack"));
});

// Register database-backed repositories.
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<IStokvelRepository, EfStokvelRepository>();

// StokvelMember uses a composite primary key and its own repository.
builder.Services.AddScoped<
    IStokvelMemberRepository,
    EfStokvelMemberRepository>();

// Contributions and cycles use the same PostgreSQL database.
builder.Services.AddScoped<
    IContributionRepository,
    EfContributionRepository>();

builder.Services.AddScoped<
    IContributionCycleRepository,
    EfContributionCycleRepository>();

// The idempotency store remains in-memory in this implementation.
builder.Services.AddSingleton<
    IIdempotencyStore,
    InMemoryIdempotencyStore>();

// Register the business service.
builder.Services.AddScoped<IStokvelService, StokvelService>();

// Assignment 6.2: OpenTelemetry traces, metrics and logs.
builder.Services.AddSingleton<RondiTrackMetrics>();
builder.Services.AddHostedService<CycleCloseBackgroundService>();
builder.Services.AddHostedService<HousekeepingBackgroundService>();

var telemetry = builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("ronditrack-api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
            options.Filter = context =>
                !context.Request.Path.StartsWithSegments("/health"))
        .AddSource("Npgsql")
        .AddSource("RondiTrack.CycleClose")
        .AddSource(RondiTrack.Services.RondiTrackActivity.Source.Name))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddMeter("System.Runtime")
        .AddMeter(RondiTrackMetrics.MeterName))
    .WithLogging();

if (!string.IsNullOrWhiteSpace(
        builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
{
    telemetry.UseOtlpExporter();
}

// PostgreSQL readiness check; liveness does not depend on the database.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<RondiTrackDbContext>(
        name: "postgres",
        tags: new[] { "ready" });


// Exact-origin CORS: credentials cannot be combined with a wildcard origin.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicies.Frontend, policy =>
    {
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins);
        policy.WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS");
        policy.WithHeaders("Authorization", "Content-Type", "Idempotency-Key", "X-Correlation-ID");
        policy.WithExposedHeaders("Retry-After", "X-Correlation-ID", "Idempotency-Replayed");
        policy.AllowCredentials().SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// Login is partitioned by client IP. Financial writes are partitioned by
// authenticated subject, so users sharing venue Wi-Fi do not share money limits.
var loginPermitLimit = builder.Configuration.GetValue("RateLimiting:Login:PermitLimit", 5);
var loginWindowSeconds = builder.Configuration.GetValue("RateLimiting:Login:WindowSeconds", 60);
var contributionPermitLimit = builder.Configuration.GetValue("RateLimiting:Contributions:PermitLimit", 25);
var contributionWindowSeconds = builder.Configuration.GetValue("RateLimiting:Contributions:WindowSeconds", 60);
var payoutPermitLimit = builder.Configuration.GetValue("RateLimiting:Payouts:PermitLimit", 5);
var payoutWindowSeconds = builder.Configuration.GetValue("RateLimiting:Payouts:WindowSeconds", 60);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        var response = context.HttpContext.Response;
        response.StatusCode = StatusCodes.Status429TooManyRequests;
        response.ContentType = "application/problem+json";
        var retryAfterSeconds = context.Lease.TryGetMetadata(
            MetadataName.RetryAfter, out var retryAfter)
            ? Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds))
            : 1;
        response.Headers["Retry-After"] = retryAfterSeconds.ToString();

        var correlationId = context.HttpContext.Items["X-Correlation-ID"]?.ToString()
            ?? context.HttpContext.TraceIdentifier;
        var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Type = "https://api.ronditrack.co.za/errors/rate-limit",
            Title = "Too Many Requests",
            Status = StatusCodes.Status429TooManyRequests,
            Detail = "The request limit has been reached. Wait before retrying.",
            Instance = context.HttpContext.Request.Path
        };
        problem.Extensions["correlationId"] = correlationId;
        await JsonSerializer.SerializeAsync(response.Body, problem, cancellationToken: cancellationToken);
    };

    options.AddPolicy(RateLimitPolicies.Login, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = Math.Max(1, loginPermitLimit),
                Window = TimeSpan.FromSeconds(Math.Max(1, loginWindowSeconds)),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy(RateLimitPolicies.Contributions, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = Math.Max(1, contributionPermitLimit),
                Window = TimeSpan.FromSeconds(Math.Max(1, contributionWindowSeconds)),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy(RateLimitPolicies.Payouts, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = Math.Max(1, payoutPermitLimit),
                Window = TimeSpan.FromSeconds(Math.Max(1, payoutWindowSeconds)),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var app = builder.Build();

// Apply pending migrations and add basic starter records when needed.
// The volume seeder required by Assignment 5.3 should remain a separate,
// explicit operation and must not run automatically at application startup.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<RondiTrackDbContext>();

    await dbContext.Database.MigrateAsync();

    // Seed starter users only if the Users table is empty.
    if (!await dbContext.Users.AnyAsync())
    {
        dbContext.Users.AddRange(
            new User("Thabo Mokoena", "thabo@example.com"),
            new User("Lerato Dlamini", "lerato@example.com"),
            new User("Sibusiso Ndlovu", "sibusiso@example.com"));

        await dbContext.SaveChangesAsync();
    }

    // Preserve the starter stokvels that were previously provided
    // by InMemoryStokvelRepository.
    if (!await dbContext.Stokvels.AnyAsync())
    {
        dbContext.Stokvels.AddRange(
            new Stokvel("Ubuntu Savings Club", 500m),
            new Stokvel("Mzanzi Monthly Stokvel", 500m));

        await dbContext.SaveChangesAsync();
    }

    // The demo's first user owns the seeded demo stokvels. Real memberships
    // are still checked from StokvelMembers on every protected request.
    var demoUser = await dbContext.Users.OrderBy(user => user.Email).FirstOrDefaultAsync();
    if (demoUser is not null)
    {
        var demoStokvels = await dbContext.Stokvels.ToListAsync();
        foreach (var stokvel in demoStokvels)
        {
            var exists = await dbContext.StokvelMembers.AnyAsync(member =>
                member.UserId == demoUser.Id && member.StokvelId == stokvel.Id);
            if (!exists)
                dbContext.StokvelMembers.Add(new StokvelMember(
                    demoUser.Id, stokvel.Id, RondiTrack.Auth.StokvelRoles.Chairperson, DateTime.UtcNow));
        }
        await dbContext.SaveChangesAsync();
    }
}

// Liveness checks the process only. Readiness checks PostgreSQL.
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = async (context, report) =>
        await context.Response.WriteAsync(report.Status.ToString())
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
        await context.Response.WriteAsync(report.Status.ToString())
}).AllowAnonymous();


// Expose documentation endpoints in Development only.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

// Register middleware in the existing request pipeline.
app.UseRouting();
app.UseCors(CorsPolicies.Frontend);
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseStatusCodePages(async context =>
{
    var httpContext = context.HttpContext;
    var response = httpContext.Response;

    if (response.HasStarted) return;

    var status = response.StatusCode;
    var title = status switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status405MethodNotAllowed => "Method Not Allowed",
        _ => "Request failed"
    };

    var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        Type = $"https://api.ronditrack.co.za/errors/{status}",
        Title = title,
        Status = status,
        Detail = status switch
        {
            StatusCodes.Status400BadRequest => "The request could not be processed.",
            StatusCodes.Status401Unauthorized => "Authentication is required or the supplied credentials are invalid.",
            StatusCodes.Status403Forbidden => "You do not have permission to perform this operation.",
            StatusCodes.Status404NotFound => "The requested resource was not found.",
            StatusCodes.Status405MethodNotAllowed => "The HTTP method is not supported for this resource.",
            _ => "The request could not be completed."
        },
        Instance = httpContext.Request.Path
    };
    problem.Extensions["correlationId"] =
        httpContext.Items["X-Correlation-ID"]?.ToString()
        ?? httpContext.TraceIdentifier;

    response.ContentType = "application/problem+json";
    await System.Text.Json.JsonSerializer.SerializeAsync(
        response.Body,
        problem,
        cancellationToken: httpContext.RequestAborted);
});
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

app.Run();
