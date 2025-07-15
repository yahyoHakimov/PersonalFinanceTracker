using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.DTOs.Transactions
{
    public class MonthlyBalanceDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance { get; set; }
        public DateTime Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int Year { get; set; }
        public bool IsPositive => Balance > 0;
        public bool IsNegative => Balance < 0;
        public decimal BalancePercentage { get; set; }
        public int IncomeTransactionCount { get; set; }
        public int ExpenseTransactionCount { get; set; }
        public int TotalTransactionCount { get; set; }
    }
}
