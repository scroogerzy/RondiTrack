namespace RondiTrack.Middleware;

/// <summary>
/// Adds a correlation id to every request.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        var correlationId =
            context.Request.Headers[HeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items[HeaderName] = correlationId;

        context.Response.Headers[HeaderName] = correlationId;

        await _next(context);
    }
}