using System.Linq;
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RondiTrack.DTOs.ContributionCycles;
using RondiTrack.DTOs.Payouts;
using RondiTrack.Infrastructure.Data;
using RondiTrack.Tests.TestSupport;

namespace RondiTrack.Tests.Integration;

[Collection("Postgres collection")]
public sealed class PayoutRotationTests(PostgresApiFactory factory)
{
    [Fact]
    public async Task Payout_order_is_server_ordered_and_has_one_next_recipient()
    {
        Guid leratoId;
        Guid stokvelId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RondiTrackDbContext>();
            leratoId = await db.Users.Where(user => user.Email == "lerato@example.com").Select(user => user.Id).SingleAsync();
            stokvelId = await db.Stokvels.Where(stokvel => stokvel.Name == "Ubuntu Savings Club").Select(stokvel => stokvel.Id).SingleAsync();
        }

        using var client = factory.CreateClientForUser(leratoId);
        var cyclesResponse = await client.GetAsync($"/api/v1/stokvels/{stokvelId}/cycles?pageSize=100&sortDirection=asc");
        Assert.Equal(HttpStatusCode.OK, cyclesResponse.StatusCode);
        var cycles = await cyclesResponse.Content.ReadFromJsonAsync<ContributionCyclePageResponse>();
        Assert.NotNull(cycles);
        var cycle = Assert.Single(cycles.Items, item => item.PeriodNumber == 4);

        var response = await client.GetAsync($"/api/v1/stokvels/{stokvelId}/cycles/{cycle.Id}/payout-order");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<PayoutOrderResponse>();
        Assert.NotNull(order);
        Assert.Equal(cycle.Id, order.CycleId);
        Assert.Equal(Enumerable.Range(1, order.Entries.Count), order.Entries.Select(entry => entry.Position));
        Assert.Single(order.Entries, entry => entry.Status == "next");
        Assert.All(order.Entries, entry => Assert.True(entry.AmountInCents > 0));

        var paidCycle = Assert.Single(cycles.Items, item => item.PeriodNumber == 2);
        var paidResponse = await client.GetAsync($"/api/v1/stokvels/{stokvelId}/cycles/{paidCycle.Id}/payout-order");
        Assert.Equal(HttpStatusCode.OK, paidResponse.StatusCode);
        var paidOrder = await paidResponse.Content.ReadFromJsonAsync<PayoutOrderResponse>();
        Assert.NotNull(paidOrder);
        Assert.Single(paidOrder.Entries, entry => entry.Status == "paid");
    }

    [Fact]
    public async Task Member_cannot_record_a_payout()
    {
        Guid memberId;
        Guid stokvelId;
        Guid cycleId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RondiTrackDbContext>();
            memberId = await db.Users.Where(user => user.Email == "sibusiso@example.com").Select(user => user.Id).SingleAsync();
            stokvelId = await db.Stokvels.Where(stokvel => stokvel.Name == "Ubuntu Savings Club").Select(stokvel => stokvel.Id).SingleAsync();
            cycleId = await db.ContributionCycles.Where(cycle => cycle.StokvelId == stokvelId).OrderBy(cycle => cycle.PeriodNumber).Select(cycle => cycle.Id).FirstAsync();
        }

        using var client = factory.CreateClientForUser(memberId);
        var response = await client.PostAsync($"/api/v1/stokvels/{stokvelId}/cycles/{cycleId}/payout", content: null);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
