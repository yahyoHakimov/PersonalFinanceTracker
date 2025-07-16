using PersonalFinanceTracker.Application.Common.DTOs.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Interfaces
{
    public interface IBackgroundJobService
    {
        Task EnqueueReportGenerationAsync(int userId, Guid jobId, ExcelReportRequestDto request, CancellationToken cancellationToken = default);
        Task<ReportStatusDto> GetReportStatusAsync(int userId, Guid jobId, CancellationToken cancellationToken = default);
        Task<FileDownloadDto> GetReportFileAsync(int userId, Guid jobId, CancellationToken cancellationToken = default);
    }
}
