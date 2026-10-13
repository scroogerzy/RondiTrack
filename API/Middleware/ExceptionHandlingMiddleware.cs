using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RondiTrack.Domain.Exceptions;

namespace RondiTrack.Middleware;

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
            var postgres = FindPostgresException(exception);
            var (status, title, type, detail) = exception switch
            {
                DbUpdateConcurrencyException =>
                    (409, "Conflict", "conflict",
                    "The resource changed. Reload it and retry."),
                AuthenticationFailedException =>
                    (401, "Unauthorized", "unauthorized",
                    "Authentication failed."),
                NotFoundException =>
                    (404, "Not Found", "not-found",
                    "The requested resource was not found."),
                ConflictException =>
                    (409, "Conflict", "conflict",
                    "The request conflicts with existing data."),
                BusinessRuleException =>
                    (422, "Unprocessable Entity", "unprocessable-entity",
                    "The request could not be processed."),
                _ when postgres?.SqlState == PostgresErrorCodes.UniqueViolation =>
                    (409, "Conflict", "conflict",
                    "A record with these values already exists."),
                _ =>
                    (500, "Internal Server Error", "internal-server-error",
                    "An unexpected error occurred.")
            };

            var traceId = Activity.Current?.TraceId.ToString()
                ?? context.TraceIdentifier;

            if (status >= 500)
            {
                _logger.LogError(exception,
                    "Request failed with status {StatusCode}. TraceId: {TraceId}",
                    status, traceId);
            }
            else
            {
                _logger.LogInformation(
                    "Request rejected with status {StatusCode}; exception type {ExceptionType}. TraceId: {TraceId}",
                    status, exception.GetType().Name, traceId);
            }

            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Type = $"https://api.ronditrack.co.za/errors/{type}",
                Title = title,
                Status = status,
                Detail = detail,
                Instance = context.Request.Path
            };
            problem.Extensions["correlationId"] = traceId;

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problem));
        }
    }

    private static PostgresException? FindPostgresException(Exception exception)
    {
        for (Exception? current = exception; current is not null;
             current = current.InnerException)
        {
            if (current is PostgresException postgres)
                return postgres;
        }

        return null;
    }
}
