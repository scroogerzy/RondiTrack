using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RondiTrack.Data;
using RondiTrack.DTOs.ContributionCycles;
using RondiTrack.DTOs.Stokvels;

namespace RondiTrack.Tests.Integration;

// Follows the Bitcube/Matric Compass Week 5 Day 3 concurrency pattern:
// two independent DbContexts, deterministic save order, no sleeps or parallel tasks.
[Collection("RondiTrack PostgreSQL collection")]
public sealed class ConcurrencyTests
{
    private readonly ApiTestFactory _factory;
    private readonly HttpClient _client;

    public ConcurrencyTests(ApiTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Second_writer_with_stale_xmin_throws_DbUpdateConcurrencyException()
    {
        var created = await CreateCycleAsync();

        // Two scopes are required: they create independent DbContexts with independent
        // original xmin values. One context uses an identity map and returns the same
        // tracked object on repeated queries, so it cannot model two stale readers.
        using var scopeA = _factory.Services.CreateScope();
        using var scopeB = _factory.Services.CreateScope();
        var dbA = scopeA.ServiceProvider.GetRequiredService<RondiTrackDbContext>();
        var dbB = scopeB.ServiceProvider.GetRequiredService<RondiTrackDbContext>();

        var writerA = await dbA.ContributionCycles
            .SingleAsync(cycle => cycle.Id == created.Cycle.Id);
        var writerB = await dbB.ContributionCycles
            .SingleAsync(cycle => cycle.Id == created.Cycle.Id);

        writerA.Update(
            writerA.PeriodNumber,
            writerA.StartDate,
            writerA.EndDate,
            writerA.TargetAmount + 1m);
        await dbA.SaveChangesAsync();

        // writerB still has the old xmin; its write must affect zero rows and fail.
        writerB.Update(
            writerB.PeriodNumber,
            writerB.StartDate,
            writerB.EndDate,
            writerB.TargetAmount + 2m);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => dbB.SaveChangesAsync());
    }

    [Fact]
    public async Task Update_with_stale_xmin_returns_409_problem_json()
    {
        var created = await CreateCycleAsync();
        var url = $"/api/stokvels/{created.StokvelId}/cycles/{created.Cycle.Id}";

        // GET exposes the token that the client must echo on update.
        var original = await _client.GetFromJsonAsync<ContributionCycleResponse>(url);
        Assert.NotNull(original);
        Assert.NotEqual(0u, original.Version);

        var first = await _client.PutAsJsonAsync(url, new
        {
            periodNumber = original.PeriodNumber,
            startDate = original.StartDate,
            endDate = original.EndDate,
            targetAmount = original.TargetAmount + 1m,
            version = original.Version
        });
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);

        // Reuse the exact token from the original GET after the first update changed xmin.
        var stale = await _client.PutAsJsonAsync(url, new
        {
            periodNumber = original.PeriodNumber,
            startDate = original.StartDate,
            endDate = original.EndDate,
            targetAmount = original.TargetAmount + 2m,
            version = original.Version
        });

        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal(
            "application/problem+json",
            stale.Content.Headers.ContentType?.MediaType);

        using var problem = JsonDocument.Parse(await stale.Content.ReadAsStringAsync());
        Assert.Equal(409, problem.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Conflict", problem.RootElement.GetProperty("title").GetString());

        var reread = await _client.GetFromJsonAsync<ContributionCycleResponse>(url);
        Assert.NotNull(reread);
        Assert.Equal(original.PeriodNumber, reread.PeriodNumber);
        Assert.Equal(original.TargetAmount + 1m, reread.TargetAmount); // The first write remains; the stale write did not overwrite it.
    }

    private async Task<(Guid StokvelId, ContributionCycleResponse Cycle)> CreateCycleAsync()
    {
        var stokvelResponse = await _client.PostAsJsonAsync("/api/stokvels", new
        {
            name = $"Concurrency Stokvel {Guid.NewGuid():N}",
            monthlyContribution = 500m
        });
        Assert.Equal(HttpStatusCode.Created, stokvelResponse.StatusCode);

        var stokvel = await stokvelResponse.Content.ReadFromJsonAsync<StokvelResponse>();
        Assert.NotNull(stokvel);

        var start = DateTime.UtcNow.Date;
        var cycleResponse = await _client.PostAsJsonAsync(
            $"/api/stokvels/{stokvel.Id}/cycles",
            new
            {
                periodNumber = 1,
                startDate = start,
                endDate = start.AddMonths(1),
                targetAmount = 500m
            });
        Assert.Equal(HttpStatusCode.Created, cycleResponse.StatusCode);

        var cycle = await cycleResponse.Content.ReadFromJsonAsync<ContributionCycleResponse>();
        Assert.NotNull(cycle);
        return (stokvel.Id, cycle);
    }
}
