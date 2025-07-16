using PersonalFinanceTracker.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.DTOs.Statistics
{
    public class DashboardStatisticsDto
    {
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Current Month Stats
        public MonthlyStatsDto CurrentMonth { get; set; } = new();
        public MonthlyStatsDto PreviousMonth { get; set; } = new();

        // Year Stats
        public YearlyStatsDto CurrentYear { get; set; } = new();
        public YearlyStatsDto PreviousYear { get; set; } = new();

        // Top Categories
        public List<CategoryExpenseItemDto> TopExpenseCategories { get; set; } = new();
        public List<CategoryExpenseItemDto> TopIncomeCategories { get; set; } = new();

        // Recent Activity
        public List<RecentTransactionDto> RecentTransactions { get; set; } = new();

        // Goals and Insights
        public List<FinancialInsightDto> Insights { get; set; } = new();
    }

    public class MonthlyStatsDto
    {
        public DateTime Month { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal Balance { get; set; }
        public int TransactionCount { get; set; }
        public decimal IncomeGrowth { get; set; }
        public decimal ExpenseGrowth { get; set; }
        public decimal BalanceGrowth { get; set; }
        public bool IsPositive => Balance > 0;
    }

    public class YearlyStatsDto
    {
        public int Year { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal Balance { get; set; }
        public int TransactionCount { get; set; }
        public decimal AverageMonthlyIncome { get; set; }
        public decimal AverageMonthlyExpenses { get; set; }
        public decimal GrowthFromPreviousYear { get; set; }
    }

    public class RecentTransactionDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RelativeTime { get; set; } = string.Empty;
    }

    public class FinancialInsightDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public InsightType Type { get; set; }
        public InsightSeverity Severity { get; set; }
        public decimal? Amount { get; set; }
        public string? Category { get; set; }
        public string Icon { get; set; } = string.Empty;
    }

    public enum InsightType
    {
        SpendingAlert,
        SavingsGoal,
        BudgetRecommendation,
        TrendObservation,
        CategoryAlert
    }

    public enum InsightSeverity
    {
        Info,
        Warning,
        Alert
    }
}
