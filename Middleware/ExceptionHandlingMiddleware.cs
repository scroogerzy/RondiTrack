
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RondiTrack.Exceptions;

namespace RondiTrack.Middleware;

/// <summary>
/// Handles application and database exceptions centrally.
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
            await HandleExceptionAsync(context, exception);
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

        var postgresException = FindPostgresException(exception);

        var (statusCode, title, type, detail) = exception switch
        {
            DbUpdateConcurrencyException =>
                (
                    StatusCodes.Status409Conflict,
                    "Conflict",
                    "https://api.ronditrack.co.za/errors/conflict",
                    "The resource was changed by another request. Reload it and retry."
                ),

            NotFoundException =>
                (
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    "https://api.ronditrack.co.za/errors/not-found",
                    exception.Message
                ),

            ConflictException =>
                (
                    StatusCodes.Status409Conflict,
                    "Conflict",
                    "https://api.ronditrack.co.za/errors/conflict",
                    exception.Message
                ),

            BusinessRuleException =>
                (
                    StatusCodes.Status422UnprocessableEntity,
                    "Unprocessable Entity",
                    "https://api.ronditrack.co.za/errors/unprocessable-entity",
                    exception.Message
                ),

            // PostgreSQL is the final authority for unique constraints.
            // EF Core may wrap its PostgreSQL exception inside DbUpdateException.
            _ when postgresException?.SqlState
                == PostgresErrorCodes.UniqueViolation =>
                (
                    StatusCodes.Status409Conflict,
                    "Conflict",
                    "https://api.ronditrack.co.za/errors/conflict",
                    "A record with these values already exists."
                ),

            _ =>
                (
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "https://api.ronditrack.co.za/errors/internal-server-error",
                    "An unexpected error occurred."
                )
        };

        context.Response.Clear();
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["correlationId"] = correlationId;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem));
    }

    private static PostgresException? FindPostgresException(
        Exception exception)
    {
        for (Exception? current = exception;
             current is not null;
             current = current.InnerException)
        {
            if (current is PostgresException postgresException)
            {
                return postgresException;
            }
        }

        return null;
    }
}
