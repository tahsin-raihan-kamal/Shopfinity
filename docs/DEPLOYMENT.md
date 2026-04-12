# 🚀 Shopfinity Deployment Guide

Complete deployment instructions for production environments.

## 📋 Table of Contents

- [Overview](#overview)
- [Database (Supabase)](#database-supabase)
- [Backend (Render/Railway)](#backend-renderrailway)
- [Frontend (Vercel)](#frontend-vercel)
- [Environment Variables](#environment-variables)
- [Post-Deployment Checklist](#post-deployment-checklist)
- [Troubleshooting](#troubleshooting)

---

## Overview

Recommended deployment architecture:

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│   Vercel        │────▶│   Render/Railway │────▶│   Supabase      │
│  (Frontend)     │     │    (Backend)     │     │  (PostgreSQL)   │
│  Next.js        │     │  ASP.NET Core    │     │   + Storage     │
└─────────────────┘     └──────────────────┘     └─────────────────┘
        │                        │
        │                        │
        └────────── JWT ─────────┘
               (HttpOnly Cookies)
```

---

## Database (Supabase)

### 1. Create Supabase Project

1. Go to [https://supabase.com](https://supabase.com)
2. Sign up/login with GitHub
3. Click "New Project"
4. Choose organization, name your project (e.g., `shopfinity-db`)
5. Select region closest to your users
6. Wait for database provisioning (2-3 minutes)

### 2. Get Connection String

1. In Supabase Dashboard, go to **Settings** → **Database**
2. Find **Connection string** section
3. Copy the **URI** connection string:
   ```
   postgresql://postgres:[YOUR-PASSWORD]@db.[PROJECT-REF].supabase.co:5432/postgres
   ```
4. Save this for later

### 3. Enable pg_trgm Extension

1. Go to **SQL Editor** in Supabase Dashboard
2. Run:
   ```sql
   CREATE EXTENSION IF NOT EXISTS pg_trgm;
   ```
3. Click **Run**

### 4. Run Migrations

Option A: Using EF Core CLI (local → remote):
```bash
# Set connection string temporarily
export CONNECTION_STRING="postgresql://postgres:[PASSWORD]@db.[REF].supabase.co:5432/postgres"

dotnet ef database update \
  --project Shopfinity.Infrastructure \
  --startup-project Shopfinity.API \
  --connection "$CONNECTION_STRING"
```

Option B: Using SQL Editor (run migrations manually):
```bash
# Generate SQL script
dotnet ef migrations script \
  --project Shopfinity.Infrastructure \
  --startup-project Shopfinity.API \
  -o migration-script.sql
```
Then copy/paste the SQL into Supabase SQL Editor.

---

## Backend (Render/Railway)

### Option A: Render Deployment

#### 1. Prepare Repository

Ensure your repo has:
- `Shopfinity.API/Shopfinity.API.csproj` at root level
- `Shopfinity.sln` in repository root

#### 2. Create Web Service

1. Go to [https://render.com](https://render.com)
2. Click **New** → **Web Service**
3. Connect your GitHub repository
4. Configure:
   
   | Setting | Value |
   |---------|-------|
   | Name | `shopfinity-api` |
   | Runtime | `.NET` |
   | Build Command | `dotnet restore && dotnet build` |
   | Start Command | `dotnet Shopfinity.API/bin/Release/net8.0/Shopfinity.API.dll` |
   | Plan | Free / Starter |

#### 3. Environment Variables

Add these in Render Dashboard → Environment:

```bash
# Database
ConnectionStrings__DefaultConnection=postgresql://postgres:[PASSWORD]@db.[REF].supabase.co:5432/postgres

# JWT
JwtSettings__Key=your-super-secret-key-min-32-chars-long
JwtSettings__Issuer=Shopfinity
JwtSettings__Audience=Shopfinity.Client
JwtSettings__ExpiryMinutes=60

# ASP.NET
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000
```

#### 4. CORS Configuration

Update `Program.cs` or use environment variable to allow your Vercel domain:

```csharp
// In Program.cs, update CORS policy:
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "https://your-app.vercel.app"  // Add your Vercel domain
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});
```

### Option B: Railway Deployment

#### 1. Install Railway CLI (optional)
```bash
npm install -g @railway/cli
railway login
```

#### 2. Create Project

Via Dashboard:
1. Go to [https://railway.app](https://railway.app)
2. Click **New Project**
3. Select **Deploy from GitHub repo**
4. Choose your repository

#### 3. Configure Service

1. Click on the deployed service
2. Go to **Settings** → **Build**
3. Set:
   - **Build Command**: `dotnet restore && dotnet build -c Release`
   - **Start Command**: `dotnet Shopfinity.API/bin/Release/net8.0/Shopfinity.API.dll`

#### 4. Add Variables

In **Variables** tab, add the same environment variables as Render (above).

---

## Frontend (Vercel)

### 1. Prepare Frontend

Ensure `shopfinity-web/.env.local` is in `.gitignore` and NOT committed.

Update `shopfinity-web/next.config.ts`:
```typescript
const nextConfig: NextConfig = {
  output: 'standalone',
  images: {
    remotePatterns: [
      { protocol: 'https', hostname: '**' },
      { protocol: 'http', hostname: 'localhost', port: '5049' },
      // Add your production backend:
      { protocol: 'https', hostname: 'your-api.onrender.com' },
    ],
  },
};
```

### 2. Deploy to Vercel

#### Via Dashboard:
1. Go to [https://vercel.com](https://vercel.com)
2. Click **Add New Project**
3. Import your GitHub repository
4. Configure:
   
   | Setting | Value |
   |---------|-------|
   | Framework Preset | Next.js |
   | Root Directory | `shopfinity-web` |
   | Build Command | `npm run build` |
   | Output Directory | `.next` |

5. Add Environment Variables:
   ```
   NEXT_PUBLIC_API_URL=https://your-api.onrender.com
   ```

6. Click **Deploy**

#### Via CLI:
```bash
cd shopfinity-web

# Install Vercel CLI if needed
npm i -g vercel

# Deploy
vercel

# For production
vercel --prod
```

### 3. Configure Custom Domain (Optional)

1. In Vercel Dashboard, select your project
2. Go to **Settings** → **Domains**
3. Add your custom domain
4. Follow DNS configuration instructions

---

## Environment Variables

### Production Environment Variables Summary

#### Backend (Render/Railway)
```bash
# Database
ConnectionStrings__DefaultConnection=postgresql://postgres:[PASSWORD]@db.[REF].supabase.co:5432/postgres

# JWT (generate secure random key)
JwtSettings__Key=YourSuperSecureRandomKeyHere32CharMin!
JwtSettings__Issuer=Shopfinity
JwtSettings__Audience=Shopfinity.Client
JwtSettings__ExpiryMinutes=60

# ASP.NET
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000

# Optional: Logging
Serilog__MinimumLevel__Default=Information
```

#### Frontend (Vercel)
```bash
NEXT_PUBLIC_API_URL=https://your-backend-url.onrender.com
```

---

## Post-Deployment Checklist

### 🔐 Security
- [ ] JWT key is unique and secure (32+ random characters)
- [ ] Database password is strong
- [ ] CORS is configured for production domain only
- [ ] No sensitive data in Git (check `.env` files are ignored)
- [ ] HTTPS enabled for all services

### 🗄️ Database
- [ ] `pg_trgm` extension enabled
- [ ] All migrations applied successfully
- [ ] Seed data created (check demo accounts work)
- [ ] Connection string uses SSL

### 🧪 Functionality
- [ ] User registration/login works
- [ ] Products load correctly
- [ ] Cart add/remove works
- [ ] Checkout creates orders
- [ ] Search suggestions appear
- [ ] Wishlist add/remove works
- [ ] Admin panel accessible (for admin users)
- [ ] Image uploads work (if configured)

### 📊 Monitoring
- [ ] Backend logs are visible (Render/Railway dashboard)
- [ ] Error tracking configured (optional: Sentry)
- [ ] Health check endpoint responding (`/health`)

---

## Troubleshooting

### CORS Errors
**Symptom**: Browser console shows CORS errors

**Fix**: Update backend CORS to include Vercel domain:
```csharp
policy.WithOrigins(
    "https://your-app.vercel.app",
    "https://www.your-domain.com"
)
```

### Database Connection Failed
**Symptom**: "Connection refused" or timeout errors

**Fix**:
1. Verify connection string format
2. Check Supabase project is active
3. Ensure IP allowlist includes Render/Railway IPs
4. Try with `SSL Mode=require` in connection string:
   ```
   postgresql://...supabase.co:5432/postgres?sslmode=require
   ```

### 401 Unauthorized on API
**Symptom**: All authenticated requests fail

**Fix**:
1. Check JWT key matches between environments
2. Verify cookies are being sent (`withCredentials: true` in axios)
3. Check cookie domain settings

### Images Not Loading
**Symptom**: Product images show placeholder

**Fix**:
1. Add backend domain to `next.config.ts` remotePatterns
2. Verify `NEXT_PUBLIC_API_URL` is correct
3. Check image URLs are being constructed correctly

### Build Failures
**Symptom**: `npm run build` or `dotnet build` fails

**Fix**:
```bash
# Frontend
cd shopfinity-web
rm -rf node_modules .next
npm install
npm run build

# Backend
dotnet clean
dotnet restore
dotnet build
```

---

## 📚 Additional Resources

- [Supabase Documentation](https://supabase.com/docs)
- [Render Documentation](https://render.com/docs)
- [Railway Documentation](https://docs.railway.app/)
- [Vercel Documentation](https://vercel.com/docs)
- [.NET 8 Deployment Guide](https://docs.microsoft.com/aspnet/core/host-and-deploy/)
- [Next.js Deployment](https://nextjs.org/docs/deployment)

---

## 🆘 Support

If you encounter issues:
1. Check service logs in respective dashboards
2. Verify all environment variables are set
3. Test API endpoints with Postman/Insomnia
4. Review browser console for frontend errors

For project-specific issues, please open a GitHub issue with:
- Environment details
- Error messages
- Steps to reproduce
