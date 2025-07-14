using PersonalFinanceTracker.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace PersonalFinanceTracker.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#000000";
        public int UserId { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        // Business logic methods
        public bool IsValidColor()
        {
            return Color.StartsWith("#") && Color.Length == 7;
        }

        public bool BelongsToUser(int userId)
        {
            return UserId == userId;
        }
    }
}
