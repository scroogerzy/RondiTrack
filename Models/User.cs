namespace RondiTrack.Models;

// User is an ENTITY, not a DTO.
// It has identity (Id) and behaviour (update methods).
public class User
{
    public Guid Id { get; }

    public string FullName { get; private set; }

    public string Email { get; private set; }

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