using PersonalFinanceTracker.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE
        public string EntityName { get; set; } = string.Empty; // Transaction, Category, User
        public int EntityId { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;

        // Navigation properties
        public User User { get; set; } = null!;

        // Business logic methods
        public bool IsCreateAction() => Action.Equals("CREATE", StringComparison.OrdinalIgnoreCase);
        public bool IsUpdateAction() => Action.Equals("UPDATE", StringComparison.OrdinalIgnoreCase);
        public bool IsDeleteAction() => Action.Equals("DELETE", StringComparison.OrdinalIgnoreCase);
    }
}
