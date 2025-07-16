using PersonalFinanceTracker.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Worker.Workers
{
    public class ReportGenerationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReportGenerationWorker> _logger;

        public ReportGenerationWorker(
            IServiceProvider serviceProvider,
            ILogger<ReportGenerationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Report Generation Worker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var backgroundJobService = scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();

                    // Process queued report generation jobs
                    _logger.LogInformation("Processing report generation jobs...");

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Report Generation Worker");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("Report Generation Worker stopped");
        }
    }
}
