using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RondiTrack.Exceptions;

namespace RondiTrack.Middleware;

/// <summary>
/// Handles application exceptions centrally and returns RFC 9457-style
/// Problem Details responses.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(
                context,
                exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var correlationId =
            context.Items["X-Correlation-ID"]?.ToString()
            ?? Guid.NewGuid().ToString();

        _logger.LogError(
            exception,
            "Request failed. CorrelationId: {CorrelationId}",
            correlationId);

        var (statusCode, title, type) = exception switch
        {
            NotFoundException =>
                (
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    "https://api.ronditrack.co.za/errors/not-found"
                ),

            ConflictException =>
                (
                    StatusCodes.Status409Conflict,
                    "Conflict",
                    "https://api.ronditrack.co.za/errors/conflict"
                ),

            BusinessRuleException =>
                (
                    StatusCodes.Status422UnprocessableEntity,
                    "Unprocessable Entity",
                    "https://api.ronditrack.co.za/errors/unprocessable-entity"
                ),

            _ =>
                (
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "https://api.ronditrack.co.za/errors/internal-server-error"
                )
        };

        context.Response.ContentType =
            "application/problem+json";

        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = statusCode,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        problem.Extensions["correlationId"] =
            correlationId;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem));
    }
}