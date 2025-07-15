using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Infrastructure.Data.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Users.AnyAsync())
            {
                await SeedUsersAsync(context);
            }

            if (!await context.Categories.AnyAsync())
            {
                await SeedCategoriesAsync(context);
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedUsersAsync(ApplicationDbContext context)
        {
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            var testUser = new User
            {
                Username = "testuser",
                Email = "testuser@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddRangeAsync(adminUser, testUser);
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            var user = await context.Users.FirstAsync(u => u.Username == "testuser");

            var categories = new List<Category>
        {
            new Category { Name = "Oziq-ovqat", Color = "#FF6B6B", UserId = user.Id },
            new Category { Name = "Transport", Color = "#4ECDC4", UserId = user.Id },
            new Category { Name = "Ko'ngilochar", Color = "#45B7D1", UserId = user.Id },
            new Category { Name = "Kommunal", Color = "#FFA07A", UserId = user.Id },
            new Category { Name = "Kiyim", Color = "#98D8C8", UserId = user.Id },
            new Category { Name = "Maosh", Color = "#F7DC6F", UserId = user.Id },
            new Category { Name = "Bonus", Color = "#BB8FCE", UserId = user.Id }
        };

            await context.Categories.AddRangeAsync(categories);
        }
    }
}
