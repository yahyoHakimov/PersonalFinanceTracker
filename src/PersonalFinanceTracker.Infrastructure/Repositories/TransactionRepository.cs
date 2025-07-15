using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Application.Common.DTOs.Transactions;
using PersonalFinanceTracker.Application.Common.Interfaces;
using PersonalFinanceTracker.Application.Common.Models;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Domain.Enums;
using PersonalFinanceTracker.Domain.ValueObjects;
using PersonalFinanceTracker.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction?> GetByIdAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);
        }

        public async Task<Transaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<PagedResult<Transaction>> GetPagedAsync(int userId, TransactionFilterRequest request, CancellationToken cancellationToken = default)
        {
            var query = _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(t => t.Note!.ToLower().Contains(request.SearchTerm.ToLower()) ||
                                       t.Category.Name.ToLower().Contains(request.SearchTerm.ToLower()));
            }

            if (request.Type.HasValue)
            {
                query = query.Where(t => t.Type == request.Type.Value);
            }

            if (request.CategoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == request.CategoryId.Value);
            }

            if (request.MinAmount.HasValue)
            {
                query = query.Where(t => t.Amount >= request.MinAmount.Value);
            }

            if (request.MaxAmount.HasValue)
            {
                query = query.Where(t => t.Amount <= request.MaxAmount.Value);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt <= request.EndDate.Value);
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "amount" => request.SortDescending ? query.OrderByDescending(t => t.Amount) : query.OrderBy(t => t.Amount),
                "type" => request.SortDescending ? query.OrderByDescending(t => t.Type) : query.OrderBy(t => t.Type),
                "category" => request.SortDescending ? query.OrderByDescending(t => t.Category.Name) : query.OrderBy(t => t.Category.Name),
                "created_at" => request.SortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
                _ => query.OrderByDescending(t => t.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync(cancellationToken);

            return new PagedResult<Transaction>(items, totalCount, request.PageNumber, request.PageSize);
        }

        public async Task<List<Transaction>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Transaction>> GetByUserIdAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);
            return transaction;
        }

        public async Task<Transaction> UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
        {
            transaction.UpdatedAt = DateTime.UtcNow;
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync(cancellationToken);
            return transaction;
        }

        public async Task<bool> DeleteAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            var transaction = await GetByIdAsync(id, userId, cancellationToken);
            if (transaction == null) return false;

            transaction.IsDeleted = true;
            transaction.DeletedAt = DateTime.UtcNow;
            await UpdateAsync(transaction, cancellationToken);
            return true;
        }

        public async Task<MonthlyBalance> GetMonthlyBalanceAsync(int userId, DateTime month, CancellationToken cancellationToken = default)
        {
            var startDate = new DateTime(month.Year, month.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var totalIncome = await _context.Transactions
                .Where(t => t.UserId == userId && t.Type == TransactionType.Income &&
                           t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .SumAsync(t => t.Amount, cancellationToken);

            var totalExpense = await _context.Transactions
                .Where(t => t.UserId == userId && t.Type == TransactionType.Expense &&
                           t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .SumAsync(t => t.Amount, cancellationToken);

            return new MonthlyBalance(totalIncome, totalExpense, month);
        }

        public async Task<List<MonthlyBalance>> GetMonthlyBalancesAsync(int userId, DateTime startMonth, DateTime endMonth, CancellationToken cancellationToken = default)
        {
            var balances = new List<MonthlyBalance>();
            var current = new DateTime(startMonth.Year, startMonth.Month, 1);
            var end = new DateTime(endMonth.Year, endMonth.Month, 1);

            while (current <= end)
            {
                var balance = await GetMonthlyBalanceAsync(userId, current, cancellationToken);
                balances.Add(balance);
                current = current.AddMonths(1);
            }

            return balances;
        }

        public async Task<List<CategoryExpense>> GetCategoryExpensesAsync(int userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var categoryExpenses = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && t.Type == TransactionType.Expense &&
                           t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .GroupBy(t => new { t.CategoryId, t.Category.Name })
                .Select(g => new
                {
                    CategoryName = g.Key.Name,
                    TotalAmount = g.Sum(t => t.Amount),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToListAsync(cancellationToken);

            var totalExpense = categoryExpenses.Sum(c => c.TotalAmount);

            return categoryExpenses.Select(c => new CategoryExpense(
                c.CategoryName,
                c.TotalAmount,
                c.TransactionCount,
                totalExpense > 0 ? (c.TotalAmount / totalExpense) * 100 : 0
            )).ToList();
        }

        public async Task<decimal> GetTotalIncomeAsync(int userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId && t.Type == TransactionType.Income &&
                           t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .SumAsync(t => t.Amount, cancellationToken);
        }

        public async Task<decimal> GetTotalExpenseAsync(int userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId && t.Type == TransactionType.Expense &&
                           t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .SumAsync(t => t.Amount, cancellationToken);
        }

        public async Task<int> GetTransactionCountAsync(int userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .CountAsync(t => t.UserId == userId && t.CreatedAt >= startDate && t.CreatedAt <= endDate, cancellationToken);
        }
    }
}
