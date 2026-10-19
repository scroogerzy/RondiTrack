using Microsoft.EntityFrameworkCore;
using RondiTrack.API.Data;
using RondiTrack.Infrastructure.Data;

namespace RondiTrack.API.Services.BackgroundJobs;

public sealed class HousekeepingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _clock;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HousekeepingBackgroundService> _logger;

    public HousekeepingBackgroundService(
        IServiceScopeFactory scopeFactory,
        TimeProvider clock,
        IConfiguration configuration,
        ILogger<HousekeepingBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _clock = clock;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var seconds = Math.Clamp(
            _configuration.GetValue("Housekeeping:IntervalSeconds", 3600),
            5,
            86400);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(seconds), _clock);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Housekeeping run failed; it will retry next interval.");
            }

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                    break;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    public async Task RunOnceAsync(CancellationToken cancellationToken = default)
    {
        var now = _clock.GetUtcNow().UtcDateTime;
        var retentionDays = Math.Clamp(
            _configuration.GetValue("Housekeeping:RefreshTokenRetentionDays", 7),
            1,
            3650);
        var idempotencyHours = Math.Clamp(
            _configuration.GetValue("Housekeeping:IdempotencyRetentionHours", 48),
            1,
            87600);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RondiTrackDbContext>();
        var idempotency = scope.ServiceProvider.GetRequiredService<IIdempotencyStore>();

        // Keep expired tokens for a reuse-detection window before deletion.
        var refreshCutoff = now.AddDays(-retentionDays);
        var deletedTokens = await db.RefreshTokens
            .Where(token => token.ExpiresAt < refreshCutoff)
            .ExecuteDeleteAsync(cancellationToken);

        var idempotencyCutoff = now.AddHours(-idempotencyHours);
        var deletedIdempotency = await idempotency.RemoveOlderThanAsync(idempotencyCutoff);

        if (deletedTokens > 0 || deletedIdempotency > 0)
        {
            _logger.LogInformation(
                "Housekeeping removed {RefreshTokenCount} old refresh tokens and {IdempotencyCount} old idempotency entries.",
                deletedTokens,
                deletedIdempotency);
        }
    }
}
