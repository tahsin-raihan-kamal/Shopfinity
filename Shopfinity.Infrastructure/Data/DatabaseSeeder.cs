using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopfinity.Domain.Entities;
using Shopfinity.Infrastructure.Identity;
using System.Text.Json;

namespace Shopfinity.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger? logger = null)
    {
        // Apply pending migrations
        await context.Database.MigrateAsync();

        // ── Seed Roles ────────────────────────────────────────────────────────
        string[] roles = ["Admin", "User", "Customer"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger?.LogInformation("Created role: {Role}", role);
            }
        }

        // ── Seed Admin Account ────────────────────────────────────────────────
        await SeedUserAsync(userManager, new SeedUserData
        {
            Email = "admin@shopfinity.com",
            FirstName = "Admin",
            LastName = "User",
            Password = "Admin123!",
            Role = "Admin"
        }, logger);

        // ── Seed Tester Admin Account ─────────────────────────────────────────
        await SeedUserAsync(userManager, new SeedUserData
        {
            Email = "tester@shopfinity.com",
            FirstName = "Tester",
            LastName = "Admin",
            Password = "Tester123!",
            Role = "Admin"
        }, logger);

        // ── Seed Test User Account ────────────────────────────────────────────
        await SeedUserAsync(userManager, new SeedUserData
        {
            Email = "test@shopfinity.com",
            FirstName = "Test",
            LastName = "User",
            Password = "Password123!",
            Role = "User"
        }, logger);

        // ── Seed Customer Account ─────────────────────────────────────────────
        await SeedUserAsync(userManager, new SeedUserData
        {
            Email = "mhdnazrul511@gmail.com",
            FirstName = "Mhd",
            LastName = "Nazrul",
            Password = "Mhd@12345",
            Role = "User"
        }, logger);

        // ── Seed Products ─────────────────────────────────────────────────────
        var seedPath = ResolveSeedDataPath();
        if (!await context.Products.AnyAsync() && seedPath != null)
        {
            var json = await File.ReadAllTextAsync(seedPath);
            using var doc = JsonDocument.Parse(json);

            var prods = doc.RootElement.GetProperty("Products").EnumerateArray()
                .Select(p => new Product
                {
                    Name = p.GetProperty("Name").GetString()!,
                    Slug = p.GetProperty("Slug").GetString()!,
                    Price = p.GetProperty("Price").GetDecimal(),
                    StockQuantity = p.GetProperty("StockQuantity").GetInt32(),
                    CategoryId = Guid.Parse(p.GetProperty("CategoryId").GetString()!),
                    ImageUrl = p.GetProperty("ImageUrl").GetString()!,
                    Description = p.GetProperty("Description").GetString()!
                });
            await context.Products.AddRangeAsync(prods);
            await context.SaveChangesAsync();
            logger?.LogInformation("Seeded {Count} products from seed data.", prods.Count());
        }
    }

    private static async Task SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        SeedUserData data,
        ILogger? logger)
    {
        var existing = await userManager.FindByEmailAsync(data.Email);
        if (existing != null)
        {
            logger?.LogInformation("User {Email} already exists, skipping.", data.Email);
            return;
        }

        var user = new ApplicationUser
        {
            UserName = data.Email,
            Email = data.Email,
            FirstName = data.FirstName,
            LastName = data.LastName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, data.Password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, data.Role);
            logger?.LogInformation("Created user {Email} with role {Role}.", data.Email, data.Role);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger?.LogWarning("Failed to create user {Email}: {Errors}", data.Email, errors);
        }
    }

    private static string? ResolveSeedDataPath()
    {
        foreach (var dir in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            var path = Path.Combine(dir, "SeedData", "products.json");
            if (File.Exists(path))
                return path;
        }
        return null;
    }

    private sealed class SeedUserData
    {
        public required string Email { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string Password { get; init; }
        public required string Role { get; init; }
    }
}
