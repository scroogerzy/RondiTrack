using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using RondiTrack.Domain.Models;
using RondiTrack.Infrastructure.Data;
using RondiTrack.Services;

namespace RondiTrack.API.Services.BackgroundJobs;

public sealed class CycleCloseBackgroundService : BackgroundService
{
    private static readonly TimeZoneInfo Johannesburg =
        TimeZoneInfo.FindSystemTimeZoneById("Africa/Johannesburg");

    private static readonly ActivitySource ActivitySource =
        new("RondiTrack.CycleClose");

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _clock;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CycleCloseBackgroundService> _logger;
    private readonly RondiTrackMetrics _metrics;

    public CycleCloseBackgroundService(
        IServiceScopeFactory scopeFactory,
        TimeProvider clock,
        IConfiguration configuration,
        ILogger<CycleCloseBackgroundService> logger,
        RondiTrackMetrics metrics)
    {
        _scopeFactory = scopeFactory;
        _clock = clock;
        _configuration = configuration;
        _logger = logger;
        _metrics = metrics;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var seconds = Math.Clamp(
            _configuration.GetValue("CycleClose:IntervalSeconds", 60),
            5,
            86400);

        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(seconds), _clock);

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
                _logger.LogError(ex, "Cycle-close run failed.");
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
        using var activity = ActivitySource.StartActivity("cycle-close.run");

        List<Guid> stokvelIds;

        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RondiTrackDbContext>();
            stokvelIds = await db.Stokvels
                .AsNoTracking()
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);
        }

        foreach (var stokvelId in stokvelIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await CloseStokvelAsync(stokvelId, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Cycle closure failed for stokvel {StokvelId}; it will be retried on the next run.",
                    stokvelId);
            }
        }
    }

    private async Task CloseStokvelAsync(
        Guid stokvelId,
        CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RondiTrackDbContext>();
        var nowUtc = _clock.GetUtcNow().UtcDateTime;

        await using var transaction =
            await db.Database.BeginTransactionAsync(cancellationToken);

        var stokvel = await db.Stokvels
            .SingleOrDefaultAsync(s => s.Id == stokvelId, cancellationToken);

        if (stokvel is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return;
        }

        var members = await db.StokvelMembers
            .AsNoTracking()
            .Where(m => m.StokvelId == stokvelId)
            .Select(m => m.UserId)
            .ToListAsync(cancellationToken);

        var closedCount = 0;
        var arrearsCount = 0;

        // Re-query after each closure so newly created overdue cycles are
        // processed during this run. All work remains inside this stokvel's
        // transaction, so a failure rolls back the entire stokvel operation.
        while (true)
        {
            var cycle = await db.ContributionCycles
                .Where(c => c.StokvelId == stokvelId && c.ClosedAtUtc == null)
                .OrderBy(c => c.PeriodNumber)
                .FirstOrDefaultAsync(cancellationToken);

            // Process periods sequentially; do not skip an earlier open cycle.
            if (cycle is null || !HasEndedInJohannesburg(cycle.EndDate, nowUtc))
                break;

            var claimed = await db.ContributionCycles
                .Where(c => c.Id == cycle.Id && c.ClosedAtUtc == null)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(c => c.ClosedAtUtc, nowUtc),
                    cancellationToken);

            if (claimed == 0)
            {
                db.Entry(cycle).State = EntityState.Detached;
                continue;
            }

            var paid = await db.Contributions
                .AsNoTracking()
                .Where(c => c.StokvelId == stokvelId &&
                            c.Cycle == cycle.PeriodNumber)
                .GroupBy(c => c.UserId)
                .Select(g => new { UserId = g.Key, Total = g.Sum(c => c.Amount) })
                .ToDictionaryAsync(x => x.UserId, x => x.Total, cancellationToken);

            foreach (var userId in members)
            {
                paid.TryGetValue(userId, out var amountPaid);
                var shortfall = Math.Max(0m, stokvel.MonthlyContribution - amountPaid);

                if (shortfall <= 0m)
                    continue;

                var cents = checked((long)decimal.Round(
                    shortfall * 100m, 0, MidpointRounding.AwayFromZero));

                db.ArrearRecords.Add(new ArrearRecord(
                    stokvelId, userId, cycle.PeriodNumber, cents, nowUtc));

                arrearsCount++;
            }

            // Keep the configured monthly date pattern and never duplicate a
            // period if it already exists.
            var nextPeriod = checked(cycle.PeriodNumber + 1);
            var nextExists = await db.ContributionCycles.AnyAsync(
                c => c.StokvelId == stokvelId &&
                     c.PeriodNumber == nextPeriod,
                cancellationToken);

            if (!nextExists)
            {
                db.ContributionCycles.Add(new ContributionCycle(
                    stokvelId,
                    nextPeriod,
                    cycle.StartDate.AddMonths(1),
                    cycle.EndDate.AddMonths(1),
                    cycle.TargetAmount));
            }

            closedCount++;

            // Persist each iteration so the next query can see the new cycle.
            // The surrounding database transaction still makes the stokvel
            // operation all-or-nothing.
            await db.SaveChangesAsync(cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        for (var i = 0; i < closedCount; i++)
            _metrics.CycleClosed();

        for (var i = 0; i < arrearsCount; i++)
            _metrics.ArrearRecorded();

        if (closedCount > 0)
        {
            _logger.LogInformation(
                "Closed {CycleCount} cycle(s) and recorded {ArrearsCount} arrears record(s) for stokvel {StokvelId}.",
                closedCount, arrearsCount, stokvelId);
        }
    }

    private static bool HasEndedInJohannesburg(DateTime endDate, DateTime nowUtc)
    {
        var localEndDate = endDate.Date;
        var nextLocalMidnight = DateTime.SpecifyKind(
            localEndDate.AddDays(1), DateTimeKind.Unspecified);

        var closureUtc = TimeZoneInfo.ConvertTimeToUtc(
            nextLocalMidnight, Johannesburg);

        return nowUtc >= closureUtc;
    }
}
