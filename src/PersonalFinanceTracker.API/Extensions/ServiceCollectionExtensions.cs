using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
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
            // AutoMapper - Fix the registration
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AuthMappingProfile>();
                cfg.AddProfile<BusinessMappingProfile>();
            });

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuration models
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            // Services
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ITransactionService, TransactionService>();

            return services;
        }
    }
}
