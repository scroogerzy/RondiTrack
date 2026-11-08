using RondiTrack.Domain.Calculations;

namespace RondiTrack.Tests.Unit;

public sealed class ArrearCalculatorTests
{
    [Theory]
    [InlineData(500, 500, 0)]
    [InlineData(500, 200, 30000)]
    [InlineData(500, 0, 50000)]
    [InlineData(500, 550, 0)]
    public void CalculatesShortfallInIntegerCents(decimal required, decimal paid, long expected) =>
        Assert.Equal(expected, ArrearCalculator.CalculateShortfallCents(required, paid));

    [Fact]
    public void RoundsHalfCentAwayFromZero() =>
        Assert.Equal(1, ArrearCalculator.CalculateShortfallCents(500.005m, 500m));

    [Fact]
    public void RejectsNegativeAmounts() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => ArrearCalculator.CalculateShortfallCents(500m, -1m));
}
