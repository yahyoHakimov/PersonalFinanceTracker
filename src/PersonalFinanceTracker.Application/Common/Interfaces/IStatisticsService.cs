using PersonalFinanceTracker.Application.Common.DTOs.Statistics;
using PersonalFinanceTracker.Application.Common.Models;
using PersonalFinanceTracker.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Interfaces
{
    public interface IStatisticsService
    {
        Task<Result<CategoryExpenseStatisticsDto>> GetCategoryExpenseStatisticsAsync(
            int userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        Task<Result<TrendAnalysisDto>> GetTrendAnalysisAsync(
            int userId, int months, CancellationToken cancellationToken = default);

        Task<Result<DashboardStatisticsDto>> GetDashboardStatisticsAsync(
            int userId, CancellationToken cancellationToken = default);

        Task<Result<ReportJobDto>> RequestExcelReportAsync(
            int userId, ExcelReportRequestDto request, CancellationToken cancellationToken = default);

        Task<Result<ReportStatusDto>> GetReportStatusAsync(
            int userId, Guid jobId, CancellationToken cancellationToken = default);

        Task<Result<FileDownloadDto>> DownloadReportAsync(
            int userId, Guid jobId, CancellationToken cancellationToken = default);
    }
}
