namespace RondiTrack.Domain.Models;

public sealed class ContributionProof
{
    public Guid Id { get; private set; }
    public Guid StokvelId { get; private set; }
    public Guid ContributionId { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public byte[] Content { get; private set; } = [];
    public DateTime UploadedAtUtc { get; private set; }
    private ContributionProof() { }
    public ContributionProof(Guid stokvelId, Guid contributionId, Guid uploadedByUserId, string fileName, string contentType, byte[] content, DateTime uploadedAtUtc)
    { Id = Guid.NewGuid(); StokvelId = stokvelId; ContributionId = contributionId; UploadedByUserId = uploadedByUserId; FileName = fileName; ContentType = contentType; Content = content; UploadedAtUtc = uploadedAtUtc; }

    public void Replace(Guid uploadedByUserId, string fileName, string contentType, byte[] content, DateTime uploadedAtUtc)
    { UploadedByUserId = uploadedByUserId; FileName = fileName; ContentType = contentType; Content = content; UploadedAtUtc = uploadedAtUtc; }
}
