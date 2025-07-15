
using PersonalFinanceTracker.API.Middleware;

namespace PersonalFinanceTracker.API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionMiddleware>();
        }

        public static IApplicationBuilder UseAuditing(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuditMiddleware>();
        }
    }
}
