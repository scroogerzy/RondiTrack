namespace RondiTrack.Exceptions;

/// <summary>
/// Thrown when a request violates a business rule.
/// </summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message)
        : base(message)
    {
    }
}