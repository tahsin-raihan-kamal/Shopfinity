using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shopfinity.Application.Common.Interfaces;
using Shopfinity.Application.Features.Auth.DTOs;
using Shopfinity.Application.Features.Auth.Services;
using Shopfinity.Domain.Entities;
using System.Threading;

namespace Shopfinity.Infrastructure.Identity;


public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator           _tokenGenerator;
    private readonly IAppDbContext                _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator           tokenGenerator,
        IAppDbContext                context)
    {
        _userManager    = userManager;
        _tokenGenerator = tokenGenerator;
        _context        = context;
    }



    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            throw new InvalidOperationException("Email is already in use.");

        var user = new ApplicationUser
        {
            UserName  = dto.Email,
            Email     = dto.Email,
            FirstName = dto.FirstName,
            LastName  = dto.LastName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, "Customer");
        return await BuildAuthResponseAsync(user, ct);
    }


    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return await BuildAuthResponseAsync(user, ct);
    }



    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto, CancellationToken ct = default)
    {
        var stored = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == dto.RefreshToken && !t.IsRevoked, ct);

        if (stored == null || stored.ExpiryDate < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token is invalid or has expired.");

    
        stored.IsRevoked = true;

        var user = await _userManager.FindByIdAsync(stored.UserId)
                   ?? throw new InvalidOperationException("User associated with token not found.");

        
        return await BuildAuthResponseAsync(user, ct);
    }



    public async Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(refreshToken)) return;

        var stored = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, ct);

        if (stored != null)
        {
            stored.IsRevoked = true;
            await _context.SaveChangesAsync(ct);
        }
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user, CancellationToken ct = default)
    {
        // Revoke all other active tokens for this user to prevent accumulation
        var activeTokens = await _context.RefreshTokens
            .Where(t => t.UserId == user.Id && !t.IsRevoked && t.ExpiryDate > DateTime.UtcNow)
            .ToListAsync(ct);
            
        foreach (var t in activeTokens) t.IsRevoked = true;

        var roles       = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenGenerator.GenerateToken(user.Id, user.Email!, roles);
        var rawRefresh  = _tokenGenerator.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token      = rawRefresh,
            UserId     = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        });

        await _context.SaveChangesAsync(ct);

        return new AuthResponseDto
        {
            Token        = accessToken,
            RefreshToken = rawRefresh,
            Email        = user.Email!,
            Roles        = roles
        };
    }
}
