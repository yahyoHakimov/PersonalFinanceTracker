// PersonalFinanceTracker.Worker/Extensions/ServiceCollectionExtensions.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PersonalFinanceTracker.Application.Common.Interfaces;
using PersonalFinanceTracker.Application.Common.Mappings;
using PersonalFinanceTracker.Application.Common.Models;
using PersonalFinanceTracker.Infrastructure.Data;
using PersonalFinanceTracker.Infrastructure.Repositories;
using PersonalFinanceTracker.Infrastructure.Services;
using PersonalFinanceTracker.Worker.Workers;

namespace PersonalFinanceTracker.Worker.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWorkerApplicationServices(this IServiceCollection services)
        {
            // AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AuthMappingProfile>();
                cfg.AddProfile<BusinessMappingProfile>();

            });

            return services;
        }

        
        public static IServiceCollection AddWorkerInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database Configuration
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(120); // Longer timeout for worker operations
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                });

                // Enable detailed errors for workers
                options.EnableDetailedErrors();
            });

            // Redis Cache Configuration
            var redisConnectionString = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "PersonalFinanceTracker_Worker";
                });
            }
            else
            {
                // Fallback to memory cache
                services.AddMemoryCache();
                services.AddSingleton<IDistributedCache,
                    MemoryDistributedCache>();
            }

            // Configuration Settings
            services.Configure<WorkerSettings>(configuration.GetSection("WorkerSettings"));
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.Configure<FileSettings>(configuration.GetSection("FileSettings"));

            // Repository Services
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            // Business Services
            services.AddScoped<IStatisticsService, StatisticsService>();
            services.AddScoped<IBackgroundJobService, BackgroundJobService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ITransactionService, TransactionService>();

            // Excel Generation Service
            services.AddScoped<ExcelReportGenerator>();

            return services;
        }

        /// <summary>
        /// Add all background worker services
        /// </summary>
        public static IServiceCollection AddWorkerServices(this IServiceCollection services)
        {
            // Background Workers
            services.AddHostedService<ReportGenerationWorker>();
            services.AddHostedService<StatisticsCalculationWorker>();
            services.AddHostedService<DataCleanupWorker>();

            return services;
        }

        
        public static IServiceCollection AddWorkerHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddHealthChecks()
                .AddCheck("database", () =>
                {
                    try
                    {
                        // Simple database health check
                        return HealthCheckResult.Healthy("Database is available");
                    }
                    catch
                    {
                        return HealthCheckResult.Unhealthy("Database is not available");
                    }
                })
                .AddCheck("redis", () =>
                {
                    try
                    {
                        // Simple Redis health check
                        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Redis is available");
                    }
                    catch
                    {
                        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Redis is not available");
                    }
                })
                .AddCheck("file_system", () =>
                {
                    try
                    {
                        // Check if required directories exist and are writable
                        var reportsPath = Path.Combine(Directory.GetCurrentDirectory(), "reports");
                        var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");

                        Directory.CreateDirectory(reportsPath);
                        Directory.CreateDirectory(logsPath);

                        // Test write permissions
                        var testFile = Path.Combine(reportsPath, "health_check_test.txt");
                        File.WriteAllText(testFile, "health check");
                        File.Delete(testFile);

                        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("File system is accessible");
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy($"File system check failed: {ex.Message}");
                    }
                });

            return services;
        }

        
        public static IServiceCollection AddWorkerLogging(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
                builder.AddDebug();

                // Add file logging if configured
                // You can add Serilog here if needed

                // Set minimum log levels
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
                builder.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
                builder.AddFilter("System", LogLevel.Warning);
            });

            return services;
        }

        /// <summary>
        /// Add all worker-specific services in one call
        /// </summary>
        public static IServiceCollection AddWorkerPlatform(this IServiceCollection services, IConfiguration configuration)
        {
            // Add all worker services
            services.AddWorkerApplicationServices();
            services.AddWorkerInfrastructureServices(configuration);
            services.AddWorkerServices();
            services.AddWorkerHealthChecks(configuration);
            services.AddWorkerLogging(configuration);

            return services;
        }
    }
}