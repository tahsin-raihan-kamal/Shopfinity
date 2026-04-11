namespace Shopfinity.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(string userId, string email, IList<string> roles);
    string GenerateRefreshToken();
}
