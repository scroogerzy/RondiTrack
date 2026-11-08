namespace RondiTrack.Domain.Calculations;

/// <summary>Single source of truth for cycle shortfall calculations.</summary>
public static class ArrearCalculator
{
    public static long CalculateShortfallCents(decimal requiredContribution, decimal amountPaid)
    {
        if (requiredContribution < 0m || amountPaid < 0m)
            throw new ArgumentOutOfRangeException(nameof(requiredContribution), "Contribution amounts cannot be negative.");
        var shortfall = Math.Max(0m, requiredContribution - amountPaid);
        return checked((long)decimal.Round(shortfall * 100m, 0, MidpointRounding.AwayFromZero));
    }
}
