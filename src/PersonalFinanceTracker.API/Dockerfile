# src/PersonalFinanceTracker.API/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create non-root user for security
RUN addgroup --system --gid 1001 appgroup
RUN adduser --system --uid 1001 --ingroup appgroup --shell /bin/false appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore as distinct layers
COPY ["src/PersonalFinanceTracker.API/PersonalFinanceTracker.API.csproj", "src/PersonalFinanceTracker.API/"]
COPY ["src/PersonalFinanceTracker.Application/PersonalFinanceTracker.Application.csproj", "src/PersonalFinanceTracker.Application/"]
COPY ["src/PersonalFinanceTracker.Domain/PersonalFinanceTracker.Domain.csproj", "src/PersonalFinanceTracker.Domain/"]
COPY ["src/PersonalFinanceTracker.Infrastructure/PersonalFinanceTracker.Infrastructure.csproj", "src/PersonalFinanceTracker.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/PersonalFinanceTracker.API/PersonalFinanceTracker.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/PersonalFinanceTracker.API"
RUN dotnet build "PersonalFinanceTracker.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PersonalFinanceTracker.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Install required packages for health checks and monitoring
RUN apt-get update && apt-get install -y \
    curl \
    procps \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create logs directory and set permissions
RUN mkdir -p /app/logs && \
    chown -R appuser:appgroup /app && \
    chmod 755 /app/logs

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "PersonalFinanceTracker.API.dll"]