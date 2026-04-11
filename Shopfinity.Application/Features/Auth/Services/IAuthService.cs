using Shopfinity.Application.Features.Auth.DTOs;
using System.Threading;

namespace Shopfinity.Application.Features.Auth.Services;

/// <summary>
/// Handles registration, login, and refresh token rotation in the Application layer.
/// No Infrastructure types are referenced here.
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto, CancellationToken ct = default);
    Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default);
}
