using PersonalFinanceTracker.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.DTOs.Categories
{
    public class CategoryFilterRequest : PaginationRequest
    {
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; } = "name";
        public bool SortDescending { get; set; } = false;
        public bool IncludeStatistics { get; set; } = true;
    }

}
