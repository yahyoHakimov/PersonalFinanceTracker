using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Models
{
    public class CacheSettings
    {
        public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(15);
        public TimeSpan SlidingExpiration { get; set; } = TimeSpan.FromMinutes(5);
    }
}
