using Scalar.AspNetCore;
using RondiTrack.Data;

var builder = WebApplication.CreateBuilder(args);

// Register controller support.
// Controllers contain the HTTP endpoints for Users and Stokvels.
builder.Services.AddControllers(options =>
{
    // Keep the Async suffix in action names.
    // This supports CreatedAtAction with asynchronous methods.
    options.SuppressAsyncSuffixInActionNames = false;
});

// Register the built-in .NET 10 OpenAPI document generation.
builder.Services.AddOpenApi();

// Register in-memory repositories as Singleton.
// Singleton is required so the same in-memory collections
// remain available throughout the lifetime of the application.
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<IStokvelRepository, InMemoryStokvelRepository>();

var app = builder.Build();

// Configure OpenAPI and Scalar during development.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Enable attribute-routed controllers.
app.MapControllers();

app.Run();