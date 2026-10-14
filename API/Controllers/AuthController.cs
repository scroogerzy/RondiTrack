using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RondiTrack.Auth;
using RondiTrack.Common;
using RondiTrack.DTOs.Auth;

namespace RondiTrack.Controllers;

[ApiController]
[Route(ApiRoutes.Auth)]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private const string RefreshCookieName = "refresh_token";
    private readonly AuthService _authService;
    public AuthController(AuthService authService) => _authService = authService;

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterAsync(RegisterRequest request)
    {
        var user = await _authService.RegisterAsync(request);
        return StatusCode(StatusCodes.Status201Created, new { user.Id, user.FullName, user.Email });
    }

    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    public async Task<ActionResult<TokenResponse>> LoginAsync(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(result.Tokens);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponse>> RefreshAsync()
    {
        var result = await _authService.RefreshAsync(Request.Cookies[RefreshCookieName]);
        SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(result.Tokens);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        await _authService.LogoutAsync(Request.Cookies[RefreshCookieName]);
        Response.Cookies.Delete(RefreshCookieName, CookieOptionsFor(null));
        return NoContent();
    }

    private void SetRefreshCookie(string rawToken, DateTime expiresAt) =>
        Response.Cookies.Append(RefreshCookieName, rawToken, CookieOptionsFor(expiresAt));

    private static CookieOptions CookieOptionsFor(DateTime? expires) => new()
    {
        HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Path = "/" + ApiRoutes.Auth, Expires = expires
    };
}
