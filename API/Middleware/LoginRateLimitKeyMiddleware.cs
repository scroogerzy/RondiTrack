using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RondiTrack.Middleware;

/// <summary>
/// Gives each login identity its own rate-limit partition without storing the raw
/// email address in HttpContext, logs, or rate-limiter keys.
/// </summary>
public sealed class LoginRateLimitKeyMiddleware
{
    public const string ItemKey = "LoginRateLimitPartition";
    private const int MaximumBodyBytes = 64 * 1024;
    private readonly RequestDelegate _next;

    public LoginRateLimitKeyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (HttpMethods.IsPost(context.Request.Method) &&
            context.Request.Path.Value?.EndsWith("/login", StringComparison.OrdinalIgnoreCase) == true &&
            context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                if (context.Request.ContentLength is null or <= MaximumBodyBytes)
                {
                    context.Request.EnableBuffering(bufferThreshold: 16 * 1024, bufferLimit: MaximumBodyBytes);
                    using var document = await JsonDocument.ParseAsync(
                        context.Request.Body,
                        cancellationToken: context.RequestAborted);

                    if (document.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        string? email = null;
                        foreach (var property in document.RootElement.EnumerateObject())
                        {
                            if (property.Name.Equals("email", StringComparison.OrdinalIgnoreCase) ||
                                property.Name.Equals("username", StringComparison.OrdinalIgnoreCase))
                            {
                                email = property.Value.ValueKind == JsonValueKind.String
                                    ? property.Value.GetString()
                                    : null;
                                break;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(email) && email.Length <= 320)
                        {
                            var normalized = email.Trim().ToLowerInvariant();
                            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
                            context.Items[ItemKey] = $"login:{hash}";
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is JsonException or IOException or InvalidOperationException)
            {
                // Malformed or oversized input falls back to the network partition.
            }
            finally
            {
                if (context.Request.Body.CanSeek)
                    context.Request.Body.Position = 0;
            }
        }

        await _next(context);
    }
}
