using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RondiTrack.Tests.Integration;

/// <summary>
/// Creates the real RondiTrack application for integration tests.
///
/// WebApplicationFactory starts the application using the actual Program.cs.
/// This means integration tests exercise the real dependency injection,
/// middleware, validation, controllers and services instead of mocking them.
/// </summary>
public class ApiTestFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Configures the application environment used by the tests.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }
}
/// <summary>One shared integration host avoids parallel startup-seed races against the same PostgreSQL database.</summary>
[CollectionDefinition("RondiTrack PostgreSQL collection")]
public sealed class RondiTrackPostgreSqlCollection : ICollectionFixture<ApiTestFactory> { }