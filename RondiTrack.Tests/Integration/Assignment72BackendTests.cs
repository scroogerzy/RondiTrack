using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RondiTrack.DTOs.ContributionCycles;
using RondiTrack.DTOs.Contributions;
using RondiTrack.DTOs.Me;
using RondiTrack.Infrastructure.Data;
using RondiTrack.Tests.TestSupport;
using System.Linq;
namespace RondiTrack.Tests.Integration;

[Collection("Postgres collection")]
public sealed class Assignment72BackendTests
{
    private readonly PostgresApiFactory _factory;

    public Assignment72BackendTests(PostgresApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Assignment72_seed_exposes_cycles_totals_cycle_ids_and_callers_arrears()
    {
        Guid userId;
        Guid stokvelId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RondiTrackDbContext>();
            userId = await db.Users.Where(user => user.Email == "lerato@example.com")
                .Select(user => user.Id).SingleAsync();
            stokvelId = await db.Stokvels.Where(stokvel => stokvel.Name == "Ubuntu Savings Club")
                .Select(stokvel => stokvel.Id).SingleAsync();
        }

        using var client = _factory.CreateClientForUser(userId);
        var cyclesResponse = await client.GetAsync($"/api/v1/stokvels/{stokvelId}/cycles?pageSize=100");
        Assert.Equal(HttpStatusCode.OK, cyclesResponse.StatusCode);
        var cycles = await cyclesResponse.Content.ReadFromJsonAsync<ContributionCyclePageResponse>();
        Assert.NotNull(cycles);
        Assert.Equal(new[] { 4, 3, 2 }, cycles.Items.Select(cycle => cycle.PeriodNumber).ToArray());
        Assert.Equal(new[] { "Open", "Closed", "PaidOut" },
            cycles.Items.Select(cycle => cycle.Status).ToArray());

        var openCycle = cycles.Items[0];
        Assert.Equal(100_000L, openCycle.TotalCollectedCents);
        Assert.Equal(150_000L, openCycle.AmountDueCents);

        foreach (var cycle in cycles.Items)
        {
            var contributionResponse = await client.GetAsync(
                $"/api/v1/stokvels/{stokvelId}/cycles/{cycle.Id}/contributions?pageSize=100");
            Assert.Equal(HttpStatusCode.OK, contributionResponse.StatusCode);
            var page = await contributionResponse.Content.ReadFromJsonAsync<ContributionPageResponse>();
            Assert.NotNull(page);
            Assert.All(page.Items, contribution => Assert.Equal(cycle.Id, contribution.CycleId));
        }

        var homeResponse = await client.GetAsync("/api/v1/me/home");
        Assert.Equal(HttpStatusCode.OK, homeResponse.StatusCode);
        var home = await homeResponse.Content.ReadFromJsonAsync<HomeResponse>();
        Assert.NotNull(home);
        var ownItem = Assert.Single(home.Stokvels, item => item.StokvelId == stokvelId);
        Assert.Equal(50_000L, ownItem.ArrearsCents);
    }
}
