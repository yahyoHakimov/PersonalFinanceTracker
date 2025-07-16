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
            try
            {
                // Ensure database is created
                await context.Database.EnsureCreatedAsync();

                // Seed Users first
                if (!await context.Users.AnyAsync())
                {
                    await SeedUsersAsync(context);
                }

                // Seed Categories only if users exist
                if (!await context.Categories.AnyAsync())
                {
                    await SeedCategoriesAsync(context);
                }

                // Seed Sample Transactions
                if (!await context.Transactions.AnyAsync())
                {
                    await SeedTransactionsAsync(context);
                }

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the error but don't throw to prevent app startup failure
                Console.WriteLine($"Error seeding database: {ex.Message}");
                throw;
            }
        }

        private static async Task SeedUsersAsync(ApplicationDbContext context)
        {
            try
            {
                var users = new List<User>
            {
                new User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "testuser",
                    Email = "testuser@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "demo",
                    Email = "demo@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!"),
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow
                }
            };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();

                Console.WriteLine("✅ Users seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding users: {ex.Message}");
                throw;
            }
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            try
            {
                // Find the test user - use FirstOrDefaultAsync to avoid exception
                var testUser = await context.Users
                    .FirstOrDefaultAsync(u => u.Username == "testuser");

                if (testUser == null)
                {
                    Console.WriteLine("⚠️  Test user not found, skipping category seeding");
                    return;
                }

                var categories = new List<Category>
            {
                new Category
                {
                    Name = "Oziq-ovqat",
                    Color = "#FF6B6B",
                    UserId = testUser.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Transport",
                    Color = "#4ECDC4",
                    UserId = testUser.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Ko'ngilochar",
                    Color = "#45B7D1",
                    UserId = testUser.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Kommunal",
                    Color = "#FFA07A",
                    UserId = testUser.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Kiyim",
                    Color = "#98D8C8",
                    UserId = testUser.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Maosh",
                    Color = "#F7DC6F",
                    UserId = testUser.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Bonus",
                    Color = "#BB8FCE",
                    UserId = testUser.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Sog'liqni saqlash",
                    Color = "#85C1E9",
                    UserId = testUser.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Ta'lim",
                    Color = "#F8C471",
                    UserId = testUser.Id,
                    CreatedAt = DateTime.UtcNow
                }
            };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                Console.WriteLine("✅ Categories seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding categories: {ex.Message}");
                throw;
            }
        }

        private static async Task SeedTransactionsAsync(ApplicationDbContext context)
        {
            try
            {
                // Find the test user
                var testUser = await context.Users
                    .FirstOrDefaultAsync(u => u.Username == "testuser");

                if (testUser == null)
                {
                    Console.WriteLine("⚠️  Test user not found, skipping transaction seeding");
                    return;
                }

                // Get categories for the test user
                var categories = await context.Categories
                    .Where(c => c.UserId == testUser.Id)
                    .ToListAsync();

                if (!categories.Any())
                {
                    Console.WriteLine("⚠️  No categories found for test user, skipping transaction seeding");
                    return;
                }

                var transactions = new List<Transaction>();
                var random = new Random();

                // Create some sample transactions for the current month
                var currentMonth = DateTime.UtcNow;
                var startOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);

                // Add some income transactions
                var incomeCategories = categories.Where(c => c.Name == "Maosh" || c.Name == "Bonus").ToList();
                if (incomeCategories.Any())
                {
                    transactions.AddRange(new[]
                    {
                    new Transaction
                    {
                        Amount = 5000000, // 5 million UZS
                        Type = TransactionType.Income,
                        CategoryId = incomeCategories.First(c => c.Name == "Maosh").Id,
                        UserId = testUser.Id,
                        Note = "Oylik maosh",
                        CreatedAt = startOfMonth.AddDays(1)
                    },
                    new Transaction
                    {
                        Amount = 1000000, // 1 million UZS
                        Type = TransactionType.Income,
                        CategoryId = incomeCategories.First(c => c.Name == "Bonus").Id,
                        UserId = testUser.Id,
                        Note = "Ish faolligi uchun bonus",
                        CreatedAt = startOfMonth.AddDays(15)
                    }
                });
                }

                // Add some expense transactions
                var expenseCategories = categories.Where(c => c.Name != "Maosh" && c.Name != "Bonus").ToList();
                for (int i = 0; i < 20; i++)
                {
                    var category = expenseCategories[random.Next(expenseCategories.Count)];
                    var amount = random.Next(50000, 500000); // 50k to 500k UZS
                    var day = random.Next(1, Math.Min(28, DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month)));

                    transactions.Add(new Transaction
                    {
                        Amount = amount,
                        Type = TransactionType.Expense,
                        CategoryId = category.Id,
                        UserId = testUser.Id,
                        Note = $"{category.Name} uchun xarajat",
                        CreatedAt = new DateTime(currentMonth.Year, currentMonth.Month, day)
                    });
                }

                await context.Transactions.AddRangeAsync(transactions);
                await context.SaveChangesAsync();

                

                Console.WriteLine($"✅ {transactions.Count} transactions seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding transactions: {ex.Message}");
                throw;
            }
        }
    }
}
