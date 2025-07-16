# src/PersonalFinanceTracker.API/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/PersonalFinanceTracker.API/PersonalFinanceTracker.API.csproj", "src/PersonalFinanceTracker.API/"]
COPY ["src/PersonalFinanceTracker.Application/PersonalFinanceTracker.Application.csproj", "src/PersonalFinanceTracker.Application/"]
COPY ["src/PersonalFinanceTracker.Domain/PersonalFinanceTracker.Domain.csproj", "src/PersonalFinanceTracker.Domain/"]
COPY ["src/PersonalFinanceTracker.Infrastructure/PersonalFinanceTracker.Infrastructure.csproj", "src/PersonalFinanceTracker.Infrastructure/"]

RUN dotnet restore "src/PersonalFinanceTracker.API/PersonalFinanceTracker.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/PersonalFinanceTracker.API"
RUN dotnet build "PersonalFinanceTracker.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PersonalFinanceTracker.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "PersonalFinanceTracker.API.dll"]