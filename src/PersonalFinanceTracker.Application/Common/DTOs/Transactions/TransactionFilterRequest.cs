using PersonalFinanceTracker.Application.Common.Models;
using PersonalFinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.DTOs.Transactions
{
    public class TransactionFilterRequest : PaginationRequest
    {
        public string? SearchTerm { get; set; }
        public TransactionType? Type { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SortBy { get; set; } = "created_at";
        public bool SortDescending { get; set; } = true;
    }
}
