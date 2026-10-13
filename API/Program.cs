
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
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;

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

// Expose documentation endpoints in Development only.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

// Register middleware in the existing request pipeline.
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
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        _ => "Request failed"
    };

    var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        Type = $"https://api.ronditrack.co.za/errors/{status}",
        Title = title,
        Status = status,
        Detail = status == StatusCodes.Status401Unauthorized
            ? "Authentication is required or the supplied credentials are invalid."
            : "You do not have permission to perform this operation.",
        Instance = httpContext.Request.Path
    };

    response.ContentType = "application/problem+json";
    await System.Text.Json.JsonSerializer.SerializeAsync(
        response.Body,
        problem,
        cancellationToken: httpContext.RequestAborted);
});
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
