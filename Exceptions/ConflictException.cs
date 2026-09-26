namespace RondiTrack.Exceptions;

/// <summary>
/// Thrown when an operation conflicts with existing state.
/// </summary>
public sealed class ConflictException : BusinessRuleException
{
    public ConflictException(string message)
        : base(message)
    {
    }
}