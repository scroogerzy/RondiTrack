#nullable enable

using System.Linq;
using System.Text;
using DotNet.Testcontainers.Images;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;
using RondiTrack.API.Services.BackgroundJobs;
using RondiTrack.Infrastructure.Data;

namespace RondiTrack.Tests.TestSupport;

public sealed class PostgresApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestSigningKey =
        "test-only-signing-key-at-least-32-bytes-long";

    private readonly string? _externalConnectionString =
        Environment.GetEnvironmentVariable("RONDITRACK_TEST_POSTGRES");

    private PostgreSqlContainer? _container;
    private string _connectionString = string.Empty;
    private string? _adminConnectionString;
    private string? _testSchema;

    public async Task InitializeAsync()
    {
        if (string.IsNullOrWhiteSpace(_externalConnectionString))
        {
            _container = new PostgreSqlBuilder(
                    new DockerImage("postgres", null, "17", null, null))
                .WithDatabase("ronditrack_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await _container.StartAsync();
            _connectionString = _container.GetConnectionString();
            return;
        }

        // Use a dedicated schema so migrations and seeded test data cannot
        // overwrite tables in the database's default schema.
        var adminBuilder =
            new NpgsqlConnectionStringBuilder(_externalConnectionString);

        _adminConnectionString = adminBuilder.ConnectionString;
        _testSchema = $"ronditrack_test_{Guid.NewGuid():N}";

        await using (var connection =
            new NpgsqlConnection(_adminConnectionString))
        {
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE SCHEMA \"{_testSchema}\"";
            await command.ExecuteNonQueryAsync();
        }

        var scopedBuilder =
            new NpgsqlConnectionStringBuilder(_adminConnectionString)
            {
                SearchPath = _testSchema
            };

        _connectionString = scopedBuilder.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.UseSetting(
            "ConnectionStrings:RondiTrack",
            _connectionString);

        builder.UseSetting("Jwt:SigningKey", TestSigningKey);
        builder.UseSetting("RateLimiting:Login:PermitLimit", "5");
        builder.UseSetting("RateLimiting:Login:WindowSeconds", "60");

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

        if (_testSchema is not null &&
            _adminConnectionString is not null)
        {
            NpgsqlConnection.ClearAllPools();

            await using var connection =
                new NpgsqlConnection(_adminConnectionString);
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText =
                $"DROP SCHEMA IF EXISTS \"{_testSchema}\" CASCADE";
            await command.ExecuteNonQueryAsync();
        }

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}

[CollectionDefinition("Postgres collection")]
public sealed class PostgresCollection :
    ICollectionFixture<PostgresApiFactory>
{
}
