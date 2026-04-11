using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shopfinity.Domain.Entities;
using Shopfinity.Infrastructure.Identity;
using System.Text.Json;

namespace Shopfinity.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        await context.Database.MigrateAsync();

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new IdentityRole("User"));
        if (!await roleManager.RoleExistsAsync("Customer"))
            await roleManager.CreateAsync(new IdentityRole("Customer"));

        if (await userManager.FindByEmailAsync("test@shopfinity.com") == null)
        {
            var user = new ApplicationUser
            {
                UserName = "test@shopfinity.com",
                Email = "test@shopfinity.com",
                FirstName = "Test",
                LastName = "User",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, "Password123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, "User");
        }

        const string adminEmail = "admin@shopfinity.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        const string customerEmail = "mhdnazrul511@gmail.com";
        var customerUser = await userManager.FindByEmailAsync(customerEmail);
        if (customerUser == null)
        {
            customerUser = new ApplicationUser
            {
                UserName = customerEmail,
                Email = customerEmail,
                FirstName = "Mhd",
                LastName = "Nazrul",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(customerUser, "Mhd12345");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(customerUser, "User");
        }

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
}
