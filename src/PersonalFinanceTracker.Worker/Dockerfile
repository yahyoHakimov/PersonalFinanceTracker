# src/PersonalFinanceTracker.Worker/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/PersonalFinanceTracker.Worker/PersonalFinanceTracker.Worker.csproj", "src/PersonalFinanceTracker.Worker/"]
COPY ["src/PersonalFinanceTracker.Application/PersonalFinanceTracker.Application.csproj", "src/PersonalFinanceTracker.Application/"]
COPY ["src/PersonalFinanceTracker.Domain/PersonalFinanceTracker.Domain.csproj", "src/PersonalFinanceTracker.Domain/"]
COPY ["src/PersonalFinanceTracker.Infrastructure/PersonalFinanceTracker.Infrastructure.csproj", "src/PersonalFinanceTracker.Infrastructure/"]

RUN dotnet restore "src/PersonalFinanceTracker.Worker/PersonalFinanceTracker.Worker.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/PersonalFinanceTracker.Worker"
RUN dotnet build "PersonalFinanceTracker.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PersonalFinanceTracker.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "PersonalFinanceTracker.Worker.dll"]