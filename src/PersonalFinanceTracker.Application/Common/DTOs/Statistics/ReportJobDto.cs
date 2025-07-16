using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.DTOs.Statistics
{
    public class ReportJobDto
    {
        public Guid JobId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? DownloadUrl { get; set; }
        public ExcelReportRequestDto Request { get; set; } = new();
    }
}
