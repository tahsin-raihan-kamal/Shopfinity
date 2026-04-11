using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Shopfinity.API.Responses;

namespace Shopfinity.API.Middleware;

public class CsrfValidationMiddleware
{
    private readonly RequestDelegate _next;

    public CsrfValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value?.ToLower() ?? "";

        // Server-to-server calls from the Next.js BFF do not send X-XSRF-Token; only browser→API via axios does.
        bool isAuthEndpoint =
            path.Contains("/auth/login")
            || path.Contains("/auth/register")
            || path.Contains("/auth/refresh")
            || path.Contains("/auth/logout")
            || path.EndsWith("/csrf");

        // 1. Skip CSRF validation for safe methods, but ensure the cookie is set
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method) || isAuthEndpoint)
        {
            // If the cookie is missing, create a new one.
            if (!context.Request.Cookies.ContainsKey("XSRF-TOKEN"))
            {
                var token = Guid.NewGuid().ToString("N");
                var isDev = context.Request.Host.Host == "localhost" || context.Request.Host.Host == "127.0.0.1";
                
                context.Response.Cookies.Append("XSRF-TOKEN", token, new CookieOptions
                {
                    HttpOnly = false, // MUST be false so frontend JS can read it
                    Secure   = !isDev, // false for localhost HTTP
                    SameSite = SameSiteMode.Lax,
                    Path     = "/"
                });
            }

            await _next(context);
            return;
        }

        // 2. For mutating requests, validate CSRF
        var cookieToken = context.Request.Cookies["XSRF-TOKEN"];
        var headerToken = context.Request.Headers["X-XSRF-Token"].FirstOrDefault();

        if (string.IsNullOrEmpty(cookieToken) || string.IsNullOrEmpty(headerToken) || cookieToken != headerToken)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var response = new ApiErrorResponse
            {
                Success = false,
                Message = "CSRF Token validation failed. Please refresh the page.",
                StatusCode = StatusCodes.Status403Forbidden
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            return;
        }

        await _next(context);
    }
}
