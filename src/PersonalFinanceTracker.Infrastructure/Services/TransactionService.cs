using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PersonalFinanceTracker.Application.Common.DTOs.Transactions;
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
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuditService _auditService;
        private readonly IDistributedCache _cache;
        private readonly IMapper _mapper;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ITransactionRepository transactionRepository,
            ICategoryRepository categoryRepository,
            IAuditService auditService,
            IDistributedCache cache,
            IMapper mapper,
            ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _auditService = auditService;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<TransactionDto>> GetByIdAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var transaction = await _transactionRepository.GetByIdAsync(id, userId, cancellationToken);
                if (transaction == null)
                {
                    return Result<TransactionDto>.Failure("Transaction not found");
                }

                var result = _mapper.Map<TransactionDto>(transaction);
                return Result<TransactionDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction {TransactionId} for user {UserId}", id, userId);
                return Result<TransactionDto>.Failure("Failed to retrieve transaction");
            }
        }

        public async Task<Result<PagedResult<TransactionDto>>> GetPagedAsync(int userId, TransactionFilterRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var transactions = await _transactionRepository.GetPagedAsync(userId, request, cancellationToken);
                var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions.Items);

                var result = new PagedResult<TransactionDto>(
                    transactionDtos,
                    transactions.TotalCount,
                    transactions.PageNumber,
                    transactions.PageSize);

                return Result<PagedResult<TransactionDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged transactions for user {UserId}", userId);
                return Result<PagedResult<TransactionDto>>.Failure("Failed to retrieve transactions");
            }
        }

        public async Task<Result<List<TransactionDto>>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var transactions = await _transactionRepository.GetByUserIdAsync(userId, cancellationToken);
                var result = _mapper.Map<List<TransactionDto>>(transactions);

                return Result<List<TransactionDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions for user {UserId}", userId);
                return Result<List<TransactionDto>>.Failure("Failed to retrieve transactions");
            }
        }

        public async Task<Result<TransactionDto>> CreateAsync(int userId, CreateTransactionRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate category belongs to user
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId, userId, cancellationToken);
                if (category == null)
                {
                    return Result<TransactionDto>.Failure("Invalid category");
                }

                var transaction = new Transaction
                {
                    Amount = request.Amount,
                    Type = request.Type,
                    CategoryId = request.CategoryId,
                    UserId = userId,
                    Note = request.Note?.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                var createdTransaction = await _transactionRepository.CreateAsync(transaction, cancellationToken);

                // Load category for mapping
                createdTransaction.Category = category;

                var result = _mapper.Map<TransactionDto>(createdTransaction);

                // Log audit
                await _auditService.LogAsync(userId, AuditActions.CREATE, EntityNames.TRANSACTION,
                    createdTransaction.Id, null, result, cancellationToken);

                // Invalidate related caches
                await InvalidateUserCachesAsync(userId, cancellationToken);

                _logger.LogInformation("Transaction created for user {UserId}, amount {Amount}", userId, request.Amount);
                return Result<TransactionDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction for user {UserId}", userId);
                return Result<TransactionDto>.Failure("Failed to create transaction");
            }
        }

        public async Task<Result<TransactionDto>> UpdateAsync(int userId, UpdateTransactionRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var transaction = await _transactionRepository.GetByIdAsync(request.Id, userId, cancellationToken);
                if (transaction == null)
                {
                    return Result<TransactionDto>.Failure("Transaction not found");
                }

                // Check optimistic concurrency
                if (!transaction.RowVersion.SequenceEqual(request.RowVersion))
                {
                    return Result<TransactionDto>.Failure("Transaction was modified by another user. Please refresh and try again.");
                }

                // Validate category belongs to user
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId, userId, cancellationToken);
                if (category == null)
                {
                    return Result<TransactionDto>.Failure("Invalid category");
                }

                var oldValue = _mapper.Map<TransactionDto>(transaction);

                // Update properties
                transaction.Amount = request.Amount;
                transaction.Type = request.Type;
                transaction.CategoryId = request.CategoryId;
                transaction.Note = request.Note?.Trim();

                var updatedTransaction = await _transactionRepository.UpdateAsync(transaction, cancellationToken);

                // Load category for mapping
                updatedTransaction.Category = category;

                var result = _mapper.Map<TransactionDto>(updatedTransaction);

                // Log audit
                await _auditService.LogAsync(userId, AuditActions.UPDATE, EntityNames.TRANSACTION,
                    transaction.Id, oldValue, result, cancellationToken);

                // Invalidate related caches
                await InvalidateUserCachesAsync(userId, cancellationToken);

                _logger.LogInformation("Transaction {TransactionId} updated for user {UserId}", request.Id, userId);
                return Result<TransactionDto>.Success(result);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result<TransactionDto>.Failure("Transaction was modified by another user. Please refresh and try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction {TransactionId} for user {UserId}", request.Id, userId);
                return Result<TransactionDto>.Failure("Failed to update transaction");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var transaction = await _transactionRepository.GetByIdAsync(id, userId, cancellationToken);
                if (transaction == null)
                {
                    return Result<bool>.Failure("Transaction not found");
                }

                var deleted = await _transactionRepository.DeleteAsync(id, userId, cancellationToken);
                if (!deleted)
                {
                    return Result<bool>.Failure("Failed to delete transaction");
                }

                // Log audit
                var transactionDto = _mapper.Map<TransactionDto>(transaction);
                await _auditService.LogAsync(userId, AuditActions.DELETE, EntityNames.TRANSACTION,
                    id, transactionDto, null, cancellationToken);

                // Invalidate related caches
                await InvalidateUserCachesAsync(userId, cancellationToken);

                _logger.LogInformation("Transaction {TransactionId} deleted for user {UserId}", id, userId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction {TransactionId} for user {UserId}", id, userId);
                return Result<bool>.Failure("Failed to delete transaction");
            }
        }

        public async Task<Result<MonthlyBalanceDto>> GetMonthlyBalanceAsync(int userId, DateTime month, CancellationToken cancellationToken = default)
        {
            try
            {
                // Try cache first
                var cacheKey = $"monthly_balance:{userId}:{month:yyyy-MM}";
                var cachedBalance = await _cache.GetStringAsync(cacheKey, cancellationToken);

                if (!string.IsNullOrEmpty(cachedBalance))
                {
                    var balanceDto = JsonSerializer.Deserialize<MonthlyBalanceDto>(cachedBalance);
                    if (balanceDto != null)
                    {
                        return Result<MonthlyBalanceDto>.Success(balanceDto);
                    }
                }

                var balance = await _transactionRepository.GetMonthlyBalanceAsync(userId, month, cancellationToken);
                var result = _mapper.Map<MonthlyBalanceDto>(balance);

                // Cache the result (cache for 1 hour if current month, 24 hours if past month)
                var cacheExpiry = month.Month == DateTime.UtcNow.Month && month.Year == DateTime.UtcNow.Year
                    ? TimeSpan.FromHours(1)
                    : TimeSpan.FromHours(24);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheExpiry
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), cacheOptions, cancellationToken);

                return Result<MonthlyBalanceDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly balance for user {UserId}, month {Month}", userId, month);
                return Result<MonthlyBalanceDto>.Failure("Failed to retrieve monthly balance");
            }
        }

        public async Task<Result<List<MonthlyBalanceDto>>> GetMonthlyBalancesAsync(int userId, DateTime startMonth, DateTime endMonth, CancellationToken cancellationToken = default)
        {
            try
            {
                var balances = await _transactionRepository.GetMonthlyBalancesAsync(userId, startMonth, endMonth, cancellationToken);
                var result = _mapper.Map<List<MonthlyBalanceDto>>(balances);

                return Result<List<MonthlyBalanceDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly balances for user {UserId}", userId);
                return Result<List<MonthlyBalanceDto>>.Failure("Failed to retrieve monthly balances");
            }
        }

        private async Task InvalidateUserCachesAsync(int userId, CancellationToken cancellationToken)
        {
            // Invalidate current month balance cache
            var currentMonth = DateTime.UtcNow;
            await _cache.RemoveAsync($"monthly_balance:{userId}:{currentMonth:yyyy-MM}", cancellationToken);

            // Invalidate category caches
            await _cache.RemoveAsync($"categories:user:{userId}", cancellationToken);
        }
    }
}
