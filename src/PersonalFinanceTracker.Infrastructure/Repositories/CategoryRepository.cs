using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Application.Common.DTOs.Categories;
using PersonalFinanceTracker.Application.Common.Interfaces;
using PersonalFinanceTracker.Application.Common.Models;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Include(c => c.Transactions)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);
        }

        public async Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Include(c => c.Transactions)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<PagedResult<Category>> GetPagedAsync(int userId, CategoryFilterRequest request, CancellationToken cancellationToken = default)
        {
            var query = _context.Categories
                .Where(c => c.UserId == userId);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(c => c.Name.Contains(request.SearchTerm));
            }

            // Simple sorting
            if (request.SortDescending)
            {
                query = query.OrderByDescending(c => c.Name);
            }
            else
            {
                query = query.OrderBy(c => c.Name);
            }

            if (request.IncludeStatistics)
            {
                query = query.Include(c => c.Transactions);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync(cancellationToken);

            return new PagedResult<Category>(items, totalCount, request.PageNumber, request.PageSize);
        }

        public async Task<List<Category>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsNameExistsAsync(string name, int userId, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Categories
                .Where(c => c.Name.ToLower() == name.ToLower() && c.UserId == userId);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync(cancellationToken);
            return category;
        }

        public async Task<Category> UpdateAsync(Category category, CancellationToken cancellationToken = default)
        {
            category.UpdatedAt = DateTime.UtcNow;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync(cancellationToken);
            return category;
        }

        public async Task<bool> DeleteAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            var category = await GetByIdAsync(id, userId, cancellationToken);
            if (category == null) return false;

            category.IsDeleted = true;
            category.DeletedAt = DateTime.UtcNow;
            await UpdateAsync(category, cancellationToken);
            return true;
        }

        public async Task<bool> HasTransactionsAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .AnyAsync(t => t.CategoryId == categoryId, cancellationToken);
        }

        public async Task<int> GetTransactionCountAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .CountAsync(t => t.CategoryId == categoryId, cancellationToken);
        }

        public async Task<decimal> GetTotalAmountAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Where(t => t.CategoryId == categoryId)
                .SumAsync(t => t.Amount, cancellationToken);
        }
    }
}
