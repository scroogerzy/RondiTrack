using System.Text;
using RondiTrack.Services.Assignment91;

namespace RondiTrack.Tests.Unit;

public sealed class ProofFileValidatorTests
{
    [Fact]
    public void AcceptsPdfSignature()
    {
        Assert.True(ProofFileValidator.TryGetContentType(Encoding.ASCII.GetBytes("%PDF-1.7"), out var type));
        Assert.Equal("application/pdf", type);
    }

    [Fact]
    public void RejectsTextRenamedAsPdf()
    {
        Assert.False(ProofFileValidator.TryGetContentType(Encoding.UTF8.GetBytes("This is only a text file."), out _));
    }

    [Fact]
    public void AcceptsPngSignature()
    {
        byte[] bytes = [137, 80, 78, 71, 13, 10, 26, 10];
        Assert.True(ProofFileValidator.TryGetContentType(bytes, out var type));
        Assert.Equal("image/png", type);
    }

    [Fact]
    public void AcceptsJpegSignature()
    {
        byte[] bytes = [0xff, 0xd8, 0xff, 0x00];
        Assert.True(ProofFileValidator.TryGetContentType(bytes, out var type));
        Assert.Equal("image/jpeg", type);
    }
}
