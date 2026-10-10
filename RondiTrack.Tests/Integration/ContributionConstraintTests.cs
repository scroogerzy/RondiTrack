using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using RondiTrack.Data;
using RondiTrack.Models;

namespace RondiTrack.Tests.Integration;

// Follows the Bitcube/Matric Compass Week 5 Day 3 ConstraintTests pattern:
// write directly through DbContext so StokvelService's pre-check is bypassed.
[Collection("RondiTrack PostgreSQL collection")]
public sealed class ContributionConstraintTests
{
    private readonly ApiTestFactory _factory;

    public ContributionConstraintTests(ApiTestFactory factory) => _factory = factory;

    [Fact]
    public async Task Duplicate_contribution_is_rejected_by_the_database_unique_index()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RondiTrackDbContext>();

        var unique = Guid.NewGuid().ToString("N");
        var user = new User($"Constraint User {unique}", $"constraint-{unique}@example.test");
        var stokvel = new Stokvel($"Constraint Stokvel {unique}", 500m);
        var start = DateTime.UtcNow.Date;
        var cycle = new ContributionCycle(
            stokvel.Id,
            1,
            start,
            start.AddMonths(1),
            10000m);
        var membership = new StokvelMember(
            user.Id,
            stokvel.Id,
            "Member",
            DateTime.UtcNow);

        db.Users.Add(user);
        db.Stokvels.Add(stokvel);
        db.ContributionCycles.Add(cycle);
        db.StokvelMembers.Add(membership);
        await db.SaveChangesAsync();

        // The first contribution is valid and is committed directly.
        db.Contributions.Add(new Contribution(stokvel.Id, user.Id, 1, 500m));
        await db.SaveChangesAsync();

        // Deliberately bypass the service's GetByMemberAndCycleAsync check.
        db.Contributions.Add(new Contribution(stokvel.Id, user.Id, 1, 500m));
        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            () => db.SaveChangesAsync());

        var postgres = Assert.IsType<PostgresException>(exception.InnerException);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, postgres.SqlState);
        Assert.Equal(
            "UX_Contributions_StokvelId_UserId_Cycle",
            postgres.ConstraintName);
    }
}
