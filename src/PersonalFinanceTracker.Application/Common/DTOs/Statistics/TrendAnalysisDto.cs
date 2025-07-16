using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Domain.ValueObjects
{
    public class TrendAnalysisDto
    {
        public int MonthsAnalyzed { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<MonthlyTrendDto> MonthlyTrends { get; set; } = new();
        public TrendSummaryDto IncomeTrend { get; set; } = new();
        public TrendSummaryDto ExpenseTrend { get; set; } = new();
        public TrendSummaryDto BalanceTrend { get; set; } = new();
        public List<CategoryTrendDto> CategoryTrends { get; set; } = new();
    }

    public class MonthlyTrendDto
    {
        public DateTime Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Balance { get; set; }
        public decimal IncomeGrowth { get; set; }
        public decimal ExpenseGrowth { get; set; }
        public decimal BalanceGrowth { get; set; }
        public int TransactionCount { get; set; }
    }

    public class TrendSummaryDto
    {
        public decimal CurrentAmount { get; set; }
        public decimal PreviousAmount { get; set; }
        public decimal GrowthAmount { get; set; }
        public decimal GrowthPercentage { get; set; }
        public bool IsIncreasing { get; set; }
        public decimal AverageMonthly { get; set; }
        public decimal HighestMonthly { get; set; }
        public decimal LowestMonthly { get; set; }
    }

    public class CategoryTrendDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public decimal CurrentMonthAmount { get; set; }
        public decimal PreviousMonthAmount { get; set; }
        public decimal GrowthPercentage { get; set; }
        public bool IsIncreasing { get; set; }
        public List<decimal> MonthlyAmounts { get; set; } = new();
    }
}
