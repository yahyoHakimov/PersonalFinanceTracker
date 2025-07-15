using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Models
{
    public class ValidationSettings
    {
        public int MinPasswordLength { get; set; } = 8;
        public int MaxPasswordLength { get; set; } = 100;
        public int MinUsernameLength { get; set; } = 3;
        public int MaxUsernameLength { get; set; } = 50;
        public int MaxCategoryNameLength { get; set; } = 100;
        public int MaxTransactionNoteLength { get; set; } = 500;
        public decimal MaxTransactionAmount { get; set; } = 999999999.99m;
        public decimal MinTransactionAmount { get; set; } = 0.01m;
    }
}
