using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RondiTrack.Domain.Models;

namespace RondiTrack.Auth;

public sealed record AccessToken(string Token, DateTime ExpiresAt);

public sealed class TokenService
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _clock;
    private readonly SigningCredentials _credentials;

    public TokenService(IOptions<JwtOptions> options, TimeProvider clock)
    {
        _options = options.Value; _clock = clock;
        _credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);
    }

    public AccessToken CreateAccessToken(User user)
    {
        var now = _clock.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(_options.AccessTokenMinutes);
        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
            [JwtRegisteredClaimNames.Email] = user.Email,
            [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
            ["role"] = user.PlatformRole
        };
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer, Audience = _options.Audience, Claims = claims,
            IssuedAt = now, NotBefore = now, Expires = expires, SigningCredentials = _credentials
        };
        return new AccessToken(new JsonWebTokenHandler().CreateToken(descriptor), expires);
    }

    public (string RawToken, string TokenHash) CreateRefreshToken()
    {
        var raw = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        return (raw, HashRefreshToken(raw));
    }

    public static string HashRefreshToken(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
