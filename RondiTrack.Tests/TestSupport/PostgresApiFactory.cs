#nullable enable

using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.PostgreSql;
using DotNet.Testcontainers.Images;
using Xunit;
using RondiTrack.API.Services.BackgroundJobs;
using RondiTrack.Infrastructure.Data;

namespace RondiTrack.Tests.TestSupport;

public sealed class PostgresApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestSigningKey =
        "test-only-signing-key-at-least-32-bytes-long";

    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder(new DockerImage("postgres", null, "17", null, null))
            .WithDatabase("ronditrack_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.UseSetting(
            "ConnectionStrings:RondiTrack",
            _container.GetConnectionString());

        builder.UseSetting("Jwt:SigningKey", TestSigningKey);
        builder.UseSetting("RateLimiting:Login:PermitLimit", "1000");

        // Prevent automatic cycle closure from changing seeded test data.
        // This removal applies only to the integration-test host.
        builder.ConfigureTestServices(services =>
        {
            var cycleCloseService = services.FirstOrDefault(descriptor =>
                descriptor.ServiceType == typeof(IHostedService) &&
                descriptor.ImplementationType ==
                    typeof(CycleCloseBackgroundService));

            if (cycleCloseService is not null)
            {
                services.Remove(cycleCloseService);
            }
        });
    }

    public new HttpClient CreateClient()
    {
        Guid userId;

        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<RondiTrackDbContext>();

            userId = db.Users
                .OrderBy(user => user.Email)
                .Select(user => user.Id)
                .First();
        }

        return CreateClientForUser(userId, "PlatformAdmin");
    }

    public HttpClient CreateClientForUser(
        Guid userId,
        string? platformRole = null)
    {
        var client = base.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                HandleCookies = false
            });

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(TestSigningKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = userId.ToString()
        };

        if (!string.IsNullOrWhiteSpace(platformRole))
        {
            claims["role"] = platformRole;
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "https://api.ronditrack.co.za",
            Audience = "ronditrack-web",
            Claims = claims,
            NotBefore = DateTime.UtcNow.AddSeconds(-5),
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = credentials
        };

        var accessToken =
            new JsonWebTokenHandler().CreateToken(descriptor);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        return client;
    }

    public HttpClient CreateUnauthenticatedClient()
    {
        return base.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                HandleCookies = false
            });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition("Postgres collection")]
public sealed class PostgresCollection :
    ICollectionFixture<PostgresApiFactory>
{
}
