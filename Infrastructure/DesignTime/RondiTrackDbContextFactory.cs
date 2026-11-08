using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RondiTrack.Infrastructure.Data;

public sealed class RondiTrackDbContextFactory
    : IDesignTimeDbContextFactory<RondiTrackDbContext>
{
    public RondiTrackDbContext CreateDbContext(string[] args)
    {
        var root = Directory.GetCurrentDirectory();
        var apiPath = Path.Combine(root, "API");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.Exists(apiPath) ? apiPath : root)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__RondiTrack")
            ?? configuration.GetConnectionString("RondiTrack");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "ConnectionStrings:RondiTrack is missing.");

        var options = new DbContextOptionsBuilder<RondiTrackDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new RondiTrackDbContext(options);
    }
}
