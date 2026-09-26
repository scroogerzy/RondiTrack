namespace RondiTrack.Exceptions;

/// <summary>
/// Thrown when a requested resource cannot be found.
/// </summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}