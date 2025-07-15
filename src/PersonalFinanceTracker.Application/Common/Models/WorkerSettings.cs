using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Models
{
    public class WorkerSettings
    {
        public TimeSpan StatisticsCalculationInterval { get; set; } = TimeSpan.FromHours(1);
        public TimeSpan ReportGenerationInterval { get; set; } = TimeSpan.FromHours(24);
        public TimeSpan DataCleanupInterval { get; set; } = TimeSpan.FromDays(7);
    }
}
