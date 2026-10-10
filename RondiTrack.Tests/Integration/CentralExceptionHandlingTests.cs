using System.IO;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using RondiTrack.Middleware;

namespace RondiTrack.Tests.Integration;

// Mirrors Matric Compass Week 5 Day 3's Unique_violation_becomes_409 test:
// feed the exact EF DbUpdateException/PostgresException shape into central handling.
public sealed class CentralExceptionHandlingTests
{
    [Fact]
    public async Task Unique_violation_becomes_409_problem_json_not_500()
    {
        var databaseError = new PostgresException(
            "duplicate key value",
            "ERROR",
            "ERROR",
            PostgresErrorCodes.UniqueViolation);
        RequestDelegate next = _ => Task.FromException(
            new DbUpdateException("save failed", databaseError));
        var middleware = new ExceptionHandlingMiddleware(
            next,
            NullLogger<ExceptionHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Request.Path = "/assignment53/test";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.Conflict, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        using var problem = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal(409, problem.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Conflict", problem.RootElement.GetProperty("title").GetString());
    }
}
