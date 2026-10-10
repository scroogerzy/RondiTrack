
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using RondiTrack.Data;
using RondiTrack.Middleware;
using RondiTrack.Models;
using RondiTrack.Services;
using RondiTrack.Validators;

var builder = WebApplication.CreateBuilder(args);

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
}

// Expose documentation endpoints in Development only.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Register middleware in the existing request pipeline.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
