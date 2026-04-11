# 📊 Shopfinity Database Schema

Complete database documentation for the Shopfinity eCommerce platform.

## 📋 Table of Contents

- [Overview](#overview)
- [Entity Relationship Diagram](#entity-relationship-diagram)
- [Tables](#tables)
- [Indexes](#indexes)
- [Extensions](#extensions)
- [SQL Schema](#sql-schema)
- [Seed Data](#seed-data)

---

## Overview

Shopfinity uses **PostgreSQL** as its primary database with:
- **Entity Framework Core** for ORM
- **ASP.NET Core Identity** for user management
- **pg_trgm** extension for fuzzy text search
- **Soft delete** pattern via `IsDeleted` flags

### Database Features

| Feature | Implementation |
|---------|---------------|
| Primary Keys | UUID (Guid) |
| Soft Delete | `IsDeleted` boolean flag |
| Auditing | `CreatedAt`, `UpdatedAt` timestamps |
| Full-Text Search | PostgreSQL `tsvector` |
| Fuzzy Search | `pg_trgm` extension |
| Concurrency | RowVersion (bytea) for products |

---

## Entity Relationship Diagram

```
┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐
│   AspNetUsers   │       │     Carts       │       │   CartItems     │
│   (Identity)    │◄──────┤                 │◄──────┤                 │
│                 │  1:M  │  UserId (FK)    │  1:M  │  CartId (FK)    │
└─────────────────┘       └─────────────────┘       │  ProductId (FK) │
         │                                          └─────────────────┘
         │                                                   │
         │                                                   ▼
         │                                          ┌─────────────────┐
         │                                          │    Products     │
         │                                          │                 │
         │                                          │  CategoryId(FK) │
         │                                          └─────────────────┘
         │                                                   │
         │                                                   ▼
         │                                          ┌─────────────────┐
         │                                          │   Categories    │
         │                                          │                 │
         │                                          └─────────────────┘
         │
         ▼
┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐
│    Orders         │◄──────│   OrderItems    │──────►│    Products     │
│   UserId (FK)     │  1:M  │  OrderId (FK)   │  M:1  │                 │
│                 │       │  ProductId (FK) │       │                 │
└─────────────────┘       └─────────────────┘       └─────────────────┘
         ▲
         │
         │
┌─────────────────┐
│  WishlistItems  │
│   UserId (FK)   │
│  ProductId (FK) │
└─────────────────┘

┌─────────────────┐
│  ProductReviews │
│   UserId (FK)   │
│  ProductId (FK) │
└─────────────────┘
```

---

## Tables

### 1. AspNetUsers (Identity)
ASP.NET Core Identity user table.

| Column | Type | Description |
|--------|------|-------------|
| Id | string (PK) | User ID |
| UserName | string | Username |
| NormalizedUserName | string | Normalized username |
| Email | string | Email address |
| NormalizedEmail | string | Normalized email |
| EmailConfirmed | boolean | Email confirmed status |
| PasswordHash | string | Hashed password |
| SecurityStamp | string | Security stamp |
| ConcurrencyStamp | string | Concurrency token |
| PhoneNumber | string | Phone number |
| PhoneNumberConfirmed | boolean | Phone confirmed |
| TwoFactorEnabled | boolean | 2FA enabled |
| LockoutEnd | timestamp | Lockout end time |
| LockoutEnabled | boolean | Lockout enabled |
| AccessFailedCount | integer | Failed login count |
| FirstName | string | User's first name |
| LastName | string | User's last name |

### 2. AspNetRoles (Identity)
Role definitions.

| Column | Type | Description |
|--------|------|-------------|
| Id | string (PK) | Role ID |
| Name | string | Role name |
| NormalizedName | string | Normalized name |
| ConcurrencyStamp | string | Concurrency token |

### 3. AspNetUserRoles (Identity)
User-role assignments.

| Column | Type | Description |
|--------|------|-------------|
| UserId | string (PK, FK) | User ID |
| RoleId | string (PK, FK) | Role ID |

### 4. Categories
Product categories.

| Column | Type | Description |
|--------|------|-------------|
| Id | uuid (PK) | Category ID |
| Name | string | Category name |
| Slug | string | URL-friendly name (unique) |
| Description | string | Category description |
| ImageUrl | string | Category image URL |
| DisplayOrder | integer | Sort order |
| IsDeleted | boolean | Soft delete flag |
| CreatedAt | timestamp | Creation time |
| UpdatedAt | timestamp | Last update time |

### 5. Products
Product catalog.

| Column | Type | Description |
|--------|------|-------------|
| Id | uuid (PK) | Product ID |
| Name | string | Product name (max 500) |
| Slug | string | URL-friendly name (unique) |
| Description | string | Product description |
| Price | decimal(18,2) | Product price |
| StockQuantity | integer | Available stock |
| ImageUrl | string | Product image URL |
| Tags | string[] | Product tags (array) |
| CategoryId | uuid (FK) | Category reference |
| SearchVector | tsvector | Full-text search vector |
| RowVersion | bytea | Concurrency token |
| IsDeleted | boolean | Soft delete flag |
| CreatedAt | timestamp | Creation time |
| UpdatedAt | timestamp | Last update time |

### 6. Carts
User shopping carts.

| Column | Type | Description |
|--------|------|-------------|
| Id | uuid (PK) | Cart ID |
| UserId | string (FK) | User reference |
| CreatedAt | timestamp | Creation time |
| UpdatedAt | timestamp | Last update time |

### 7. CartItems
Individual cart line items.

| Column | Type | Description |
|--------|------|-------------|
| Id | uuid (PK) | Cart item ID |
| CartId | uuid (FK) | Cart reference |
| ProductId | uuid (FK) | Product reference |
| Quantity | integer | Item quantity |
| CreatedAt | timestamp | Creation time |
| UpdatedAt | timestamp | Last update time |

**Constraints:**
- Unique: `(CartId, ProductId)` - prevents duplicate products in cart

### 8. Orders
Customer orders.

| Column | Type | Description |
|--------|------|-------------|
| Id | uuid (PK) | Order ID |
| UserId | string (FK) | User reference |
| TotalAmount | decimal(18,2) | Order total |
| Status | string | Order status (Pending/Processing/Shipped/Delivered/Cancelled) |
| IdempotencyKey | string | Prevent duplicate orders (unique, nullable) |
| IsDeleted | boolean | Soft delete flag |
| CreatedAt | timestamp | Creation time |
| UpdatedAt | timestamp | Last update time |

**Constraints:**
- Unique: `IdempotencyKey` - prevents duplicate checkouts

### 9. OrderItems
Order line items.

| Column | Type | Description |
|--------|------|-------------|
| Id | uuid (PK) | Order item ID |
| OrderId | uuid (FK) | Order reference |
| ProductId | uuid (FK) | Product reference |
| Quantity | integer | Item quantity |
| UnitPrice | decimal(18,2) | Price at time of order |
| CreatedAt | timestamp | Creation time |
| UpdatedAt | timestamp | Last update time |

**Constraints:**
- Foreign Key: `ProductId` → `Products(Id)` with **RESTRICT** delete (preserve order history)

### 10. WishlistItems
User wishlist entries.

| Column | Type | Description |
|--------|------|-------------|
| Id | uuid (PK) | Wishlist item ID |
| UserId | string (FK) | User reference |
| ProductId | uuid (FK) | Product reference |
| IsDeleted | boolean | Soft delete flag |
| CreatedAt | timestamp | Creation time |
| UpdatedAt | timestamp | Last update time |

**Constraints:**
- Unique: `(UserId, ProductId)` - prevents duplicate wishlist entries

### 11. ProductReviews
Product reviews and ratings.

| Column | Type | Description |
|--------|------|-------------|
| Id | uuid (PK) | Review ID |
| UserId | string (FK) | User reference |
| ProductId | uuid (FK) | Product reference |
| Rating | integer | Rating 1-5 |
| Title | string | Review title |
| Comment | string | Review text |
| IsDeleted | boolean | Soft delete flag |
| CreatedAt | timestamp | Creation time |
| UpdatedAt | timestamp | Last update time |

**Constraints:**
- Unique: `(UserId, ProductId)` - one review per user per product
- Check: `Rating >= 1 AND Rating <= 5`

### 12. RefreshTokens
JWT refresh tokens.

| Column | Type | Description |
|--------|------|-------------|
| Id | uuid (PK) | Token ID |
| Token | string | Refresh token value |
| Expires | timestamp | Expiration time |
| Created | timestamp | Creation time |
| Revoked | timestamp | Revocation time (nullable) |
| UserId | string (FK) | User reference |

---

## Indexes

### Performance Indexes

| Table | Column(s) | Type | Purpose |
|-------|-----------|------|---------|
| Categories | Slug | Unique | Fast lookup by slug |
| Products | Name | B-tree | Product search |
| Products | CategoryId | B-tree | Category filtering |
| Products | Slug | Unique | Fast product lookup |
| Products | SearchVector | GIN | Full-text search |
| Products | Name | GIN (trgm) | Fuzzy search |
| CartItems | (CartId, ProductId) | Unique | Prevent duplicates |
| Orders | IdempotencyKey | Unique | Prevent duplicate orders |
| Orders | UserId | B-tree | User order lookup |
| OrderItems | OrderId | B-tree | Order detail lookup |
| WishlistItems | (UserId, ProductId) | Unique | Prevent duplicates |
| ProductReviews | (UserId, ProductId) | Unique | One review per user |

---

## Extensions

### Required PostgreSQL Extensions

```sql
-- Enable pg_trgm for fuzzy text search
CREATE EXTENSION IF NOT EXISTS pg_trgm;
```

**Purpose**: Enables similarity matching for search suggestions (e.g., "app" matches "Apple").

---

## SQL Schema

### Complete Database Creation Script

```sql
-- =============================================
-- Shopfinity Database Schema
-- =============================================

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS pg_trgm;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =============================================
-- Categories
-- =============================================
CREATE TABLE "Categories" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Name" VARCHAR(200) NOT NULL,
    "Slug" VARCHAR(200) NOT NULL UNIQUE,
    "Description" TEXT,
    "ImageUrl" VARCHAR(500),
    "DisplayOrder" INTEGER DEFAULT 0,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE
);

CREATE INDEX "IX_Categories_Slug" ON "Categories" ("Slug");

-- =============================================
-- Products
-- =============================================
CREATE TABLE "Products" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Name" VARCHAR(500) NOT NULL,
    "Slug" VARCHAR(500) NOT NULL UNIQUE,
    "Description" TEXT,
    "Price" DECIMAL(18,2) NOT NULL,
    "StockQuantity" INTEGER NOT NULL DEFAULT 0,
    "ImageUrl" VARCHAR(500),
    "Tags" TEXT[],
    "CategoryId" UUID REFERENCES "Categories"("Id"),
    "SearchVector" TSVECTOR,
    "RowVersion" BYTEA,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE
);

CREATE INDEX "IX_Products_Name" ON "Products" ("Name");
CREATE INDEX "IX_Products_CategoryId" ON "Products" ("CategoryId");
CREATE INDEX "IX_Products_Slug" ON "Products" ("Slug");
CREATE INDEX "IX_Products_SearchVector" ON "Products" USING GIN ("SearchVector");
CREATE INDEX "IX_Products_Name_trgm" ON "Products" USING GIN ("Name" gin_trgm_ops);

-- Full-text search trigger
CREATE OR REPLACE FUNCTION products_search_vector_update()
RETURNS TRIGGER AS $$
BEGIN
    NEW."SearchVector" := 
        setweight(to_tsvector('english', coalesce(NEW."Name", '')), 'A') ||
        setweight(to_tsvector('english', coalesce(NEW."Description", '')), 'B');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER tsvectorupdate BEFORE INSERT OR UPDATE
ON "Products" FOR EACH ROW
EXECUTE FUNCTION products_search_vector_update();

-- =============================================
-- Carts
-- =============================================
CREATE TABLE "Carts" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" VARCHAR(450) NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE
);

CREATE INDEX "IX_Carts_UserId" ON "Carts" ("UserId");

-- =============================================
-- CartItems
-- =============================================
CREATE TABLE "CartItems" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "CartId" UUID NOT NULL REFERENCES "Carts"("Id") ON DELETE CASCADE,
    "ProductId" UUID NOT NULL REFERENCES "Products"("Id") ON DELETE CASCADE,
    "Quantity" INTEGER NOT NULL DEFAULT 1,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    UNIQUE ("CartId", "ProductId")
);

CREATE INDEX "IX_CartItems_CartId" ON "CartItems" ("CartId");
CREATE INDEX "IX_CartItems_ProductId" ON "CartItems" ("ProductId");

-- =============================================
-- Orders
-- =============================================
CREATE TABLE "Orders" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" VARCHAR(450) NOT NULL,
    "TotalAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Pending',
    "IdempotencyKey" VARCHAR(100) UNIQUE,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE
);

CREATE INDEX "IX_Orders_UserId" ON "Orders" ("UserId");
CREATE INDEX "IX_Orders_IdempotencyKey" ON "Orders" ("IdempotencyKey");

-- =============================================
-- OrderItems
-- =============================================
CREATE TABLE "OrderItems" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "OrderId" UUID NOT NULL REFERENCES "Orders"("Id") ON DELETE CASCADE,
    "ProductId" UUID NOT NULL REFERENCES "Products"("Id") ON DELETE RESTRICT,
    "Quantity" INTEGER NOT NULL,
    "UnitPrice" DECIMAL(18,2) NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE
);

CREATE INDEX "IX_OrderItems_OrderId" ON "OrderItems" ("OrderId");
CREATE INDEX "IX_OrderItems_ProductId" ON "OrderItems" ("ProductId");

-- =============================================
-- WishlistItems
-- =============================================
CREATE TABLE "WishlistItems" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" VARCHAR(450) NOT NULL,
    "ProductId" UUID NOT NULL REFERENCES "Products"("Id") ON DELETE CASCADE,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    UNIQUE ("UserId", "ProductId")
);

CREATE INDEX "IX_WishlistItems_UserId" ON "WishlistItems" ("UserId");
CREATE INDEX "IX_WishlistItems_ProductId" ON "WishlistItems" ("ProductId");

-- =============================================
-- ProductReviews
-- =============================================
CREATE TABLE "ProductReviews" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" VARCHAR(450) NOT NULL,
    "ProductId" UUID NOT NULL REFERENCES "Products"("Id") ON DELETE CASCADE,
    "Rating" INTEGER NOT NULL CHECK ("Rating" >= 1 AND "Rating" <= 5),
    "Title" VARCHAR(200) NOT NULL,
    "Comment" TEXT,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    UNIQUE ("UserId", "ProductId")
);

CREATE INDEX "IX_ProductReviews_ProductId" ON "ProductReviews" ("ProductId");
CREATE INDEX "IX_ProductReviews_UserId" ON "ProductReviews" ("UserId");

-- =============================================
-- RefreshTokens
-- =============================================
CREATE TABLE "RefreshTokens" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Token" VARCHAR(500) NOT NULL,
    "Expires" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Created" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "Revoked" TIMESTAMP WITH TIME ZONE,
    "UserId" VARCHAR(450) NOT NULL
);

CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");
CREATE INDEX "IX_RefreshTokens_Token" ON "RefreshTokens" ("Token");
```

---

## Seed Data

### Initial Categories

```sql
INSERT INTO "Categories" ("Id", "Name", "Slug", "Description", "DisplayOrder", "IsDeleted", "CreatedAt")
VALUES 
    ('11111111-1111-1111-1111-111111111111', 'Laptops', 'laptops', 'High-performance laptops for work and gaming', 1, FALSE, NOW()),
    ('22222222-2222-2222-2222-222222222222', 'Smartphones', 'smartphones', 'Latest flagship smartphones', 2, FALSE, NOW()),
    ('33333333-3333-3333-3333-333333333333', 'Audio', 'audio', 'Premium headphones and speakers', 3, FALSE, NOW()),
    ('44444444-4444-4444-4444-444444444444', 'Wearables', 'wearables', 'Smartwatches and fitness trackers', 4, FALSE, NOW());
```

### Sample Products

```sql
-- Laptops
INSERT INTO "Products" ("Id", "Name", "Slug", "Description", "Price", "StockQuantity", "CategoryId", "ImageUrl", "IsDeleted", "CreatedAt")
VALUES 
    (uuid_generate_v4(), 'MacBook Pro 16"', 'macbook-pro-16', 'Apple M3 Pro chip, 16GB RAM, 512GB SSD', 2499.99, 15, '11111111-1111-1111-1111-111111111111', '/uploads/macbook-pro.jpg', FALSE, NOW()),
    (uuid_generate_v4(), 'Dell XPS 15', 'dell-xps-15', 'Intel Core i7, 32GB RAM, 1TB SSD, OLED Display', 1899.99, 8, '11111111-1111-1111-1111-111111111111', '/uploads/dell-xps.jpg', FALSE, NOW());

-- Smartphones
INSERT INTO "Products" ("Id", "Name", "Slug", "Description", "Price", "StockQuantity", "CategoryId", "ImageUrl", "IsDeleted", "CreatedAt")
VALUES 
    (uuid_generate_v4(), 'iPhone 15 Pro', 'iphone-15-pro', 'A17 Pro chip, 256GB, Titanium design', 999.99, 25, '22222222-2222-2222-2222-222222222222', '/uploads/iphone-15-pro.jpg', FALSE, NOW()),
    (uuid_generate_v4(), 'Samsung Galaxy S24', 'samsung-galaxy-s24', 'AI-powered camera, 256GB, 8K video', 899.99, 20, '22222222-2222-2222-2222-222222222222', '/uploads/galaxy-s24.jpg', FALSE, NOW());
```

---

## Migration Notes

### Using Entity Framework

The project uses **EF Core Migrations**. To create a new migration:

```bash
dotnet ef migrations add MigrationName \
  --project Shopfinity.Infrastructure \
  --startup-project Shopfinity.API
```

To apply migrations:
```bash
dotnet ef database update \
  --project Shopfinity.Infrastructure \
  --startup-project Shopfinity.API \
  --connection "your-connection-string"
```

To generate SQL script:
```bash
dotnet ef migrations script \
  --project Shopfinity.Infrastructure \
  --startup-project Shopfinity.API \
  -o output.sql
```

---

## Backup & Restore

### Backup Database

```bash
# Using pg_dump
pg_dump -h localhost -U postgres -d shopfinity -F c -f shopfinity-backup.dump

# Plain SQL
pg_dump -h localhost -U postgres -d shopfinity > shopfinity-backup.sql
```

### Restore Database

```bash
# From custom format
pg_restore -h localhost -U postgres -d shopfinity shopfinity-backup.dump

# From SQL
psql -h localhost -U postgres -d shopfinity < shopfinity-backup.sql
```

---

## Performance Tuning

### Query Optimization Tips

1. **Always use indexes on foreign keys** - EF Core does this automatically
2. **Enable query caching** for frequently accessed data
3. **Use `.AsNoTracking()`** for read-only queries
4. **Filter early** - apply `Where()` before `Include()`

### Connection Pooling

Configure in connection string:
```
Host=localhost;Database=shopfinity;Username=postgres;Password=***;Pooling=true;MaxPoolSize=100;
```

---

For questions or issues, refer to the main [README.md](./README.md) or open a GitHub issue.
