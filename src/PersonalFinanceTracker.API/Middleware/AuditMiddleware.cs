using PersonalFinanceTracker.Application.Common.Interfaces;
using System.Security.Claims;

namespace PersonalFinanceTracker.API.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                // Log successful requests for authenticated users
                if (context.User.Identity?.IsAuthenticated == true &&
                    ShouldAudit(context.Request.Method, context.Request.Path))
                {
                    var userId = GetUserId(context.User);
                    if (userId.HasValue)
                    {
                        // Get audit service from DI container
                        var auditService = context.RequestServices.GetService<IAuditService>();
                        if (auditService != null)
                        {
                            var action = GetActionFromMethod(context.Request.Method);
                            var entityName = GetEntityFromPath(context.Request.Path);
                            var ipAddress = GetClientIpAddress(context);
                            var userAgent = context.Request.Headers.UserAgent.ToString();

                            await auditService.LogAsync(userId.Value, action, entityName, 0, null, null, ipAddress, userAgent);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in audit middleware");
                throw;
            }
        }

        private static bool ShouldAudit(string method, PathString path)
        {
            var auditableMethods = new[] { "POST", "PUT", "DELETE" };
            var auditablePaths = new[] { "/api/transactions", "/api/categories", "/api/auth" };

            return auditableMethods.Contains(method.ToUpper()) &&
                   auditablePaths.Any(p => path.StartsWithSegments(p));
        }

        private static int? GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private static string GetActionFromMethod(string method) => method.ToUpper() switch
        {
            "POST" => "CREATE",
            "PUT" => "UPDATE",
            "DELETE" => "DELETE",
            _ => "VIEW"
        };

        private static string GetEntityFromPath(PathString path)
        {
            var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments?.LastOrDefault()?.ToUpper() ?? "UNKNOWN";
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
