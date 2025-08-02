# 🚂 Railway PostgreSQL Connection - Step by Step

## **Quick Railway Setup**

### **1. Create PostgreSQL Database in Railway**

1. **Login to Railway**: https://railway.app
2. **Go to your project** (or create a new one)
3. **Click "New Service"**
4. **Select "Database" → "PostgreSQL"**
5. **Wait for Railway to provision the database** (takes 1-2 minutes)

### **2. Get Your Connection Details**

1. **Click on the PostgreSQL service** in your Railway dashboard
2. **Click the "Variables" tab**
3. **Copy these values:**

```
PGHOST=containers-us-west-123.railway.app
PGPORT=5432
PGDATABASE=railway
PGUSER=postgres
PGPASSWORD=abc123xyz456
```

### **3. Build Your Connection String**

**Template:**
```
Host=PGHOST;Port=PGPORT;Database=PGDATABASE;Username=PGUSER;Password=PGPASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

**Example with Railway values:**
```
Host=containers-us-west-123.railway.app;Port=5432;Database=railway;Username=postgres;Password=abc123xyz456;SSL Mode=Require;Trust Server Certificate=true
```

### **4. Update appsettings.Development.json**

Replace the `DefaultConnection` in your `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=containers-us-west-123.railway.app;Port=5432;Database=railway;Username=postgres;Password=abc123xyz456;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

### **5. Test the Connection**

```bash
dotnet run
```

**Look for this console output:**
```
Using PostgreSQL database
```

### **6. Apply Database Migrations**

```bash
dotnet ef database update
```

## **🔥 Quick Copy-Paste Template**

**Replace the placeholders with your Railway values:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_PGHOST;Port=5432;Database=railway;Username=postgres;Password=YOUR_PGPASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

## **📋 Railway Dashboard Screenshots Guide**

### **Finding Your PostgreSQL Variables:**

1. **Railway Dashboard** → **Your Project**
2. **PostgreSQL Service** → **Variables Tab**
3. **Look for these variables:**
   - `PGHOST` - Your database host
   - `PGPASSWORD` - Your database password
   - `PGDATABASE` - Usually "railway"
   - `PGUSER` - Usually "postgres"
   - `PGPORT` - Usually "5432"

### **Alternative: Use DATABASE_URL Directly**

Railway also provides a `DATABASE_URL` variable in this format:
```
postgres://postgres:password@host:5432/railway
```

You can use this directly as an environment variable instead of updating appsettings.json:

**PowerShell:**
```powershell
$env:DATABASE_URL = "postgres://postgres:your_password@your_host:5432/railway"
dotnet run
```

## **✅ Success Checklist**

- [ ] PostgreSQL service created in Railway
- [ ] Connection string copied from Railway variables
- [ ] `appsettings.Development.json` updated
- [ ] Application runs with "Using PostgreSQL database" message
- [ ] Database migrations applied successfully
- [ ] Application accessible at `http://localhost:5000`

## **❌ Troubleshooting**

### **"SSL connection required" Error:**
✅ Make sure your connection string includes: `SSL Mode=Require;Trust Server Certificate=true`

### **"Authentication failed" Error:**
✅ Double-check your `PGPASSWORD` value from Railway

### **"Could not connect to server" Error:**
✅ Verify your `PGHOST` value from Railway

### **Application still using SQLite:**
✅ Make sure your connection string contains `Host=` (not `Data Source=`)

## **🎯 What Happens Next**

1. **Your app connects to Railway PostgreSQL** instead of local SQLite
2. **All TPD/Dean features work** with the cloud database
3. **Data persists** in Railway's managed PostgreSQL
4. **Ready for production deployment** with the same database
5. **Automatic backups** handled by Railway

**You're now running on cloud PostgreSQL!** 🚀
