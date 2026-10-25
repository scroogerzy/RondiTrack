using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using DotNet.Testcontainers.Images;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using RondiTrack.Infrastructure.Data;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace RondiTrack.Tests.TestSupport;

public sealed class PostgresApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestSigningKey = "test-only-signing-key-at-least-32-bytes-long";
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(new DockerImage("postgres", null, "17", null, null))
        .WithDatabase("ronditrack_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync() => await _container.StartAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:RondiTrack", _container.GetConnectionString());
        builder.UseSetting("Jwt:SigningKey", TestSigningKey);
        builder.UseSetting("RateLimiting:Login:PermitLimit", "1000");
    }

    public new HttpClient CreateClient()
    {
        Guid userId;
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RondiTrackDbContext>();
            userId = db.Users.OrderBy(user => user.Email).Select(user => user.Id).First();
        }
        return CreateClientForUser(userId, "PlatformAdmin");
    }

    public HttpClient CreateClientForUser(Guid userId, string? platformRole = null)
    {
        var client = base.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = userId.ToString()
        };
        if (!string.IsNullOrWhiteSpace(platformRole))
            claims["role"] = platformRole;
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "https://api.ronditrack.co.za",
            Audience = "ronditrack-web",
            Claims = claims,
            NotBefore = DateTime.UtcNow.AddSeconds(-5),
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = credentials
        };
        var accessToken = new JsonWebTokenHandler().CreateToken(descriptor);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    public HttpClient CreateUnauthenticatedClient() => base.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition("Postgres collection")]
public sealed class PostgresCollection : ICollectionFixture<PostgresApiFactory> { }
