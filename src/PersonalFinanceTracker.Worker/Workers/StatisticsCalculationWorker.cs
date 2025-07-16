using Microsoft.Extensions.Options;
using PersonalFinanceTracker.Application.Common.Interfaces;
using PersonalFinanceTracker.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Worker.Workers
{
    public class StatisticsCalculationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StatisticsCalculationWorker> _logger;
        private readonly WorkerSettings _settings;

        public StatisticsCalculationWorker(
            IServiceProvider serviceProvider,
            ILogger<StatisticsCalculationWorker> logger,
            IOptions<WorkerSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Statistics Calculation Worker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var statisticsService = scope.ServiceProvider.GetRequiredService<IStatisticsService>();

                    _logger.LogInformation("Running statistics calculation...");

                    // Pre-calculate statistics for active users
                    await CalculateStatisticsForActiveUsers(statisticsService, stoppingToken);

                    await Task.Delay(_settings.StatisticsCalculationInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Statistics Calculation Worker");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("Statistics Calculation Worker stopped");
        }

        private async Task CalculateStatisticsForActiveUsers(IStatisticsService statisticsService, CancellationToken stoppingToken)
        {
            // Pre-calculate dashboard statistics for active users
            _logger.LogInformation("Pre-calculating dashboard statistics for active users");

            // Add your logic here to get active users and pre-calculate their statistics
            await Task.Delay(1000, stoppingToken);
        }
    }
}
