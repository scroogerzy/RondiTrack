namespace RondiTrack.Models;

// User is an ENTITY, not a DTO.
// It has identity (Id) and behaviour (update methods).
// EF Core also needs a private constructor so it can materialize
// existing database rows without bypassing the public domain constructor.
public class User
{
    public Guid Id { get; private set; }
    public uint Version { get; private set; }

    public string FullName { get; private set; } = null!;

    public string Email { get; private set; } = null!;

// A User can participate in multiple stokvels.
// StokvelMember stores the additional data about each membership.

public ICollection<StokvelMember> StokvelMemberships { get; private set; }
    = new List<StokvelMember>();
    public User(string fullName, string email)
    {
        // Invalid users should never exist.
        // Enforce rules at construction time.

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException(
                "Full name is required.",
                nameof(fullName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "Email is required.",
                nameof(email));

        // Basic email validation for Week 4.
        if (!email.Contains("@") || !email.Contains("."))
            throw new ArgumentException(
                "Email address is invalid.",
                nameof(email));

        Id = Guid.NewGuid();
        FullName = fullName;
        Email = email;
    }

    // EF Core uses this constructor when materializing
    // an existing User row from PostgreSQL.
    private User()
    {
    }

    // Updates must go through domain behaviour,
    // not direct property setters.
    public void UpdateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException(
                "Full name is required.",
                nameof(fullName));

        FullName = fullName;
    }

    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "Email is required.",
                nameof(email));

        if (!email.Contains("@") || !email.Contains("."))
            throw new ArgumentException(
                "Email address is invalid.",
                nameof(email));

        Email = email;
    }
}