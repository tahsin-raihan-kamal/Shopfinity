# 🛍️ Shopfinity – Full Stack eCommerce Platform

[![.NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Next.js](https://img.shields.io/badge/Next.js-15-000000?logo=next.js)](https://nextjs.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16+-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![React Query](https://img.shields.io/badge/React%20Query-5-FF4154?logo=reactquery)](https://tanstack.com/query)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind-4-06B6D4?logo=tailwindcss)](https://tailwindcss.com/)

## 🚀 Overview

**Shopfinity** is a production-ready full-stack eCommerce platform featuring a modern **Next.js** storefront and a robust **ASP.NET Core** REST API. Built with clean architecture principles, it delivers a complete online shopping experience with real-time inventory management, secure authentication, and powerful search capabilities.

### Key Highlights

- 🔐 **JWT + Cookie Authentication** with CSRF protection
- 🔍 **Fuzzy Search** with PostgreSQL `pg_trgm` for typo-tolerant product discovery
- 🛒 **Real-time Cart & Wishlist** with optimistic UI updates
- 📦 **Inventory-aware Checkout** with transaction safety
- 👨‍💼 **Admin Dashboard** for product and order management
- 📱 **Responsive Design** with Tailwind CSS

## 🧠 Tech Stack

### Frontend (`shopfinity-web/`)
| Technology | Purpose |
|------------|---------|
| [Next.js 15](https://nextjs.org/) | React framework with App Router |
| [TypeScript](https://www.typescriptlang.org/) | Type-safe development |
| [TanStack Query](https://tanstack.com/query) | Server state management |
| [Tailwind CSS](https://tailwindcss.com/) | Utility-first styling |
| [Axios](https://axios-http.com/) | HTTP client with cookie support |
| [React Hook Form](https://react-hook-form.com/) | Form handling |
| [Zod](https://zod.dev/) | Schema validation |
| [React Hot Toast](https://react-hot-toast.com/) | Notifications |

### Backend
| Technology | Purpose |
|------------|---------|
| [ASP.NET Core 8](https://dotnet.microsoft.com/) | Web API framework |
| [Entity Framework Core](https://docs.microsoft.com/ef/) | ORM with PostgreSQL |
| [ASP.NET Core Identity](https://docs.microsoft.com/aspnet/core/security/authentication/identity) | User management |
| [FluentValidation](https://docs.fluentvalidation.net/) | Input validation |
| [AutoMapper](https://automapper.org/) | Object mapping |
| [Serilog](https://serilog.net/) | Structured logging |
| [Npgsql](https://www.npgsql.org/) | PostgreSQL driver |

### Database
- **PostgreSQL** with `pg_trgm` extension for fuzzy search
- Full-text search with GIN indexes
- Row-level security considerations

## 📁 Repository Structure

```
shopfinity/
├── 📁 Shopfinity.API/                 # HTTP API, middleware, DI configuration
│   ├── Controllers/v1/                # REST API endpoints
│   ├── Middleware/                    # Exception handling, CSRF, correlation IDs
│   └── Responses/                     # Standardized API response models
│
├── 📁 Shopfinity.Application/         # Business logic, use cases, DTOs
│   ├── Common/                        # Shared abstractions, exceptions
│   └── Features/                      # Feature-organized modules
│       ├── Auth/
│       ├── Carts/
│       ├── Categories/
│       ├── Orders/
│       ├── Products/
│       ├── Reviews/
│       ├── Uploads/
│       └── Wishlists/
│
├── 📁 Shopfinity.Domain/              # Core entities, enums, constants
│   ├── Entities/                      # Domain models
│   ├── Common/                        # Base entity classes
│   └── Constants/                     # Role definitions
│
├── 📁 Shopfinity.Infrastructure/      # Data access, Identity, external services
│   ├── Data/                          # DbContext, migrations, seeding
│   ├── Identity/                      # ApplicationUser, JWT services
│   ├── Migrations/                    # EF Core migration files
│   └── Services/                      # File upload, image handling
│
├── 📁 shopfinity-web/                 # Next.js frontend application
│   ├── app/                           # App Router pages
│   │   ├── (routes)/                  # Public routes
│   │   ├── admin/                     # Admin dashboard
│   │   └── api/                       # Next.js API routes (auth proxy)
│   ├── components/                    # React components
│   ├── hooks/                         # Custom React Query hooks
│   ├── services/                      # API service layer
│   ├── types/                         # TypeScript type definitions
│   └── lib/                           # Utilities, axios config
│
└── 📁 Shopfinity.Tests/               # Unit & integration tests
```

## ✨ Feature Details

### 🔐 Authentication & Security
- **JWT Tokens** stored in HttpOnly cookies for XSS protection
- **CSRF Protection** on mutating requests via `XSRF-TOKEN` cookie
- **Rate Limiting** on search (20/10s), auth (5/min), reviews (3/5min)
- **Password Policy**: 8+ chars, uppercase, digit required

### 🔍 Search System
- **Live Suggestions** in navbar with 300ms debounce
- **Fuzzy Matching** using PostgreSQL `pg_trgm` similarity
- **Full-Text Search** with tsvector ranking
- **Category Filtering** by slug or ID
- **Price Range Filtering**

### 🛒 Cart & Checkout
- **Server-Side Cart** persisted per user
- **Inventory Validation** before checkout
- **Optimistic UI** updates for instant feedback
- **Transaction Safety** with database transactions
- **Idempotency Keys** prevent duplicate orders

### 👨‍💼 Admin Features
- Product CRUD with image uploads
- Category management
- Order status tracking
- Sales dashboard (planned)

## ⚙️ Local Development Setup

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) 18+ (LTS recommended)
- [PostgreSQL](https://www.postgresql.org/download/) 14+

### 1. Clone & Navigate
```bash
git clone https://github.com/mhdnazrul/shopfinity.git
cd shopfinity
```

### 2. Database Setup

Create PostgreSQL database:
```sql
CREATE DATABASE shopfinity;
```

Configure connection in `Shopfinity.API/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=shopfinity;Username=postgres;Password=YOUR_PASSWORD"
  },
  "JwtSettings": {
    "Key": "your-super-secret-key-at-least-32-characters-long",
    "Issuer": "Shopfinity",
    "Audience": "Shopfinity.Client",
    "ExpiryMinutes": 60
  }
}
```

Apply migrations:
```bash
dotnet ef database update --project Shopfinity.Infrastructure --startup-project Shopfinity.API
```

### 3. Run Backend
```bash
dotnet run --project Shopfinity.API --launch-profile http
# API available at: http://localhost:5049
```

### 4. Run Frontend
```bash
cd shopfinity-web

# Create environment file
cp .env.example .env.local
# Edit .env.local: NEXT_PUBLIC_API_URL=http://localhost:5049

npm install
npm run dev
# App available at: http://localhost:3000
```

### 5. Verify Setup
```bash
# Run all tests
dotnet test

# Build frontend
cd shopfinity-web && npm run build
```

## 🔐 Demo Accounts

After first run, the database seeder creates:

| Role | Email | Password |
|------|-------|----------|
| 👨‍💼 Admin | `admin@shopfinity.com` | `Admin123!` |
| 👤 Customer | `test@shopfinity.com` | `Test123!` |

> ⚠️ **Change these in production!**

## 🔌 API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/Auth/register` | Register new user |
| POST | `/api/v1/Auth/login` | Authenticate user |
| POST | `/api/v1/Auth/logout` | Sign out |
| POST | `/api/v1/Auth/refresh` | Refresh JWT token |

### Products
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/Products` | Search/filter products |
| GET | `/api/v1/Products/search?q={term}` | Search suggestions |
| GET | `/api/v1/Products/{slug}` | Get product details |
| POST | `/api/v1/Products` | Create product (Admin) |
| PUT | `/api/v1/Products/{id}` | Update product (Admin) |
| DELETE | `/api/v1/Products/{id}` | Delete product (Admin) |

### Cart
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/Carts` | Get user's cart |
| POST | `/api/v1/Carts/items` | Add item to cart |
| DELETE | `/api/v1/Carts/items/{id}` | Remove item from cart |

### Orders
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/Orders` | Get my orders |
| POST | `/api/v1/Orders/checkout` | Place order |
| GET | `/api/v1/Orders/admin/all` | Get all orders (Admin) |
| PUT | `/api/v1/Orders/{id}/status` | Update order status (Admin) |

### Wishlist
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/Wishlists` | Get my wishlist |
| POST | `/api/v1/Wishlists` | Add to wishlist |
| DELETE | `/api/v1/Wishlists/{id}` | Remove from wishlist |

## 🚀 Deployment

See [DEPLOYMENT.md](./DEPLOYMENT.md) for detailed deployment instructions for:
- **Vercel** (Frontend)
- **Render/Railway** (Backend)
- **Supabase** (PostgreSQL)

## 📝 Environment Variables

### Frontend (`shopfinity-web/.env.local`)
| Variable | Required | Description |
|----------|----------|-------------|
| `NEXT_PUBLIC_API_URL` | ✅ | Backend API URL (no trailing slash) |

### Backend (`Shopfinity.API/appsettings.json`)
| Variable | Required | Description |
|----------|----------|-------------|
| `ConnectionStrings:DefaultConnection` | ✅ | PostgreSQL connection string |
| `JwtSettings:Key` | ✅ | JWT signing key (32+ chars) |
| `JwtSettings:Issuer` | ✅ | Token issuer |
| `JwtSettings:Audience` | ✅ | Token audience |

## 🛠️ Common Issues & Fixes

### Git Submodule Issue
If `shopfinity-web` appears as a submodule but isn't configured:
```bash
# Remove nested git repository
cd shopfinity-web
rm -rf .git
cd ..

# Re-add to parent repository
git add shopfinity-web
git commit -m "Fix nested git repository"
```

### Database Migrations
If migrations fail:
```bash
# Drop and recreate database
dotnet ef database drop --project Shopfinity.Infrastructure --startup-project Shopfinity.API
dotnet ef database update --project Shopfinity.Infrastructure --startup-project Shopfinity.API
```

### pg_trgm Extension
If fuzzy search doesn't work:
```sql
CREATE EXTENSION IF NOT EXISTS pg_trgm;
```

## 📊 Database Schema

See [DATABASE.md](./DATABASE.md) for complete SQL schema documentation.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Please ensure:
- `dotnet test` passes
- `npm run build` succeeds in `shopfinity-web/`
- No console errors in browser

## 📄 License

Distributed under the MIT License. See [LICENSE](./LICENSE) for details.

## 👤 Author

**Nazrul Islam**
- 📧 Email: mhdnazrul511@gmail.com
- 🐙 GitHub: [https://github.com/mhdnazrul](https://github.com/mhdnazrul)
- 💼 LinkedIn: [https://linkedin.com/nazrulislam7](https://linkedin.com/nazrulislam7)

---
** roki **

<p align="center">
  <i>Built with ❤️ using Next.js, ASP.NET Core, and PostgreSQL</i>
</p>
