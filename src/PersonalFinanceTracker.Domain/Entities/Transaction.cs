using PersonalFinanceTracker.Domain.Common;
using PersonalFinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Domain.Entities
{
    public class Transaction : BaseEntity
    {
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public string? Note { get; set; }

        // Navigation properties
        public Category Category { get; set; } = null!;
        public User User { get; set; } = null!;

        // Business logic methods
        public bool IsValidAmount()
        {
            return Amount > 0;
        }

        public bool IsIncome() => Type == TransactionType.Income;
        public bool IsExpense() => Type == TransactionType.Expense;

        public bool BelongsToUser(int userId)
        {
            return UserId == userId;
        }

        public decimal GetSignedAmount()
        {
            return Type == TransactionType.Income ? Amount : -Amount;
        }

        public bool IsFromCurrentMonth()
        {
            var now = DateTime.UtcNow;
            return CreatedAt.Year == now.Year && CreatedAt.Month == now.Month;
        }
    }
}
