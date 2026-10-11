namespace RondiTrack.Services;

/// <summary>
/// Represents the possible outcomes of adding a user to a stokvel.
/// </summary>
public abstract record AddMemberResult
{
    /// <summary>
    /// Indicates that the member was added successfully.
    /// </summary>
    public sealed record Added : AddMemberResult;

    /// <summary>
    /// Indicates that the stokvel could not be found.
    /// </summary>
    public sealed record StokvelNotFound(Guid StokvelId)
        : AddMemberResult;

    /// <summary>
    /// Indicates that the user could not be found.
    /// </summary>
    public sealed record UserNotFound(Guid UserId)
        : AddMemberResult;

    /// <summary>
    /// Indicates that the user is already a member.
    /// </summary>
    public sealed record AlreadyMember(Guid UserId)
        : AddMemberResult;

    /// <summary>
    /// Indicates that the stokvel has reached its membership limit.
    /// </summary>
    public sealed record MembershipLimitReached
        : AddMemberResult;
}