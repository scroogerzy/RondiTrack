using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RondiTrack.Domain.Exceptions;
using RondiTrack.Domain.Models;
using RondiTrack.Domain.Repositories;
using RondiTrack.DTOs.Auth;

namespace RondiTrack.Auth;

public sealed record AuthResult(TokenResponse Tokens, string RefreshToken, DateTime RefreshTokenExpiresAt);

public sealed class AuthService
{
    private static readonly string UnknownUserHash =
        new PasswordHasher<User>().HashPassword(null!, "unknown-user-timing-equaliser");

    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly TokenService _tokenService;
    private readonly JwtOptions _options;
    private readonly TimeProvider _clock;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository users, IRefreshTokenRepository refreshTokens,
        IPasswordHasher<User> passwordHasher, TokenService tokenService,
        IOptions<JwtOptions> options, TimeProvider clock, ILogger<AuthService> logger)
    {
        _users = users; _refreshTokens = refreshTokens; _passwordHasher = passwordHasher;
        _tokenService = tokenService; _options = options.Value; _clock = clock; _logger = logger;
    }

    private DateTime Now => _clock.GetUtcNow().UtcDateTime;
    private TimeSpan RefreshLifetime => TimeSpan.FromDays(_options.RefreshTokenDays);

    public async Task<User> RegisterAsync(RegisterRequest request)
    {
        var normalizedEmail = User.NormalizeEmail(request.Email);
        if (await _users.GetByEmailAsync(normalizedEmail) is not null)
            throw new RondiTrack.Domain.Exceptions.ConflictException("An account with this email already exists.");

        var user = new User(request.FullName, normalizedEmail);
        user.SetPasswordHash(_passwordHasher.HashPassword(user, request.Password));
        return await _users.AddAsync(user);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var user = await _users.GetByEmailAsync(request.Email);
        var result = user?.PasswordHash is null
            ? _passwordHasher.VerifyHashedPassword(null!, UnknownUserHash, request.Password)
            : _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (user is null || user.PasswordHash is null || result == PasswordVerificationResult.Failed)
            throw new AuthenticationFailedException("Email or password is incorrect.");

        var (rawToken, tokenHash) = _tokenService.CreateRefreshToken();
        var refreshToken = RefreshToken.StartFamily(user.Id, tokenHash, Now, RefreshLifetime);
        await _refreshTokens.AddAsync(refreshToken);
        _logger.LogInformation("Authentication login succeeded.");
        return Issue(user, rawToken, refreshToken);
    }

    public async Task<AuthResult> RefreshAsync(string? rawRefreshToken)
    {
        if (string.IsNullOrEmpty(rawRefreshToken))
            throw new AuthenticationFailedException("Refresh token is missing. Please sign in again.");
        var now = Now;
        var current = await _refreshTokens.GetByHashAsync(TokenService.HashRefreshToken(rawRefreshToken))
            ?? throw new AuthenticationFailedException("Refresh token is not recognised.");

        if (current.RevokedAt is not null)
        {
            await _refreshTokens.RevokeFamilyAsync(current.FamilyId, now);
            throw new AuthenticationFailedException("Refresh token was already used. Please sign in again.");
        }
        if (!current.IsActive(now))
            throw new AuthenticationFailedException("Refresh token has expired. Please sign in again.");

        var user = await _users.GetByIdAsync(current.UserId)
            ?? throw new AuthenticationFailedException("This account no longer exists.");
        var (rawToken, tokenHash) = _tokenService.CreateRefreshToken();
        var successor = current.Rotate(tokenHash, now, RefreshLifetime);
        await _refreshTokens.SaveRotationAsync(successor);
        _logger.LogInformation("Authentication refresh succeeded.");
        return Issue(user, rawToken, successor);
    }

    public async Task LogoutAsync(string? rawRefreshToken)
    {
        if (string.IsNullOrEmpty(rawRefreshToken)) return;
        var current = await _refreshTokens.GetByHashAsync(TokenService.HashRefreshToken(rawRefreshToken));
        if (current is not null) await _refreshTokens.RevokeFamilyAsync(current.FamilyId, Now);
        _logger.LogInformation("Authentication logout completed.");
    }

    private AuthResult Issue(User user, string rawToken, RefreshToken refreshToken)
    {
        var access = _tokenService.CreateAccessToken(user);
        return new AuthResult(new TokenResponse(access.Token, access.ExpiresAt), rawToken, refreshToken.ExpiresAt);
    }
}
