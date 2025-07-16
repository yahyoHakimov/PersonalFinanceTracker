using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.DTOs.Statistics
{
    public class ExcelReportRequestDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ExcelReportType> ReportTypes { get; set; } = new();
        public bool IncludeCharts { get; set; } = true;
        public bool IncludeCategoryBreakdown { get; set; } = true;
        public bool IncludeTrendAnalysis { get; set; } = true;
        public string? EmailTo { get; set; }
    }

    public enum ExcelReportType
    {
        Transactions,
        Categories,
        MonthlyBalance,
        CategoryExpenses,
        TrendAnalysis,
        Complete
    }
}
