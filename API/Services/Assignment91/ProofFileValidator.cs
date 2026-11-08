namespace RondiTrack.Services.Assignment91;

public static class ProofFileValidator
{
    public static bool TryGetContentType(ReadOnlySpan<byte> bytes, out string contentType)
    {
        if (bytes.Length >= 5 && bytes[..5].SequenceEqual("%PDF-"u8))
        { contentType = "application/pdf"; return true; }
        if (bytes.Length >= 8 && bytes[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
        { contentType = "image/png"; return true; }
        if (bytes.Length >= 3 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff)
        { contentType = "image/jpeg"; return true; }
        contentType = string.Empty;
        return false;
    }
}
