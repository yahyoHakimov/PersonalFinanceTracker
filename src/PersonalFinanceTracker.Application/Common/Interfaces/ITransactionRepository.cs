using PersonalFinanceTracker.Application.Common.DTOs.Transactions;
using PersonalFinanceTracker.Application.Common.Models;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(int id, int userId, CancellationToken cancellationToken = default);
        Task<Transaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedResult<Transaction>> GetPagedAsync(int userId, TransactionFilterRequest request, CancellationToken cancellationToken = default);
        Task<List<Transaction>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<List<Transaction>> GetByUserIdAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default);
        Task<Transaction> UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, int userId, CancellationToken cancellationToken = default);
        Task<MonthlyBalance> GetMonthlyBalanceAsync(int userId, DateTime month, CancellationToken cancellationToken = default);
        Task<List<MonthlyBalance>> GetMonthlyBalancesAsync(int userId, DateTime startMonth, DateTime endMonth, CancellationToken cancellationToken = default);
        Task<List<CategoryExpense>> GetCategoryExpensesAsync(int userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalIncomeAsync(int userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalExpenseAsync(int userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<int> GetTransactionCountAsync(int userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
