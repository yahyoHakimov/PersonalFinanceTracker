using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PersonalFinanceTracker.Application.Common.Interfaces;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAsync(int userId, string action, string entityName, int entityId,
            object? oldValue, object? newValue, CancellationToken cancellationToken = default)
        {
            await LogAsync(userId, action, entityName, entityId, oldValue, newValue, null, null, cancellationToken);
        }

        public async Task LogAsync(int userId, string action, string entityName, int entityId,
            object? oldValue, object? newValue, string? ipAddress, string? userAgent,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    OldValue = oldValue != null ? JsonConvert.SerializeObject(oldValue) : null,
                    NewValue = newValue != null ? JsonConvert.SerializeObject(newValue) : null,
                    IpAddress = ipAddress ?? "Unknown",
                    UserAgent = userAgent ?? "Unknown",
                    CreatedAt = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log audit for user {UserId}, action {Action}", userId, action);
            }
        }
    }

}
