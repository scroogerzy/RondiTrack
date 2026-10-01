using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using RondiTrack.Data;
using RondiTrack.Middleware;
using RondiTrack.Services;
using RondiTrack.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();

builder.Services.AddOpenApi();

// EF Core is configured for PostgreSQL.
// User persistence moves to the database in Assignment 5.1,
// while the remaining resources deliberately stay in-memory
// until their own database migration work is introduced.
builder.Services.AddDbContext<RondiTrackDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("RondiTrack"));
});

// User now uses the EF Core repository backed by PostgreSQL.
builder.Services.AddScoped<IUserRepository, EfUserRepository>();

// These repositories remain in-memory for Assignment 5.1.
builder.Services.AddSingleton<IStokvelRepository, InMemoryStokvelRepository>();
builder.Services.AddSingleton<IContributionRepository, InMemoryContributionRepository>();
builder.Services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
builder.Services.AddSingleton<IContributionCycleRepository, InMemoryContributionCycleRepository>();

// The service contains business logic but does not maintain request state.
builder.Services.AddScoped<IStokvelService, StokvelService>();

var app = builder.Build();
// Apply pending EF Core migrations and seed the database during startup.
// The seed checks for existing Users first so restarting the API does
// not create duplicate records.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<RondiTrackDbContext>();

    await dbContext.Database.MigrateAsync();

    if (!await dbContext.Users.AnyAsync())
    {
        dbContext.Users.AddRange(
            new RondiTrack.Models.User(
                "Thabo Mokoena",
                "thabo@example.com"),

            new RondiTrack.Models.User(
                "Lerato Dlamini",
                "lerato@example.com"),

            new RondiTrack.Models.User(
                "Sibusiso Ndlovu",
                "sibusiso@example.com"));
        
        await dbContext.SaveChangesAsync();
    }
}
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();