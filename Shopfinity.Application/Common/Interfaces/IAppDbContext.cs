using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Shopfinity.Domain.Entities;
using System.Data;
using System.Threading;

namespace Shopfinity.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Category>     Categories    { get; set; }
    DbSet<Product>      Products      { get; set; }
    DbSet<Cart>         Carts         { get; set; }
    DbSet<CartItem>     CartItems     { get; set; }
    DbSet<Order>        Orders        { get; set; }
    DbSet<OrderItem>    OrderItems    { get; set; }
    DbSet<RefreshToken> RefreshTokens { get; set; }
    DbSet<WishlistItem> WishlistItems { get; set; }
    DbSet<ProductReview> ProductReviews { get; set; }

    ChangeTracker ChangeTracker { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a DB transaction. Returns null on in-memory providers (tests).
    /// </summary>
    Task<IDbContextTransaction?> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
}
