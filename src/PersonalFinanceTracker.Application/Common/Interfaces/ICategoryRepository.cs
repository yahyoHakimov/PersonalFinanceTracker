using PersonalFinanceTracker.Application.Common.DTOs.Categories;
using PersonalFinanceTracker.Application.Common.Models;
using PersonalFinanceTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id, int userId, CancellationToken cancellationToken = default);
        Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedResult<Category>> GetPagedAsync(int userId, CategoryFilterRequest request, CancellationToken cancellationToken = default);
        Task<List<Category>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> IsNameExistsAsync(string name, int userId, int? excludeId = null, CancellationToken cancellationToken = default);
        Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default);
        Task<Category> UpdateAsync(Category category, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, int userId, CancellationToken cancellationToken = default);
        Task<bool> HasTransactionsAsync(int categoryId, CancellationToken cancellationToken = default);
        Task<int> GetTransactionCountAsync(int categoryId, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalAmountAsync(int categoryId, CancellationToken cancellationToken = default);
    }
}
