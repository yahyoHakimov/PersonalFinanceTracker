# Personal Finance Tracker API

A RESTful API for personal finance management built with ASP.NET Core 8.0, featuring JWT authentication, role-based access control, and comprehensive financial tracking capabilities.

## Features

- 🔐 **Authentication** - JWT tokens with refresh token support
- 👥 **Role-Based Access Control** - Admin and User roles
- 📊 **Transaction Management** - Create, read, update, delete transactions
- 🏷️ **Category Management** - Organize expenses by categories
- 📈 **Financial Reports** - Monthly balance and statistics
- 🗃️ **Audit Logging** - Track all user activities
- 📋 **Excel Export** - Generate financial reports
- 🔄 **Soft Delete** - Data recovery capabilities
- 🎯 **Optimistic Concurrency** - Prevent data conflicts

## Tech Stack

- **Backend**: ASP.NET Core 8.0
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Caching**: Redis/In-Memory cache
- **Logging**: Serilog structured logging
- **Documentation**: Swagger/OpenAPI
- **Testing**: xUnit with integration tests
- **Containerization**: Docker & Docker Compose

## Quick Start

### Prerequisites

- .NET 8.0 SDK
- PostgreSQL 12+
- Git

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/yahyoHakimov/PersonalFinanceTracker.git
cd src
```

2. **Set up the database**
```bash
# Update connection string in appsettings.Development.json
# Then run migrations
cd src/PersonalFinanceTracker.API
dotnet ef database update --project ../PersonalFinanceTracker.Infrastructure
```

3. **Run the application**
```bash
dotnet run
```

4. **Access the API**
- Swagger UI: `http://localhost:5184`
- API Base URL: `http://localhost:5184/api`

## Configuration

Update `appsettings.json` with your settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=PersonalFinanceTracker;Username=postgres;Password=your_password;Port=5432"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-32-characters-minimum",
    "Issuer": "PersonalFinanceTracker",
    "Audience": "PersonalFinanceTracker-Users",
    "ExpiryMinutes": 60
  }
}
```

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `POST /api/auth/refresh` - Refresh JWT token
- `GET /api/auth/me` - Get current user info

### Categories
- `GET /api/categories` - Get user categories
- `POST /api/categories` - Create category
- `PUT /api/categories/{id}` - Update category
- `DELETE /api/categories/{id}` - Delete category

### Transactions
- `GET /api/transactions` - Get user transactions
- `POST /api/transactions` - Create transaction
- `PUT /api/transactions/{id}` - Update transaction
- `DELETE /api/transactions/{id}` - Delete transaction
- `GET /api/transactions/monthly-balance/{year}/{month}` - Get monthly balance

## Usage Example

1. **Register a user**
```bash
curl -X POST http://localhost:5184/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "john_doe",
    "email": "john@example.com",
    "password": "Password123!",
    "confirmPassword": "Password123!"
  }'
```

2. **Create a category**
```bash
curl -X POST http://localhost:5184/api/categories \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Food",
    "color": "#FF6B6B"
  }'
```

3. **Add a transaction**
```bash
curl -X POST http://localhost:5184/api/transactions \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 25.50,
    "type": 2,
    "categoryId": 1,
    "note": "Lunch at restaurant"
  }'
```

## Docker Deployment

```bash
# Build and run with Docker Compose
docker-compose up --build

# Or run individual services
docker build -t finance-tracker .
docker run -p 5184:80 finance-tracker
```

## Testing

```bash
# Run unit tests
dotnet test src/PersonalFinanceTracker.Tests.Unit

# Run integration tests
dotnet test src/PersonalFinanceTracker.Tests.Integration
```

## Thank You