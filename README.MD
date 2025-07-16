# Personal Finance Tracker

Shaxsiy moliyaviy boshqaruv uchun to'liq RESTful API. Bu xavfsiz, monitoring va distributed arxitekturaga ega tizim.

## 🎯 Interview uchun bajarilgan loyiha maqsadi

Foydalanuvchilarga real production muhitiga yaqin, xavfsiz, monitoring va distributed arxitekturaga ega RESTful API yaratish. Kengaytirilgan biznes qoidalari, kesh, audit log, testlar va container texnologiyalarini qamrab oladi.

## ⚡ Tezkor ishga tushirish. Internet tezligi yaxshi bo'lmasa shu usuli qulay :)

### 1-usul: Development Setup ()

```bash
# Infrastructure xizmatlarini ishga tushiring
docker-compose -f docker-compose.fast.yml up -d

# API ni local ishga tushiring
cd src/PersonalFinanceTracker.API
dotnet ef database update
dotnet run

# Worker ni ishga tushiring (ixtiyoriy)
cd src/PersonalFinanceTracker.Worker
dotnet run
```

### 2-usul: To'liq Docker setup. Bu Docker hamma narsani build qilguncha biroz vaqt oladi

```bash
# Barcha xizmatlarni ishga tushiring
docker-compose up --build
```

### 3-usul: Agar hech qaysi biri ish bermasa:

# Server side
```bash
cd \src
```
```bash
cd \PersonalFinanceTracker.API

dotnet run build
```

#Client side
```bash
cd \PersonalFinanceTracker.UI
```
```bash
npm install 

npm run dev
```

## 🔧 Texnik stack

- **Backend**: ASP.NET Core 8.0, C#
- **Database**: PostgreSQL 15
- **Cache**: Redis 7
- **ORM**: Entity Framework Core
- **Auth**: JWT + Refresh Tokens
- **Testing**: xUnit
- **Logging**: Serilog
- **Containerization**: Docker Compose

## 📋 Amalga oshirilgan talablar

### ✅ Asosiy funksiyalar
- **ASP.NET Core Web API** loyihasi
- **JWT + Refresh token** autentifikatsiya
- **RBAC** (admin va oddiy foydalanuvchi rollari)
- **Transaction CRUD** operatsiyalari
- **Category CRUD** operatsiyalari
- **Filterlash, sortlash, pagination**
- **Oylik moliyaviy summary**
- **Statistika API** va Excel export (worker orqali)
- **Soft delete** mexanizmi
- **Audit log** barcha operatsiyalar uchun
- **Optimistic concurrency** (RowVersion)
- **Background worker** xizmati
- **Redis caching**
- **Structured logging** (Serilog)
- **Health check** endpointlar
- **Docker Compose** konfiguratsiyasi
- **Unit va integration testlar**
- **Swagger** dokumentatsiya
- **Clean architecture** (Controller → Service → Repository)

### 🗂️ Modellar
- **User**: Id, Username, Email, PasswordHash, Role
- **Category**: Id, Name, Color, UserId  
- **Transaction**: Id, Amount, Type, CategoryId, UserId, CreatedAt, Note, RowVersion
- **AuditLog**: Id, UserId, Action, EntityName, EntityId, CreatedAt, OldValue, NewValue

## 🌐 API Endpointlar

### Autentifikatsiya
- `POST /api/auth/register` - Ro'yxatdan o'tish
- `POST /api/auth/login` - Kirish
- `POST /api/auth/refresh` - Token yangilash

### Tranzaksiyalar
- `GET /api/transactions` - Tranzaksiyalar ro'yxati (filter, sort, pagination)
- `POST /api/transactions` - Yangi tranzaksiya
- `PUT /api/transactions/{id}` - Tranzaksiyani tahrirlash
- `DELETE /api/transactions/{id}` - Tranzaksiyani o'chirish

### Kategoriyalar
- `GET /api/categories` - Kategoriyalar ro'yxati
- `POST /api/categories` - Yangi kategoriya
- `PUT /api/categories/{id}` - Kategoriyani tahrirlash
- `DELETE /api/categories/{id}` - Kategoriyani o'chirish

### Statistika
- `GET /api/analytics/summary` - Oylik moliyaviy xulose
- `GET /api/analytics/trends` - Xarajat tendentsiyalari
- `GET /api/analytics/categories` - Kategoriya bo'yicha xarajatlar
- `GET /api/analytics/export` - Excel hisobot

### Monitoring
- `GET /health` - Tizim holati
- `GET /health/ready` - Tayyor holati
- `GET /health/live` - Jonli holati

## 🔧 Konfiguratsiya

### Ma'lumotlar bazasi
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=PersonalFinanceTracker;Username=postgres;Password=postgres123"
  }
}
```

### Redis
```json
{
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

## 🧪 Testlar

```bash
# Barcha testlarni ishga tushirish
dotnet test

# Test coverage bilan
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 Monitoring

- **Health Dashboard**: `http://localhost:8090`
- **API**: `http://localhost:8080`
- **Swagger**: `http://localhost:8080/swagger`

## 🐳 Docker Setup

### Nega Hybrid Deployment?

Bu loyiha **hybrid containerization** yondashuvini ishlatadi:
- **Infrastructure xizmatlar** (PostgreSQL, Redis) Docker'da - izchillik uchun
- **Application xizmatlar** (API, Worker) local - tez development uchun
- **To'liq containerization** 

### Afzalliklar:
- ⚡ **Tez development** (container qayta build kerak emas)
- 🔧 **Oson debugging** IDE integratsiyasi bilan
- 🏗️ **Production parity** infrastructure uchun Docker orqali
- 📈 **Scalable architecture** orchestration uchun tayyor

## 🚀 Kirish

### Xizmatlarni tekshirish
```bash
# Containerlar holatini tekshirish
docker ps

# Database ulanishini tekshirish
docker exec -it pft_postgres_dev psql -U postgres -d PersonalFinanceTracker

# Redis ulanishini tekshirish
docker exec -it pft_redis_dev redis-cli ping
```

### Muammolarni hal qilish
1. **Port conflicts**: docker-compose.yml da portlarni o'zgartiring
2. **Database migrations**: `dotnet ef database update` ni ishga tushiring
3. **Missing packages**: `dotnet restore` ni har bir loyihada ishga tushiring

## 📝 Loyiha strukturasi

```
PersonalFinanceTracker/
├── src/
│   ├── PersonalFinanceTracker.API/          # Web API Layer
│   ├── PersonalFinanceTracker.Application/   # Business Logic
│   ├── PersonalFinanceTracker.Domain/        # Domain Models
│   ├── PersonalFinanceTracker.Infrastructure/ # Data Access
│   ├── PersonalFinanceTracker.Worker/        # Background Services
│   └── PersonalFinanceTracker.UI/            # Vue.js Frontend
├── tests/                                   # Test Projects
├── docker-compose.yml                       # Production Docker
├── docker-compose.fast.yml                  # Development Docker
└── README.md
