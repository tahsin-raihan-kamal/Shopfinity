using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shopfinity.Application.Common.Interfaces;
using Shopfinity.Domain.Common;
using Shopfinity.Domain.Constants;
using Shopfinity.Domain.Entities;
using Shopfinity.Infrastructure.Identity;
using System.Data;
using System.Threading;

namespace Shopfinity.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }
    public DbSet<ProductReview> ProductReviews { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured) return;
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Critical for Identity

        // Global Query Filters for Soft Delete
        builder.Entity<Category>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Product>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<ProductReview>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<WishlistItem>().HasQueryFilter(x => !x.IsDeleted);

        // Category Configuration
        builder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Slug).IsUnique(); // Unique Slugs
        });

        // Product Configuration
        builder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.Name);
            entity.HasIndex(p => p.CategoryId);
            entity.HasIndex(p => p.Slug).IsUnique(); // Unique Slugs

            entity.Property(p => p.Name).IsRequired().HasMaxLength(500);
            entity.Property(p => p.Slug).IsRequired().HasMaxLength(500);
            entity.Property(p => p.Price).HasPrecision(18, 2);

            entity.Property(p => p.RowVersion)
                  .IsRowVersion();

            // Generated tsvector is PostgreSQL-only; in-memory tests use a fallback search in ProductService.
            if (Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                entity.HasGeneratedTsVectorColumn(
                    p => p.SearchVector,
                    "english",
                    p => new { p.Name, p.Description }
                ).HasIndex(p => p.SearchVector).HasMethod("GIN");
            }
            else
            {
                entity.Ignore(p => p.SearchVector);
            }
        });

        // ── DATABASE CONSTRAINT AUDIT: Cart & Order Hardening ──────────────────
        
        builder.Entity<CartItem>(entity =>
        {
            // PARANOID: One user can never have the same product duplicated as separate rows
            entity.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();
        });

        builder.Entity<Order>(entity =>
        {
            entity.Property(o => o.UserId).IsRequired();
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            
            // IDEMPOTENCY: Prevent duplicate checkouts
            entity.HasIndex(o => o.IdempotencyKey).IsUnique();
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
            
            // CASCADE: Deleting a product should be RESTRICTING to preserve history
            entity.HasOne(oi => oi.Product)
                  .WithMany()
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Phase 2: Wishlist & Reviews Configuration
        builder.Entity<WishlistItem>(entity =>
        {
            // A user can only wishlist a specific product once
            entity.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
        });

        builder.Entity<ProductReview>(entity =>
        {
            // A user can only review a product once
            entity.HasIndex(r => new { r.UserId, r.ProductId }).IsUnique();
            entity.HasCheckConstraint("CK_ProductReview_Rating", "\"Rating\" >= 1 AND \"Rating\" <= 5");
        });

        // Seed Roles
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "1", Name = AppRoles.Admin, NormalizedName = "ADMIN" },
            new IdentityRole { Id = "2", Name = AppRoles.Customer, NormalizedName = "CUSTOMER" }
        );

        // Seed Categories
        var catLaptops = new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Laptops", Slug = "laptops", Description = "High-performance laptops.", DisplayOrder = 1, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        var catPhones = new Category { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Smartphones", Slug = "smartphones", Description = "Latest flagship smartphones.", DisplayOrder = 2, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        var catAudio = new Category { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Audio", Slug = "audio", Description = "Premium headphones and speakers.", DisplayOrder = 3, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        var catWearables = new Category { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Wearables", Slug = "wearables", Description = "Smartwatches and fitness trackers.", DisplayOrder = 4, IsDeleted = false, CreatedAt = DateTime.UtcNow };

        builder.Entity<Category>().HasData(catLaptops, catPhones, catAudio, catWearables);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction?> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            return null;

        return await Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }
}
