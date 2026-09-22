using RondiTrack.DTOs.Contributions;
using RondiTrack.DTOs.Stokvels;
using RondiTrack.DTOs.Users;
using RondiTrack.Models;

namespace RondiTrack.Mappings;

/// <summary>
/// Provides explicit mappings between domain entities and response DTOs.
/// </summary>
public static class DomainMappings
{
    /// <summary>
    /// Maps a User entity to a UserResponse DTO.
    /// </summary>
    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email);
    }

    /// <summary>
    /// Maps a Stokvel entity to a StokvelResponse DTO.
    /// </summary>
    public static StokvelResponse ToResponse(
        this Stokvel stokvel)
    {
        return new StokvelResponse(
            stokvel.Id,
            stokvel.Name,
            stokvel.MonthlyContribution,
            stokvel.MemberIds.Count);
    }

    /// <summary>
    /// Maps a Contribution entity to a ContributionResponse DTO.
    /// </summary>
    public static ContributionResponse ToResponse(
        this Contribution contribution)
    {
        return new ContributionResponse(
            contribution.Id,
            contribution.StokvelId,
            contribution.UserId,
            contribution.Cycle,
            contribution.Amount,
            contribution.RecordedAt);
    }
}