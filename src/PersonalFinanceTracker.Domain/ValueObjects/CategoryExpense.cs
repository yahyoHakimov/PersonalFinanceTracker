using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Domain.ValueObjects
{
    public class CategoryExpense
    {
        public string CategoryName { get; }
        public decimal TotalAmount { get; }
        public int TransactionCount { get; }
        public decimal Percentage { get; }

        public CategoryExpense(string categoryName, decimal totalAmount, int transactionCount, decimal percentage)
        {
            CategoryName = categoryName;
            TotalAmount = totalAmount;
            TransactionCount = transactionCount;
            Percentage = percentage;
        }
    }
}
