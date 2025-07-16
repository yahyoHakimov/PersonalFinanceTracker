// PersonalFinanceTracker.API/Extensions/ServiceCollectionExtensions.cs
using FluentValidation;
using PersonalFinanceTracker.Application.Common.Interfaces;
using PersonalFinanceTracker.Application.Common.Mappings;
using PersonalFinanceTracker.Application.Common.Models;
using PersonalFinanceTracker.Infrastructure.Repositories;
using PersonalFinanceTracker.Infrastructure.Services;
using System.Reflection;

namespace PersonalFinanceTracker.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AuthMappingProfile>();
                cfg.AddProfile<BusinessMappingProfile>();
                // Note: We removed StatisticsMappingProfile as it was causing issues
                // You can add it back if you create custom mappings for statistics
            });

            // FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Get the Application assembly for validators
            var applicationAssembly = typeof(Application.Common.DTOs.Auth.RegisterRequest).Assembly;
            services.AddValidatorsFromAssembly(applicationAssembly);

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuration models
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.Configure<FileSettings>(configuration.GetSection("FileSettings"));
            services.Configure<WorkerSettings>(configuration.GetSection("WorkerSettings"));
            services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));
            services.Configure<ValidationSettings>(configuration.GetSection("ValidationSettings"));

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            // Core Services
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ITransactionService, TransactionService>();

            // Statistics Services - ADD THESE
            services.AddScoped<IStatisticsService, StatisticsService>();
            services.AddScoped<IBackgroundJobService, BackgroundJobService>();

            // Excel Generation Service
            services.AddScoped<ExcelReportGenerator>();

            return services;
        }
    }
}