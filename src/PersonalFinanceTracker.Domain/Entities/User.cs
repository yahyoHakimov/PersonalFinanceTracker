using PersonalFinanceTracker.Domain.Common;
using PersonalFinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace PersonalFinanceTracker.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        // Navigation properties
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        // Business logic methods
        public bool IsTokenValid()
        {
            return RefreshTokenExpiry.HasValue && RefreshTokenExpiry.Value > DateTime.UtcNow;
        }

        public bool IsAdmin() => Role == UserRole.Admin;

        public void UpdateRefreshToken(string token, DateTime expiry)
        {
            RefreshToken = token;
            RefreshTokenExpiry = expiry;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RevokeRefreshToken()
        {
            RefreshToken = null;
            RefreshTokenExpiry = null;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
