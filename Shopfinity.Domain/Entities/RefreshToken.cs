using Shopfinity.Domain.Common;

namespace Shopfinity.Domain.Entities;

/// <summary>
/// Stores issued refresh tokens for JWT token rotation.
/// Kept in the Domain layer so Application and Infrastructure can both reference it cleanly.
/// </summary>
public class RefreshToken : BaseEntity
{
    public string Token      { get; set; } = string.Empty;
    public string UserId     { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked    { get; set; }
}
