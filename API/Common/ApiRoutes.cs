namespace RondiTrack.Common;

/// <summary>Canonical API route prefixes. Controllers must not duplicate the version string.</summary>
public static class ApiRoutes
{
    public const string V1 = "api/v1";
    public const string Auth = V1 + "/auth";
    public const string Stokvels = V1 + "/stokvels";
    public const string Users = V1 + "/users";
}
