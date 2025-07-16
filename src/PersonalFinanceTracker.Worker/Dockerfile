# src/PersonalFinanceTracker.Worker/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Create non-root user for security
RUN addgroup --system --gid 1001 appgroup
RUN adduser --system --uid 1001 --ingroup appgroup --shell /bin/false appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore as distinct layers
COPY ["src/PersonalFinanceTracker.Worker/PersonalFinanceTracker.Worker.csproj", "src/PersonalFinanceTracker.Worker/"]
COPY ["src/PersonalFinanceTracker.Application/PersonalFinanceTracker.Application.csproj", "src/PersonalFinanceTracker.Application/"]
COPY ["src/PersonalFinanceTracker.Domain/PersonalFinanceTracker.Domain.csproj", "src/PersonalFinanceTracker.Domain/"]
COPY ["src/PersonalFinanceTracker.Infrastructure/PersonalFinanceTracker.Infrastructure.csproj", "src/PersonalFinanceTracker.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/PersonalFinanceTracker.Worker/PersonalFinanceTracker.Worker.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/PersonalFinanceTracker.Worker"
RUN dotnet build "PersonalFinanceTracker.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PersonalFinanceTracker.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Install required packages for health checks and Excel processing
RUN apt-get update && apt-get install -y \
    curl \
    procps \
    libgdiplus \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create directories and set permissions
RUN mkdir -p /app/logs /app/exports && \
    chown -R appuser:appgroup /app && \
    chmod 755 /app/logs /app/exports

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD ps aux | grep "PersonalFinanceTracker.Worker" | grep -v grep || exit 1

ENTRYPOINT ["dotnet", "PersonalFinanceTracker.Worker.dll"]