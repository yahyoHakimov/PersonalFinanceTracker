using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Domain.ValueObjects
{
    public class MonthlyBalance
    {
        public decimal TotalIncome { get; }
        public decimal TotalExpense { get; }
        public decimal Balance { get; }
        public DateTime Month { get; }

        public MonthlyBalance(decimal totalIncome, decimal totalExpense, DateTime month)
        {
            TotalIncome = totalIncome;
            TotalExpense = totalExpense;
            Balance = totalIncome - totalExpense;
            Month = month;
        }

        public bool IsPositive() => Balance > 0;
        public bool IsNegative() => Balance < 0;
        public decimal GetBalancePercentage() => TotalIncome > 0 ? (Balance / TotalIncome) * 100 : 0;
    }
}
