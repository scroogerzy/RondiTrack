namespace RondiTrack.Domain.Models;

public sealed class CycleCloseReceipt
{
    public Guid Id { get; private set; }
    public Guid StokvelId { get; private set; }
    public Guid CycleId { get; private set; }
    public string IdempotencyKey { get; private set; } = null!;
    public long PreviewVersion { get; private set; }
    public DateTime ClosedAtUtc { get; private set; }

    private CycleCloseReceipt() { }

    public CycleCloseReceipt(
        Guid stokvelId,
        Guid cycleId,
        string key,
        long version,
        DateTime closedAtUtc)
    {
        Id = Guid.NewGuid();
        StokvelId = stokvelId;
        CycleId = cycleId;
        IdempotencyKey = key;
        PreviewVersion = version;
        ClosedAtUtc = closedAtUtc;
    }
}
