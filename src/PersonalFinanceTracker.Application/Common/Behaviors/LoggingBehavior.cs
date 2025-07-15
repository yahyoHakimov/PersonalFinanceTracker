using MediatR;
using Microsoft.Extensions.Logging;
using PersonalFinanceTracker.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogInformation("Handling {RequestName}", requestName);

            try
            {
                var response = await next();
                _logger.LogInformation("Handled {RequestName} successfully", requestName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling {RequestName}", requestName);
                throw;
            }
        }
    }
}
