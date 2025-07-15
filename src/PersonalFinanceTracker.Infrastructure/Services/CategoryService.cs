using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PersonalFinanceTracker.Application.Common.DTOs.Categories;
using PersonalFinanceTracker.Application.Common.Interfaces;
using PersonalFinanceTracker.Application.Common.Models;
using PersonalFinanceTracker.Domain.Constants;
using PersonalFinanceTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuditService _auditService;
        private readonly IDistributedCache _cache;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IAuditService auditService,
            IDistributedCache cache,
            IMapper mapper,
            ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _auditService = auditService;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CategoryDto>> GetByIdAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id, userId, cancellationToken);
                if (category == null)
                {
                    return Result<CategoryDto>.Failure("Category not found");
                }

                var result = _mapper.Map<CategoryDto>(category);

                // Calculate statistics
                result.TransactionCount = await _categoryRepository.GetTransactionCountAsync(id, cancellationToken);
                result.TotalAmount = await _categoryRepository.GetTotalAmountAsync(id, cancellationToken);

                return Result<CategoryDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category {CategoryId} for user {UserId}", id, userId);
                return Result<CategoryDto>.Failure("Failed to retrieve category");
            }
        }

        public async Task<Result<PagedResult<CategoryDto>>> GetPagedAsync(int userId, CategoryFilterRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var categories = await _categoryRepository.GetPagedAsync(userId, request, cancellationToken);
                var categoryDtos = new List<CategoryDto>();

                foreach (var category in categories.Items)
                {
                    var dto = _mapper.Map<CategoryDto>(category);

                    if (request.IncludeStatistics)
                    {
                        dto.TransactionCount = category.Transactions.Count;
                        dto.TotalAmount = category.Transactions.Sum(t => t.Amount);
                    }

                    categoryDtos.Add(dto);
                }

                var result = new PagedResult<CategoryDto>(
                    categoryDtos,
                    categories.TotalCount,
                    categories.PageNumber,
                    categories.PageSize);

                return Result<PagedResult<CategoryDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged categories for user {UserId}", userId);
                return Result<PagedResult<CategoryDto>>.Failure("Failed to retrieve categories");
            }
        }

        public async Task<Result<List<CategoryDto>>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var categories = await _categoryRepository.GetByUserIdAsync(userId, cancellationToken);
                var result = _mapper.Map<List<CategoryDto>>(categories);

                return Result<List<CategoryDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories for user {UserId}", userId);
                return Result<List<CategoryDto>>.Failure("Failed to retrieve categories");
            }
        }

        public async Task<Result<CategoryDto>> CreateAsync(int userId, CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return Result<CategoryDto>.Failure("Category name is required");
                }

                if (request.Name.Length > 100) // Assuming max length
                {
                    return Result<CategoryDto>.Failure("Category name is too long (maximum 100 characters)");
                }

                // Add timeout for the name existence check
                try
                {
                    if (await _categoryRepository.IsNameExistsAsync(request.Name, userId, null, cancellationToken))
                    {
                        return Result<CategoryDto>.Failure("Category with this name already exists");
                    }
                }
                catch (TimeoutException)
                {
                    _logger.LogWarning("Timeout when checking category name existence for user {UserId}", userId);
                    return Result<CategoryDto>.Failure("Operation timed out. Please try again.");
                }

                var category = new Category
                {
                    Name = request.Name.Trim(),
                    Color = request.Color ?? "#000000",
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                var createdCategory = await _categoryRepository.CreateAsync(category, cancellationToken);
                var result = _mapper.Map<CategoryDto>(createdCategory);

                // Log audit (with timeout protection)
                try
                {
                    await _auditService.LogAsync(userId, AuditActions.CREATE, EntityNames.CATEGORY,
                        createdCategory.Id, null, result, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Audit logging was cancelled for category creation {CategoryId}", createdCategory.Id);
                    // Don't fail the entire operation if audit logging fails
                }

                _logger.LogInformation("Category {CategoryName} created for user {UserId}", request.Name, userId);
                return Result<CategoryDto>.Success(result);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Category creation was cancelled for user {UserId}", userId);
                return Result<CategoryDto>.Failure("Operation was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category {CategoryName} for user {UserId}", request.Name, userId);
                return Result<CategoryDto>.Failure("Failed to create category");
            }
        }

        public async Task<Result<CategoryDto>> UpdateAsync(int userId, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(request.Id, userId, cancellationToken);
                if (category == null)
                {
                    return Result<CategoryDto>.Failure("Category not found");
                }

                if (await _categoryRepository.IsNameExistsAsync(request.Name, userId, request.Id, cancellationToken))
                {
                    return Result<CategoryDto>.Failure("Category with this name already exists");
                }

                var oldValue = _mapper.Map<CategoryDto>(category);

                category.Name = request.Name.Trim();
                category.Color = request.Color;

                var updatedCategory = await _categoryRepository.UpdateAsync(category, cancellationToken);
                var result = _mapper.Map<CategoryDto>(updatedCategory);

                // Log audit
                await _auditService.LogAsync(userId, AuditActions.UPDATE, EntityNames.CATEGORY,
                    category.Id, oldValue, result, cancellationToken);

                _logger.LogInformation("Category {CategoryId} updated for user {UserId}", request.Id, userId);
                return Result<CategoryDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId} for user {UserId}", request.Id, userId);
                return Result<CategoryDto>.Failure("Failed to update category");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id, userId, cancellationToken);
                if (category == null)
                {
                    return Result<bool>.Failure("Category not found");
                }

                if (await _categoryRepository.HasTransactionsAsync(id, cancellationToken))
                {
                    return Result<bool>.Failure("Cannot delete category with existing transactions");
                }

                var deleted = await _categoryRepository.DeleteAsync(id, userId, cancellationToken);
                if (!deleted)
                {
                    return Result<bool>.Failure("Failed to delete category");
                }

                // Log audit
                var categoryDto = _mapper.Map<CategoryDto>(category);
                await _auditService.LogAsync(userId, AuditActions.DELETE, EntityNames.CATEGORY,
                    id, categoryDto, null, cancellationToken);

                _logger.LogInformation("Category {CategoryId} deleted for user {UserId}", id, userId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId} for user {UserId}", id, userId);
                return Result<bool>.Failure("Failed to delete category");
            }
        }
    }
}
