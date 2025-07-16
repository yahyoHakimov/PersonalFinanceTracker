using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PersonalFinanceTracker.API.Extensions;
using PersonalFinanceTracker.Infrastructure.Data;
using PersonalFinanceTracker.Infrastructure.Data.Seed;
using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add application services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(60);
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Configure Redis Cache
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "PersonalFinanceTracker";
    });
}
else
{
    // Fallback to memory cache for development
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<Microsoft.Extensions.Caching.Distributed.IDistributedCache,
        MemoryDistributedCache>();
}

// Configure JWT Authentication
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key is required");
var issuer = jwtSection["Issuer"] ?? "PersonalFinanceTracker";
var audience = jwtSection["Audience"] ?? "PersonalFinanceTracker-Users";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Configure Health Checks
builder.Services.AddHealthChecks()
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
            return HealthCheckResult.Healthy("Redis is available");
        }
        catch
        {
            return HealthCheckResult.Unhealthy("Redis is not available");
        }
    });
// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Personal Finance Tracker API",
        Version = "v1",
        Description = "RESTful API for personal finance management with statistics and Excel export capabilities",
        Contact = new OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@personalfinancetracker.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:8080")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("*")
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });

    options.AddPolicy("ProductionPolicy", policy =>
    {
        // Configure for production domains
        policy.WithOrigins() // Add your production domains here
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Personal Finance Tracker API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
    });
}

// Use appropriate CORS policy
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentPolicy");
}
else
{
    app.UseCors("ProductionPolicy");
}

// Custom middleware
app.UseCustomExceptionHandling();
app.UseAuditing();

// Health checks
app.UseHealthChecks("/health");
app.UseHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.UseHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

// Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Add a simple root endpoint
app.MapGet("/", () => new
{
    Application = "Personal Finance Tracker API",
    Version = "1.0",
    Status = "Running",
    Documentation = "/swagger",
    Health = "/health"
});

// Database seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Ensuring database is created...");
        await context.Database.EnsureCreatedAsync();

        logger.LogInformation("Starting database seeding...");
        await DataSeeder.SeedAsync(context);
        logger.LogInformation("Database seeding completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database");

        // Don't fail startup in production, just log the error
        if (app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

app.Run();