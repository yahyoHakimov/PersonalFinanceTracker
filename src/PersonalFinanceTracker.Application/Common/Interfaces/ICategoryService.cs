using PersonalFinanceTracker.Application.Common.DTOs.Categories;
using PersonalFinanceTracker.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Interfaces
{
    public interface ICategoryService
    {
        Task<Result<CategoryDto>> GetByIdAsync(int id, int userId, CancellationToken cancellationToken = default);
        Task<Result<PagedResult<CategoryDto>>> GetPagedAsync(int userId, CategoryFilterRequest request, CancellationToken cancellationToken = default);
        Task<Result<List<CategoryDto>>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<Result<CategoryDto>> CreateAsync(int userId, CreateCategoryRequest request, CancellationToken cancellationToken = default);
        Task<Result<CategoryDto>> UpdateAsync(int userId, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
        Task<Result<bool>> DeleteAsync(int id, int userId, CancellationToken cancellationToken = default);
    }
}
