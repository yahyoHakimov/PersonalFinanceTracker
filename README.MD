# Personal Finance Tracker 💰

A comprehensive **Personal Finance Management System** built with **ASP.NET Core 7.0** following **Clean Architecture** principles. Track your income, expenses, and financial goals with powerful analytics and reporting features.

## 🚀 Features

### ✅ **Completed Features**
- **🔐 Authentication & Authorization**
  - JWT + Refresh Token implementation
  - Role-based Access Control (Admin/User)
  - Secure password hashing with BCrypt
  - Token refresh mechanism

- **💳 Transaction Management**
  - Full CRUD operations for transactions
  - Income and expense tracking
  - Transaction categorization
  - Soft delete functionality

- **📊 Category Management**
  - Custom category creation
  - Color-coded categories
  - Category-based organization

- **🛡️ Security & Data Integrity**
  - Optimistic concurrency control
  - Comprehensive audit logging
  - Data validation with FluentValidation

- **🏗️ Architecture**
  - Clean Architecture implementation
  - Repository Pattern
  - Dependency Injection
  - Exception handling middleware

### 🚧 **In Development**
- Advanced filtering and search
- Financial analytics and reporting
- Excel export functionality
- Background job processing
- Redis caching integration

## 🛠️ Tech Stack

### **Backend**
- **ASP.NET Core 7.0** - Web API Framework
- **Entity Framework Core** - ORM
- **PostgreSQL** - Primary Database
- **JWT** - Authentication
- **AutoMapper** - Object Mapping
- **FluentValidation** - Input Validation

### **Architecture Patterns**
- **Clean Architecture** - Dependency separation
- **Repository Pattern** - Data access abstraction
- **CQRS** - Command Query Responsibility Segregation
- **Mediator Pattern** - Request/Response handling

### **Documentation**
- **Swagger/OpenAPI** - API Documentation
- **JWT Integration** - Swagger Authentication

## 📁 Project Structure

```
PersonalFinanceTracker/
├── src/
│   ├── PersonalFinanceTracker.API/          # Web API layer
│   ├── PersonalFinanceTracker.Application/  # Business logic
│   ├── PersonalFinanceTracker.Domain/       # Domain entities
│   ├── PersonalFinanceTracker.Infrastructure/ # Data access, external services
│   └── PersonalFinanceTracker.Worker/       # Background services
├── tests/
│   ├── PersonalFinanceTracker.Tests.Unit/
│   └── PersonalFinanceTracker.Tests.Integration/
├── docker-compose.yml
├── README.md
└── .gitignore
```

## 🚀 Getting Started

### Prerequisites
- **.NET 7.0 SDK** or higher
- **PostgreSQL** database
- **Visual Studio 2022** or **VS Code**

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/personal-finance-tracker.git
   cd personal-finance-tracker
   ```

2. **Set up PostgreSQL Database**
   - Create a new PostgreSQL database
   - Update connection string in `appsettings.json`

3. **Configure Application Settings**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=PersonalFinanceDB;Username=your_username;Password=your_password"
     },
     "JwtSettings": {
       "SecretKey": "your-secret-key-here",
       "Issuer": "PersonalFinanceTracker",
       "Audience": "PersonalFinanceTracker",
       "ExpiryMinutes": 60
     }
   }
   ```

4. **Run Database Migrations**
   ```bash
   dotnet ef database update --project src/PersonalFinanceTracker.Infrastructure --startup-project src/PersonalFinanceTracker.API
   ```

5. **Build and Run**
   ```bash
   dotnet build
   dotnet run --project src/PersonalFinanceTracker.API
   ```

6. **Access the API**
   - API: `https://localhost:7070`
   - Swagger Documentation: `https://localhost:7070/swagger`

## 📖 API Documentation

### Authentication Endpoints
```http
POST /api/auth/register    # User registration
POST /api/auth/login       # User login
POST /api/auth/refresh     # Refresh token
POST /api/auth/logout      # User logout
```

### Transaction Endpoints
```http
GET    /api/transactions           # Get all transactions (paginated)
GET    /api/transactions/{id}      # Get transaction by ID
POST   /api/transactions           # Create new transaction
PUT    /api/transactions/{id}      # Update transaction
DELETE /api/transactions/{id}      # Delete transaction
```

### Category Endpoints
```http
GET    /api/categories        # Get all categories
GET    /api/categories/{id}   # Get category by ID
POST   /api/categories        # Create new category
PUT    /api/categories/{id}   # Update category
DELETE /api/categories/{id}   # Delete category
```

### Request/Response Examples

**Create Transaction:**
```json
POST /api/transactions
{
  "amount": 150.00,
  "type": "Expense",
  "categoryId": 1,
  "note": "Grocery shopping",
  "date": "2024-01-15T10:30:00Z"
}
```

**Response:**
```json
{
  "id": 1,
  "amount": 150.00,
  "type": "Expense",
  "categoryId": 1,
  "note": "Grocery shopping",
  "createdAt": "2024-01-15T10:30:00Z",
  "category": {
    "id": 1,
    "name": "Groceries",
    "color": "#FF6B6B"
  }
}
```

## 🔧 Database Schema

### Core Entities
- **User** - User account information
- **Category** - Transaction categories
- **Transaction** - Financial transactions
- **AuditLog** - System audit trail

### Key Features
- **Soft Delete** - Logical deletion of records
- **Audit Logging** - Complete change tracking
- **Optimistic Concurrency** - Data consistency
- **Row Versioning** - Concurrent update handling

## 🧪 Testing

### Running Tests
```bash
# Unit Tests
dotnet test tests/PersonalFinanceTracker.Tests.Unit

# Integration Tests
dotnet test tests/PersonalFinanceTracker.Tests.Integration

# All Tests
dotnet test
```

## 📊 Project Status

| Feature | Status | Progress |
|---------|---------|----------|
| Core Architecture | ✅ Complete | 100% |
| Authentication | ✅ Complete | 100% |
| Basic CRUD | ✅ Complete | 100% |
| Advanced Features | 🟡 Partial | 60% |
| Analytics/Reports | 🔴 Planned | 0% |
| Testing | 🔴 Planned | 0% |
| Deployment | 🔴 Planned | 0% |

**Overall Progress: ~40% Complete**

## 🗺️ Roadmap

### Phase 1: Core Business Features
- [ ] Financial summary endpoints
- [ ] Advanced filtering & search
- [ ] Data seeding with sample data

### Phase 2: Background Processing
- [ ] Background worker service
- [ ] Excel export functionality
- [ ] Advanced analytics

### Phase 3: Production Readiness
- [ ] Comprehensive testing
- [ ] Logging & monitoring
- [ ] Performance optimization

### Phase 4: Deployment
- [ ] Docker containerization
- [ ] Redis cache integration
- [ ] Production deployment

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**Your Name**
- GitHub: [@yahyoHakimov](https://github.com/yahyoHakimov)
- Email: your.email@example.com

## 📞 Support

If you have any questions or need help with the project, please:
1. Check the [Issues](https://github.com/yahyoHakimov/PerconalFinanceTracker/issues) page
2. Create a new issue if your question isn't already answered
3. Contact me directly via email

---

⭐ **If you find this project helpful, please give it a star!** ⭐