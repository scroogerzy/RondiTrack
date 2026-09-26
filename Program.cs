using FluentValidation;
using FluentValidation.AspNetCore;
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

// In-memory repositories are registered as singletons so that
// application data remains available across HTTP requests.
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<IStokvelRepository, InMemoryStokvelRepository>();
builder.Services.AddSingleton<IContributionRepository, InMemoryContributionRepository>();
builder.Services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
builder.Services.AddSingleton<IContributionCycleRepository, InMemoryContributionCycleRepository>();

// The service contains business logic but does not maintain request state.
builder.Services.AddScoped<IStokvelService, StokvelService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();