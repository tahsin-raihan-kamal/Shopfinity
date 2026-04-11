namespace Shopfinity.Domain.Enums;

/// <summary>
/// Shared order status enum — serialized as string via JsonStringEnumConverter.
/// Mirrored on the frontend in types/index.ts.
/// </summary>
public enum OrderStatus
{
    Pending    = 0,
    Processing = 1,
    Shipped    = 2,
    Delivered  = 3,
    Cancelled  = 4
}
