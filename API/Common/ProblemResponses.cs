using Microsoft.AspNetCore.Mvc;

namespace RondiTrack.Common;

/// <summary>
/// Provides consistent RFC 9457 Problem Details responses for API errors.
/// </summary>
public static class ProblemResponses
{
    /// <summary>
    /// Creates a 400 Bad Request response.
    /// </summary>
    public static ObjectResult BadRequest(
        string detail,
        string instance)
    {
        return Problem(
            type: "https://api.ronditrack.co.za/errors/bad-request",
            title: "Bad Request",
            status: StatusCodes.Status400BadRequest,
            detail: detail,
            instance: instance);
    }

    /// <summary>
    /// Creates a 404 Not Found response.
    /// </summary>
    public static ObjectResult NotFound(
        string detail,
        string instance)
    {
        return Problem(
            type: "https://api.ronditrack.co.za/errors/not-found",
            title: "Not Found",
            status: StatusCodes.Status404NotFound,
            detail: detail,
            instance: instance);
    }

    /// <summary>
    /// Creates a 409 Conflict response.
    /// </summary>
    public static ObjectResult Conflict(
        string detail,
        string instance)
    {
        return Problem(
            type: "https://api.ronditrack.co.za/errors/conflict",
            title: "Conflict",
            status: StatusCodes.Status409Conflict,
            detail: detail,
            instance: instance);
    }

    /// <summary>
    /// Creates a 422 Unprocessable Entity response.
    /// </summary>
    public static ObjectResult UnprocessableEntity(
        string detail,
        string instance)
    {
        return Problem(
            type: "https://api.ronditrack.co.za/errors/unprocessable-entity",
            title: "Unprocessable Entity",
            status: StatusCodes.Status422UnprocessableEntity,
            detail: detail,
            instance: instance);
    }

    /// <summary>
    /// Creates a Problem Details response with the supplied HTTP information.
    /// </summary>
    private static ObjectResult Problem(
        string type,
        string title,
        int status,
        string detail,
        string instance)
    {
        return new ObjectResult(new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = status,
            Detail = detail,
            Instance = instance
        })
        {
            StatusCode = status
        };
    }
}