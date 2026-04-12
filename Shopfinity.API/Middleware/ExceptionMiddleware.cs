using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shopfinity.API.Responses;
using Shopfinity.Application.Common.Exceptions;
using System.Security.Authentication;
using System.Text.Json;
using ValidationException = FluentValidation.ValidationException;

namespace Shopfinity.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during the request.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var errors = new List<string>();
        int statusCode;
        string detail;

        switch (exception)
        {
            // ── Validation ────────────────────────────────────────────────────
            case ValidationException validationException:
                statusCode = StatusCodes.Status400BadRequest;
                detail = "One or more validation errors occurred.";
                errors.AddRange(validationException.Errors.Select(e => e.ErrorMessage));
                break;

            case InvalidOperationException invalidOpException:
                statusCode = StatusCodes.Status400BadRequest;
                detail = invalidOpException.Message;
                break;

            // ── Conflict ──────────────────────────────────────────────────────
            case ConflictException conflict:
                statusCode = StatusCodes.Status409Conflict;
                detail = conflict.Message;
                break;

            case DbUpdateConcurrencyException:
                statusCode = StatusCodes.Status409Conflict;
                detail = "The resource was modified by another request. Please try again.";
                break;

            // ── Not Found ─────────────────────────────────────────────────────
            case KeyNotFoundException knf:
                statusCode = StatusCodes.Status404NotFound;
                detail = string.IsNullOrWhiteSpace(knf.Message)
                    ? "The requested resource was not found."
                    : knf.Message;
                break;

            // ── Auth ──────────────────────────────────────────────────────────
            case UnauthorizedAccessException:
                statusCode = StatusCodes.Status401Unauthorized;
                detail = "You are not authorized to access this resource.";
                break;

            case AuthenticationException:
                statusCode = StatusCodes.Status403Forbidden;
                detail = "You do not have permission to perform this action.";
                break;

            // ── Timeout / Cancel ──────────────────────────────────────────────
            case OperationCanceledException:
                statusCode = StatusCodes.Status503ServiceUnavailable;
                detail = "The request was canceled or timed out at the server. Please try again.";
                break;

            // ── Database Connection ───────────────────────────────────────────
            case NpgsqlException:
                statusCode = StatusCodes.Status500InternalServerError;
                detail = "A database connection error occurred. Please try again later.";
                break;

            case DbUpdateException:
                statusCode = StatusCodes.Status500InternalServerError;
                detail = "A database operation failed. Please try again later.";
                break;

            // ── Catch-All ─────────────────────────────────────────────────────
            default:
                statusCode = StatusCodes.Status500InternalServerError;
                detail = "An unexpected error occurred.";
                break;
        }

        context.Response.StatusCode = statusCode;

        var apiErrorResponse = new ApiErrorResponse
        {
            Success = false,
            Message = detail,
            StatusCode = statusCode,
            Errors = errors
        };

        var json = JsonSerializer.Serialize(apiErrorResponse,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        return context.Response.WriteAsync(json);
    }
}
