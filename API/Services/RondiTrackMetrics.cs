using System.Diagnostics.Metrics;

namespace RondiTrack.Services;

public sealed class RondiTrackMetrics : IDisposable
{
    public const string MeterName = "RondiTrack.Business";

    private readonly Meter _meter;
    private readonly Counter<long> _contributions;
    private readonly Counter<long> _payouts;
    private readonly Counter<long> _authorizationRefusals;

    public RondiTrackMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MeterName);
        _contributions = _meter.CreateCounter<long>("ronditrack.contributions.recorded");
        _payouts = _meter.CreateCounter<long>("ronditrack.payouts.processed");
        _authorizationRefusals = _meter.CreateCounter<long>("ronditrack.authorization.refusals");
    }

    public void ContributionRecorded() =>
        _contributions.Add(1);

    public void PayoutProcessed(string outcome) =>
        _payouts.Add(1, new KeyValuePair<string, object?>("outcome", outcome));

    public void AuthorizationRefused(string reason) =>
        _authorizationRefusals.Add(1, new KeyValuePair<string, object?>("reason", reason));

    public void Dispose() => _meter.Dispose();
}
