using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PersonalFinanceTracker.Application.Common.Models;
using PersonalFinanceTracker.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Worker.Workers
{
    public class DataCleanupWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataCleanupWorker> _logger;
        private readonly WorkerSettings _settings;

        public DataCleanupWorker(
            IServiceProvider serviceProvider,
            ILogger<DataCleanupWorker> logger,
            IOptions<WorkerSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Data Cleanup Worker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    _logger.LogInformation("Running data cleanup...");

                    // Clean up old audit logs (older than 1 year)
                    await CleanupOldAuditLogs(context, stoppingToken);

                    // Clean up old report files
                    await CleanupOldReportFiles(stoppingToken);

                    // Clean up expired cache entries
                    await CleanupExpiredCacheEntries(stoppingToken);

                    // Clean up soft-deleted records (older than 90 days)
                    await CleanupSoftDeletedRecords(context, stoppingToken);

                    _logger.LogInformation("Data cleanup completed successfully");

                    await Task.Delay(_settings.DataCleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Data Cleanup Worker");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("Data Cleanup Worker stopped");
        }

        private async Task CleanupOldAuditLogs(ApplicationDbContext context, CancellationToken stoppingToken)
        {
            var cutoffDate = DateTime.UtcNow.AddYears(-1);

            var oldLogsCount = await context.AuditLogs
                .Where(log => log.CreatedAt < cutoffDate)
                .CountAsync(stoppingToken);

            if (oldLogsCount > 0)
            {
                _logger.LogInformation("Found {Count} old audit logs to cleanup", oldLogsCount);

                // Delete old audit logs in batches to avoid memory issues
                var batchSize = 1000;
                var deletedTotal = 0;

                while (true)
                {
                    var batch = await context.AuditLogs
                        .Where(log => log.CreatedAt < cutoffDate)
                        .Take(batchSize)
                        .ToListAsync(stoppingToken);

                    if (!batch.Any())
                        break;

                    context.AuditLogs.RemoveRange(batch);
                    var deletedCount = await context.SaveChangesAsync(stoppingToken);
                    deletedTotal += deletedCount;

                    _logger.LogInformation("Deleted {Count} audit logs in this batch", deletedCount);

                    // Small delay to prevent overwhelming the database
                    await Task.Delay(100, stoppingToken);
                }

                _logger.LogInformation("Cleanup completed: {Total} old audit logs deleted", deletedTotal);
            }
            else
            {
                _logger.LogInformation("No old audit logs found for cleanup");
            }
        }

        private async Task CleanupOldReportFiles(CancellationToken stoppingToken)
        {
            try
            {
                // Clean up report files older than 30 days
                var reportsPath = Path.Combine(Directory.GetCurrentDirectory(), "reports");

                if (!Directory.Exists(reportsPath))
                {
                    _logger.LogInformation("Reports directory does not exist, skipping file cleanup");
                    return;
                }

                var cutoffDate = DateTime.UtcNow.AddDays(-30);
                var reportFiles = Directory.GetFiles(reportsPath, "*.xlsx")
                    .Where(file => File.GetCreationTime(file) < cutoffDate)
                    .ToList();

                if (reportFiles.Any())
                {
                    _logger.LogInformation("Found {Count} old report files to cleanup", reportFiles.Count);

                    var deletedCount = 0;
                    foreach (var file in reportFiles)
                    {
                        try
                        {
                            File.Delete(file);
                            deletedCount++;
                            _logger.LogDebug("Deleted report file: {FileName}", Path.GetFileName(file));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete report file: {FileName}", Path.GetFileName(file));
                        }
                    }

                    _logger.LogInformation("Cleanup completed: {Count} old report files deleted", deletedCount);
                }
                else
                {
                    _logger.LogInformation("No old report files found for cleanup");
                }

                // Also clean up temporary files
                await CleanupTemporaryFiles(reportsPath, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during report files cleanup");
            }
        }

        private async Task CleanupTemporaryFiles(string reportsPath, CancellationToken stoppingToken)
        {
            try
            {
                var tempPath = Path.Combine(reportsPath, "temp");
                if (!Directory.Exists(tempPath))
                    return;

                var cutoffDate = DateTime.UtcNow.AddHours(-2); // Clean temp files older than 2 hours
                var tempFiles = Directory.GetFiles(tempPath)
                    .Where(file => File.GetCreationTime(file) < cutoffDate)
                    .ToList();

                if (tempFiles.Any())
                {
                    _logger.LogInformation("Found {Count} temporary files to cleanup", tempFiles.Count);

                    foreach (var file in tempFiles)
                    {
                        try
                        {
                            File.Delete(file);
                            _logger.LogDebug("Deleted temporary file: {FileName}", Path.GetFileName(file));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete temporary file: {FileName}", Path.GetFileName(file));
                        }
                    }
                }

                await Task.Delay(50, stoppingToken); // Small delay
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during temporary files cleanup");
            }
        }

        private async Task CleanupExpiredCacheEntries(CancellationToken stoppingToken)
        {
            try
            {
                // This is mainly for file-based cache systems
                // Redis handles expiration automatically
                _logger.LogInformation("Checking for expired cache entries...");

                // If you're using file-based caching, add cleanup logic here
                // For Redis, this is handled automatically

                await Task.Delay(100, stoppingToken);
                _logger.LogInformation("Cache cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cache cleanup");
            }
        }

        private async Task CleanupSoftDeletedRecords(ApplicationDbContext context, CancellationToken stoppingToken)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-90); // Permanently delete after 90 days

                // Clean up soft-deleted transactions
                var deletedTransactions = await context.Transactions
                    .IgnoreQueryFilters() // Include soft-deleted records
                    .Where(t => t.IsDeleted && t.DeletedAt < cutoffDate)
                    .CountAsync(stoppingToken);

                if (deletedTransactions > 0)
                {
                    _logger.LogInformation("Found {Count} soft-deleted transactions to permanently delete", deletedTransactions);

                    // Delete in batches
                    var batchSize = 500;
                    var deletedTotal = 0;

                    while (true)
                    {
                        var batch = await context.Transactions
                            .IgnoreQueryFilters()
                            .Where(t => t.IsDeleted && t.DeletedAt < cutoffDate)
                            .Take(batchSize)
                            .ToListAsync(stoppingToken);

                        if (!batch.Any())
                            break;

                        context.Transactions.RemoveRange(batch);
                        var deletedCount = await context.SaveChangesAsync(stoppingToken);
                        deletedTotal += deletedCount;

                        _logger.LogInformation("Permanently deleted {Count} transactions in this batch", deletedCount);
                        await Task.Delay(100, stoppingToken);
                    }

                    _logger.LogInformation("Permanently deleted {Total} soft-deleted transactions", deletedTotal);
                }

                // Clean up soft-deleted categories
                var deletedCategories = await context.Categories
                    .IgnoreQueryFilters()
                    .Where(c => c.IsDeleted && c.DeletedAt < cutoffDate)
                    .CountAsync(stoppingToken);

                if (deletedCategories > 0)
                {
                    _logger.LogInformation("Found {Count} soft-deleted categories to permanently delete", deletedCategories);

                    var categoriesToDelete = await context.Categories
                        .IgnoreQueryFilters()
                        .Where(c => c.IsDeleted && c.DeletedAt < cutoffDate)
                        .ToListAsync(stoppingToken);

                    context.Categories.RemoveRange(categoriesToDelete);
                    await context.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("Permanently deleted {Count} soft-deleted categories", deletedCategories);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during soft-deleted records cleanup");
            }
        }
    }
}
