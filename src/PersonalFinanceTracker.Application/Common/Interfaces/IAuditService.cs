using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(int userId, string action, string entityName, int entityId,
            object? oldValue, object? newValue, CancellationToken cancellationToken = default);
        Task LogAsync(int userId, string action, string entityName, int entityId,
            object? oldValue, object? newValue, string? ipAddress, string? userAgent,
            CancellationToken cancellationToken = default);
    }
}
