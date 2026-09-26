namespace RondiTrack.Exceptions;

/// <summary>
/// Base exception for all business/domain failures.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message)
    {
    }
}