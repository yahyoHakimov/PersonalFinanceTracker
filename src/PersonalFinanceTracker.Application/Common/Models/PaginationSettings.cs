using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Models
{
    public class PaginationSettings
    {
        public int DefaultPageSize { get; set; } = 10;
        public int MaxPageSize { get; set; } = 100;
        public int MinPageSize { get; set; } = 1;
    }
}
