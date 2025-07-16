using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PersonalFinanceTracker.Application.Common.DTOs.Statistics;
using PersonalFinanceTracker.Application.Common.Interfaces;
using PersonalFinanceTracker.Application.Common.Models;
using PersonalFinanceTracker.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Infrastructure.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IDistributedCache _cache;
        private readonly IMapper _mapper;
        private readonly ILogger<StatisticsService> _logger;

        public StatisticsService(
            ITransactionRepository transactionRepository,
            ICategoryRepository categoryRepository,
            IBackgroundJobService backgroundJobService,
            IDistributedCache cache,
            IMapper mapper,
            ILogger<StatisticsService> logger)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _backgroundJobService = backgroundJobService;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CategoryExpenseStatisticsDto>> GetCategoryExpenseStatisticsAsync(
            int userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = $"category_expenses:{userId}:{startDate:yyyy-MM-dd}:{endDate:yyyy-MM-dd}";
                var cachedResult = await _cache.GetStringAsync(cacheKey, cancellationToken);

                if (!string.IsNullOrEmpty(cachedResult))
                {
                    var cached = JsonSerializer.Deserialize<CategoryExpenseStatisticsDto>(cachedResult);
                    if (cached != null)
                    {
                        return Result<CategoryExpenseStatisticsDto>.Success(cached);
                    }
                }

                var categoryExpenses = await _transactionRepository.GetCategoryExpensesAsync(
                    userId, startDate, endDate, cancellationToken);

                var totalExpenses = categoryExpenses.Sum(c => c.TotalAmount);
                var totalTransactions = categoryExpenses.Sum(c => c.TransactionCount);

                var categoryItems = categoryExpenses.Select(c => new CategoryExpenseItemDto
                {
                    CategoryName = c.CategoryName,
                    CategoryColor = "#FF6B6B", // You might want to get this from category
                    TotalAmount = c.TotalAmount,
                    TransactionCount = c.TransactionCount,
                    Percentage = c.Percentage,
                    AverageAmount = c.TransactionCount > 0 ? c.TotalAmount / c.TransactionCount : 0,
                    MaxAmount = c.TotalAmount, // You might want to calculate actual max
                    MinAmount = c.TotalAmount // You might want to calculate actual min
                }).ToList();

                var statistics = new CategoryExpenseStatisticsDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalExpenses = totalExpenses,
                    Categories = categoryItems,
                    TopCategory = categoryItems.FirstOrDefault(),
                    TotalTransactions = totalTransactions,
                    AverageTransactionAmount = totalTransactions > 0 ? totalExpenses / totalTransactions : 0
                };

                // Cache for 1 hour
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(statistics), cacheOptions, cancellationToken);

                return Result<CategoryExpenseStatisticsDto>.Success(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category expense statistics for user {UserId}", userId);
                return Result<CategoryExpenseStatisticsDto>.Failure("Failed to retrieve category expense statistics");
            }
        }

        public async Task<Result<TrendAnalysisDto>> GetTrendAnalysisAsync(
            int userId, int months, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = $"trend_analysis:{userId}:{months}";
                var cachedResult = await _cache.GetStringAsync(cacheKey, cancellationToken);

                if (!string.IsNullOrEmpty(cachedResult))
                {
                    var cached = JsonSerializer.Deserialize<TrendAnalysisDto>(cachedResult);
                    if (cached != null)
                    {
                        return Result<TrendAnalysisDto>.Success(cached);
                    }
                }

                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddMonths(-months);

                var monthlyBalances = await _transactionRepository.GetMonthlyBalancesAsync(
                    userId, startDate, endDate, cancellationToken);

                var monthlyTrends = new List<MonthlyTrendDto>();

                for (int i = 0; i < monthlyBalances.Count; i++)
                {
                    var current = monthlyBalances[i];
                    var previous = i > 0 ? monthlyBalances[i - 1] : null;

                    var trend = new MonthlyTrendDto
                    {
                        Month = current.Month,
                        MonthName = current.Month.ToString("MMMM yyyy"),
                        Income = current.TotalIncome,
                        Expenses = current.TotalExpense,
                        Balance = current.Balance,
                        IncomeGrowth = previous != null && previous.TotalIncome > 0
                            ? ((current.TotalIncome - previous.TotalIncome) / previous.TotalIncome) * 100
                            : 0,
                        ExpenseGrowth = previous != null && previous.TotalExpense > 0
                            ? ((current.TotalExpense - previous.TotalExpense) / previous.TotalExpense) * 100
                            : 0,
                        BalanceGrowth = previous != null && previous.Balance != 0
                            ? ((current.Balance - previous.Balance) / Math.Abs(previous.Balance)) * 100
                            : 0,
                        TransactionCount = await _transactionRepository.GetTransactionCountAsync(
                            userId,
                            new DateTime(current.Month.Year, current.Month.Month, 1),
                            new DateTime(current.Month.Year, current.Month.Month, 1).AddMonths(1).AddDays(-1),
                            cancellationToken)
                    };

                    monthlyTrends.Add(trend);
                }

                // Calculate trend summaries
                var currentMonth = monthlyBalances.LastOrDefault();
                var previousMonth = monthlyBalances.Count > 1 ? monthlyBalances[^2] : null;

                var incomeTrend = new TrendSummaryDto
                {
                    CurrentAmount = currentMonth?.TotalIncome ?? 0,
                    PreviousAmount = previousMonth?.TotalIncome ?? 0,
                    GrowthAmount = (currentMonth?.TotalIncome ?? 0) - (previousMonth?.TotalIncome ?? 0),
                    GrowthPercentage = previousMonth?.TotalIncome > 0
                        ? (((currentMonth?.TotalIncome ?? 0) - previousMonth.TotalIncome) / previousMonth.TotalIncome) * 100
                        : 0,
                    IsIncreasing = (currentMonth?.TotalIncome ?? 0) > (previousMonth?.TotalIncome ?? 0),
                    AverageMonthly = monthlyBalances.Average(m => m.TotalIncome),
                    HighestMonthly = monthlyBalances.Max(m => m.TotalIncome),
                    LowestMonthly = monthlyBalances.Min(m => m.TotalIncome)
                };

                var expenseTrend = new TrendSummaryDto
                {
                    CurrentAmount = currentMonth?.TotalExpense ?? 0,
                    PreviousAmount = previousMonth?.TotalExpense ?? 0,
                    GrowthAmount = (currentMonth?.TotalExpense ?? 0) - (previousMonth?.TotalExpense ?? 0),
                    GrowthPercentage = previousMonth?.TotalExpense > 0
                        ? (((currentMonth?.TotalExpense ?? 0) - previousMonth.TotalExpense) / previousMonth.TotalExpense) * 100
                        : 0,
                    IsIncreasing = (currentMonth?.TotalExpense ?? 0) > (previousMonth?.TotalExpense ?? 0),
                    AverageMonthly = monthlyBalances.Average(m => m.TotalExpense),
                    HighestMonthly = monthlyBalances.Max(m => m.TotalExpense),
                    LowestMonthly = monthlyBalances.Min(m => m.TotalExpense)
                };

                var balanceTrend = new TrendSummaryDto
                {
                    CurrentAmount = currentMonth?.Balance ?? 0,
                    PreviousAmount = previousMonth?.Balance ?? 0,
                    GrowthAmount = (currentMonth?.Balance ?? 0) - (previousMonth?.Balance ?? 0),
                    GrowthPercentage = previousMonth?.Balance != 0
                        ? (((currentMonth?.Balance ?? 0) - previousMonth.Balance) / Math.Abs(previousMonth.Balance)) * 100
                        : 0,
                    IsIncreasing = (currentMonth?.Balance ?? 0) > (previousMonth?.Balance ?? 0),
                    AverageMonthly = monthlyBalances.Average(m => m.Balance),
                    HighestMonthly = monthlyBalances.Max(m => m.Balance),
                    LowestMonthly = monthlyBalances.Min(m => m.Balance)
                };

                var trendAnalysis = new TrendAnalysisDto
                {
                    MonthsAnalyzed = months,
                    StartDate = startDate,
                    EndDate = endDate,
                    MonthlyTrends = monthlyTrends,
                    IncomeTrend = incomeTrend,
                    ExpenseTrend = expenseTrend,
                    BalanceTrend = balanceTrend,
                    CategoryTrends = new List<CategoryTrendDto>() // TODO: Implement category trends
                };

                // Cache for 2 hours
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(trendAnalysis), cacheOptions, cancellationToken);

                return Result<TrendAnalysisDto>.Success(trendAnalysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trend analysis for user {UserId}", userId);
                return Result<TrendAnalysisDto>.Failure("Failed to retrieve trend analysis");
            }
        }

        public async Task<Result<DashboardStatisticsDto>> GetDashboardStatisticsAsync(
            int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = $"dashboard_stats:{userId}:{DateTime.UtcNow:yyyy-MM-dd-HH}";
                var cachedResult = await _cache.GetStringAsync(cacheKey, cancellationToken);

                if (!string.IsNullOrEmpty(cachedResult))
                {
                    var cached = JsonSerializer.Deserialize<DashboardStatisticsDto>(cachedResult);
                    if (cached != null)
                    {
                        return Result<DashboardStatisticsDto>.Success(cached);
                    }
                }

                var currentDate = DateTime.UtcNow;
                var currentMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
                var previousMonth = currentMonth.AddMonths(-1);
                var currentYear = new DateTime(currentDate.Year, 1, 1);
                var previousYear = currentYear.AddYears(-1);

                // Get monthly balances
                var currentMonthBalance = await _transactionRepository.GetMonthlyBalanceAsync(userId, currentMonth, cancellationToken);
                var previousMonthBalance = await _transactionRepository.GetMonthlyBalanceAsync(userId, previousMonth, cancellationToken);

                // Get top categories (current month)
                var categoryExpenses = await _transactionRepository.GetCategoryExpensesAsync(
                    userId, currentMonth, currentMonth.AddMonths(1).AddDays(-1), cancellationToken);

                var topExpenseCategories = categoryExpenses.Take(5).Select(c => new CategoryExpenseItemDto
                {
                    CategoryName = c.CategoryName,
                    CategoryColor = "#FF6B6B",
                    TotalAmount = c.TotalAmount,
                    TransactionCount = c.TransactionCount,
                    Percentage = c.Percentage,
                    AverageAmount = c.TransactionCount > 0 ? c.TotalAmount / c.TransactionCount : 0
                }).ToList();

                // Get recent transactions
                var recentTransactions = await _transactionRepository.GetByUserIdAsync(userId, cancellationToken);
                var recentTransactionDtos = recentTransactions.Take(10).Select(t => new RecentTransactionDto
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    Type = t.Type.ToString(),
                    CategoryName = t.Category?.Name ?? "Unknown",
                    CategoryColor = t.Category?.Color ?? "#000000",
                    Note = t.Note,
                    CreatedAt = t.CreatedAt,
                    RelativeTime = GetRelativeTime(t.CreatedAt)
                }).ToList();

                // Generate insights
                var insights = GenerateFinancialInsights(currentMonthBalance, previousMonthBalance, categoryExpenses);

                var dashboard = new DashboardStatisticsDto
                {
                    GeneratedAt = DateTime.UtcNow,
                    CurrentMonth = new MonthlyStatsDto
                    {
                        Month = currentMonth,
                        TotalIncome = currentMonthBalance.TotalIncome,
                        TotalExpenses = currentMonthBalance.TotalExpense,
                        Balance = currentMonthBalance.Balance,
                        TransactionCount = await _transactionRepository.GetTransactionCountAsync(
                            userId, currentMonth, currentMonth.AddMonths(1).AddDays(-1), cancellationToken),
                        IncomeGrowth = previousMonthBalance.TotalIncome > 0
                            ? ((currentMonthBalance.TotalIncome - previousMonthBalance.TotalIncome) / previousMonthBalance.TotalIncome) * 100
                            : 0,
                        ExpenseGrowth = previousMonthBalance.TotalExpense > 0
                            ? ((currentMonthBalance.TotalExpense - previousMonthBalance.TotalExpense) / previousMonthBalance.TotalExpense) * 100
                            : 0,
                        BalanceGrowth = previousMonthBalance.Balance != 0
                            ? ((currentMonthBalance.Balance - previousMonthBalance.Balance) / Math.Abs(previousMonthBalance.Balance)) * 100
                            : 0
                    },
                    PreviousMonth = new MonthlyStatsDto
                    {
                        Month = previousMonth,
                        TotalIncome = previousMonthBalance.TotalIncome,
                        TotalExpenses = previousMonthBalance.TotalExpense,
                        Balance = previousMonthBalance.Balance,
                        TransactionCount = await _transactionRepository.GetTransactionCountAsync(
                            userId, previousMonth, previousMonth.AddMonths(1).AddDays(-1), cancellationToken)
                    },
                    TopExpenseCategories = topExpenseCategories,
                    RecentTransactions = recentTransactionDtos,
                    Insights = insights
                };

                // Cache for 1 hour
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dashboard), cacheOptions, cancellationToken);

                return Result<DashboardStatisticsDto>.Success(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard statistics for user {UserId}", userId);
                return Result<DashboardStatisticsDto>.Failure("Failed to retrieve dashboard statistics");
            }
        }

        public async Task<Result<ReportJobDto>> RequestExcelReportAsync(
            int userId, ExcelReportRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var jobId = Guid.NewGuid();
                var reportJob = new ReportJobDto
                {
                    JobId = jobId,
                    Status = "Queued",
                    CreatedAt = DateTime.UtcNow,
                    Request = request
                };

                // Queue background job
                await _backgroundJobService.EnqueueReportGenerationAsync(userId, jobId, request, cancellationToken);

                _logger.LogInformation("Excel report generation queued for user {UserId}, job {JobId}", userId, jobId);
                return Result<ReportJobDto>.Success(reportJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting Excel report for user {UserId}", userId);
                return Result<ReportJobDto>.Failure("Failed to request Excel report");
            }
        }

        public async Task<Result<ReportStatusDto>> GetReportStatusAsync(
            int userId, Guid jobId, CancellationToken cancellationToken = default)
        {
            try
            {
                var status = await _backgroundJobService.GetReportStatusAsync(userId, jobId, cancellationToken);
                return Result<ReportStatusDto>.Success(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report status for user {UserId}, job {JobId}", userId, jobId);
                return Result<ReportStatusDto>.Failure("Report not found");
            }
        }

        public async Task<Result<FileDownloadDto>> DownloadReportAsync(
            int userId, Guid jobId, CancellationToken cancellationToken = default)
        {
            try
            {
                var fileData = await _backgroundJobService.GetReportFileAsync(userId, jobId, cancellationToken);
                return Result<FileDownloadDto>.Success(fileData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading report for user {UserId}, job {JobId}", userId, jobId);
                return Result<FileDownloadDto>.Failure("Report file not found");
            }
        }

        private static string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            return timeSpan.TotalMinutes switch
            {
                < 1 => "Just now",
                < 60 => $"{(int)timeSpan.TotalMinutes} minutes ago",
                < 1440 => $"{(int)timeSpan.TotalHours} hours ago",
                < 10080 => $"{(int)timeSpan.TotalDays} days ago",
                _ => dateTime.ToString("MMM dd, yyyy")
            };
        }

        private static List<FinancialInsightDto> GenerateFinancialInsights(
            Domain.ValueObjects.MonthlyBalance currentMonth,
            Domain.ValueObjects.MonthlyBalance previousMonth,
            List<Domain.ValueObjects.CategoryExpense> categoryExpenses)
        {
            var insights = new List<FinancialInsightDto>();

            // Spending increase alert
            if (currentMonth.TotalExpense > previousMonth.TotalExpense * 1.2m)
            {
                insights.Add(new FinancialInsightDto
                {
                    Title = "High Spending Alert",
                    Description = $"Your expenses increased by {((currentMonth.TotalExpense - previousMonth.TotalExpense) / previousMonth.TotalExpense * 100):F1}% compared to last month",
                    Type = InsightType.SpendingAlert,
                    Severity = InsightSeverity.Warning,
                    Amount = currentMonth.TotalExpense - previousMonth.TotalExpense,
                    Icon = "⚠️"
                });
            }

            // Top category spending
            var topCategory = categoryExpenses.FirstOrDefault();
            if (topCategory != null && topCategory.Percentage > 40)
            {
                insights.Add(new FinancialInsightDto
                {
                    Title = "Category Concentration",
                    Description = $"You spent {topCategory.Percentage:F1}% of your budget on {topCategory.CategoryName}",
                    Type = InsightType.CategoryAlert,
                    Severity = InsightSeverity.Info,
                    Amount = topCategory.TotalAmount,
                    Category = topCategory.CategoryName,
                    Icon = "📊"
                });
            }

            // Positive balance
            if (currentMonth.Balance > 0)
            {
                insights.Add(new FinancialInsightDto
                {
                    Title = "Great Job!",
                    Description = $"You saved {currentMonth.Balance:C} this month",
                    Type = InsightType.SavingsGoal,
                    Severity = InsightSeverity.Info,
                    Amount = currentMonth.Balance,
                    Icon = "💰"
                });
            }

            return insights;
        }
    }
}
