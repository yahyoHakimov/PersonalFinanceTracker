// PersonalFinanceTracker.Infrastructure/Services/BackgroundJobService.cs
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PersonalFinanceTracker.Application.Common.DTOs.Statistics;
using PersonalFinanceTracker.Application.Common.Interfaces;
using System.Text.Json;

namespace PersonalFinanceTracker.Infrastructure.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<BackgroundJobService> _logger;
        private static readonly Dictionary<Guid, ReportStatusDto> _jobs = new();
        private static readonly Dictionary<Guid, byte[]> _reportFiles = new();

        public BackgroundJobService(IDistributedCache cache, ILogger<BackgroundJobService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task EnqueueReportGenerationAsync(int userId, Guid jobId, ExcelReportRequestDto request, CancellationToken cancellationToken = default)
        {
            var status = new ReportStatusDto
            {
                JobId = jobId,
                Status = ReportStatus.Queued,
                StatusDescription = "Report generation queued",
                CreatedAt = DateTime.UtcNow,
                ProgressPercentage = 0
            };

            _jobs[jobId] = status;

            // Store job info in cache with user association
            var cacheKey = $"report_job:{userId}:{jobId}";
            var jobInfo = new { UserId = userId, JobId = jobId, Request = request, Status = status };

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(jobInfo),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7) },
                cancellationToken);

            // Start background processing (fire-and-forget)
            _ = Task.Run(async () => await ProcessReportGenerationAsync(userId, jobId, request), cancellationToken);

            _logger.LogInformation("Report generation job {JobId} queued for user {UserId}", jobId, userId);
        }

        public async Task<ReportStatusDto> GetReportStatusAsync(int userId, Guid jobId, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"report_job:{userId}:{jobId}";
            var cachedJob = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (string.IsNullOrEmpty(cachedJob))
            {
                throw new KeyNotFoundException($"Report job {jobId} not found for user {userId}");
            }

            if (_jobs.TryGetValue(jobId, out var status))
            {
                return status;
            }

            throw new KeyNotFoundException($"Report job {jobId} status not found");
        }

        public async Task<FileDownloadDto> GetReportFileAsync(int userId, Guid jobId, CancellationToken cancellationToken = default)
        {
            var status = await GetReportStatusAsync(userId, jobId, cancellationToken);

            if (status.Status != ReportStatus.Completed)
            {
                throw new InvalidOperationException($"Report {jobId} is not completed yet");
            }

            if (!_reportFiles.TryGetValue(jobId, out var fileData))
            {
                throw new FileNotFoundException($"Report file {jobId} not found");
            }

            return new FileDownloadDto
            {
                Content = fileData,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = $"financial_report_{jobId:N}.xlsx",
                Size = fileData.Length
            };
        }

        private async Task ProcessReportGenerationAsync(int userId, Guid jobId, ExcelReportRequestDto request)
        {
            try
            {
                _logger.LogInformation("Starting report generation for job {JobId}", jobId);

                // Update status to processing
                UpdateJobStatus(jobId, ReportStatus.Processing, "Generating Excel report...", 10);

                // Simulate report generation (replace with actual Excel generation logic)
                await Task.Delay(5000); // Simulate processing time
                UpdateJobStatus(jobId, ReportStatus.Processing, "Collecting transaction data...", 30);

                await Task.Delay(3000);
                UpdateJobStatus(jobId, ReportStatus.Processing, "Calculating statistics...", 60);

                await Task.Delay(2000);
                UpdateJobStatus(jobId, ReportStatus.Processing, "Generating Excel file...", 90);

                // Generate actual Excel file (placeholder - you'll implement this with ExcelReportGenerator)
                var excelData = await GenerateExcelReportAsync(userId, request);
                _reportFiles[jobId] = excelData;

                UpdateJobStatus(jobId, ReportStatus.Completed, "Report generation completed", 100);

                _logger.LogInformation("Report generation completed for job {JobId}", jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report for job {JobId}", jobId);
                UpdateJobStatus(jobId, ReportStatus.Failed, $"Report generation failed: {ex.Message}", 0);
            }
        }

        private void UpdateJobStatus(Guid jobId, ReportStatus status, string description, int progress)
        {
            if (_jobs.TryGetValue(jobId, out var existingStatus))
            {
                existingStatus.Status = status;
                existingStatus.StatusDescription = description;
                existingStatus.ProgressPercentage = progress;

                if (status == ReportStatus.Processing && existingStatus.StartedAt == null)
                {
                    existingStatus.StartedAt = DateTime.UtcNow;
                }

                if (status == ReportStatus.Completed || status == ReportStatus.Failed)
                {
                    existingStatus.CompletedAt = DateTime.UtcNow;

                    if (status == ReportStatus.Completed)
                    {
                        existingStatus.DownloadUrl = $"/api/statistics/reports/{jobId}/download";
                        existingStatus.FileSizeBytes = _reportFiles.TryGetValue(jobId, out var file) ? file.Length : 0;
                    }
                }

                _jobs[jobId] = existingStatus;
            }
        }

        private async Task<byte[]> GenerateExcelReportAsync(int userId, ExcelReportRequestDto request)
        {
            // This is a simplified placeholder. 
            // In a real implementation, you would inject ExcelReportGenerator and use it here

            // For now, return a simple placeholder file
            var content = $"Financial Report for User {userId}\n" +
                         $"Period: {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}\n" +
                         $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}\n" +
                         $"Report Types: {string.Join(", ", request.ReportTypes)}";

            return System.Text.Encoding.UTF8.GetBytes(content);
        }
    }
}