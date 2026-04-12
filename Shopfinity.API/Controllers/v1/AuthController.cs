using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shopfinity.API.Responses;
using Shopfinity.Application.Features.Auth.DTOs;
using Shopfinity.Application.Features.Auth.Services;
using System.Threading;

namespace Shopfinity.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[EnableRateLimiting("AuthRatePolicy")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Register a new customer account.</summary>
    [HttpPost("register")]
    [EnableRateLimiting("AuthRatePolicy")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(dto, ct);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Registration successful."));
    }

    /// <summary>Authenticate and receive an access + refresh token pair.</summary>
    [HttpPost("login")]
    [EnableRateLimiting("AuthRatePolicy")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(dto, ct);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Login successful."));
    }

    /// <summary>Exchange a valid refresh token for a new access + refresh token pair.</summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Refresh([FromBody] RefreshTokenRequestDto dto, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(dto, ct);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Token refreshed."));
    }

    /// <summary>Initialize or refresh the CSRF token for the frontend.</summary>
    [HttpGet("csrf")]
    [AllowAnonymous]
    public ActionResult<ApiResponse<object>> GetCsrfToken()
    {
        // The CsrfValidationMiddleware automatically sets the XSRF-TOKEN cookie 
        // since this is a GET request to a safe endpoint.
        return Ok(ApiResponse<object>.SuccessResponse(new {}, "CSRF token initialized."));
    }

    /// <summary>Revoke the refresh token on logout and clear cookies.</summary>
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] RefreshTokenRequestDto dto, CancellationToken ct)
    {
        await _authService.RevokeTokenAsync(dto.RefreshToken, ct);
        
        // Clear all related auth and security cookies
        var isDev = Request.Host.Host == "localhost";
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure   = !isDev,
            SameSite = SameSiteMode.Lax,
            Expires  = DateTime.UtcNow.AddDays(-1),
            Path     = "/"
        };
        
        Response.Cookies.Delete("shopfinity_token", cookieOptions);
        Response.Cookies.Delete("shopfinity_refresh", cookieOptions);
        
        // Clear non-httponly CSRF cookie
        Response.Cookies.Delete("XSRF-TOKEN", new CookieOptions { Path = "/", Expires = DateTime.UtcNow.AddDays(-1) });

        return Ok(ApiResponse<object>.SuccessResponse(new {}, "Logged out successfully."));
    }
}
