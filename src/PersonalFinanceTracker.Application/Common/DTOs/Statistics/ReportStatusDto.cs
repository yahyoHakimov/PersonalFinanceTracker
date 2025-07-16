using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.DTOs.Statistics
{
    public class ReportStatusDto
    {
        public Guid JobId { get; set; }
        public ReportStatus Status { get; set; }
        public string StatusDescription { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int ProgressPercentage { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsCompleted => Status == ReportStatus.Completed;
        public bool HasError => Status == ReportStatus.Failed;
        public string? DownloadUrl { get; set; }
        public long? FileSizeBytes { get; set; }
    }

    public enum ReportStatus
    {
        Queued,
        Processing,
        Completed,
        Failed
    }
}
