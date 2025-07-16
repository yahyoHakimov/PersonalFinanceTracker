using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Domain.ValueObjects
{
    public class CategoryExpenseStatisticsDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalExpenses { get; set; }
        public List<CategoryExpenseItemDto> Categories { get; set; } = new();
        public CategoryExpenseItemDto? TopCategory { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AverageTransactionAmount { get; set; }
    }

    public class CategoryExpenseItemDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
        public decimal Percentage { get; set; }
        public decimal AverageAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal MinAmount { get; set; }
    }
}
