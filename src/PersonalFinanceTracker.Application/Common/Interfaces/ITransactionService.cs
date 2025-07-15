using PersonalFinanceTracker.Application.Common.DTOs.Transactions;
using PersonalFinanceTracker.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Interfaces
{
    public interface ITransactionService
    {
        Task<Result<TransactionDto>> GetByIdAsync(int id, int userId, CancellationToken cancellationToken = default);
        Task<Result<PagedResult<TransactionDto>>> GetPagedAsync(int userId, TransactionFilterRequest request, CancellationToken cancellationToken = default);
        Task<Result<List<TransactionDto>>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<Result<TransactionDto>> CreateAsync(int userId, CreateTransactionRequest request, CancellationToken cancellationToken = default);
        Task<Result<TransactionDto>> UpdateAsync(int userId, UpdateTransactionRequest request, CancellationToken cancellationToken = default);
        Task<Result<bool>> DeleteAsync(int id, int userId, CancellationToken cancellationToken = default);
        Task<Result<MonthlyBalanceDto>> GetMonthlyBalanceAsync(int userId, DateTime month, CancellationToken cancellationToken = default);
        Task<Result<List<MonthlyBalanceDto>>> GetMonthlyBalancesAsync(int userId, DateTime startMonth, DateTime endMonth, CancellationToken cancellationToken = default);
    }
}
