# 🚀 Quick PostgreSQL Connection - Copy & Paste Guide

## **Method 1: Update Configuration File (Easiest)**

**Edit: `appsettings.Development.json`**

Replace the `DefaultConnection` with your PostgreSQL details:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_HOST;Port=5432;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

**Example for Supabase:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.abcdefg.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your_password;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

## **Method 2: Environment Variable (Best for Production)**

**Windows PowerShell:**
```powershell
$env:DATABASE_URL = "postgres://username:password@host:5432/database"
dotnet run
```

**Command Prompt:**
```cmd
set DATABASE_URL=postgres://username:password@host:5432/database
dotnet run
```

## **Method 3: Test Your Connection**

**Windows:**
```powershell
.\test-postgres.ps1 "Host=your-host;Port=5432;Database=your_db;Username=user;Password=pass;SSL Mode=Require"
```

## **🔥 Popular Providers - Ready-to-Use Templates**

### **Supabase:**
```
Host=db.YOUR_PROJECT_ID.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

### **Neon:**
```
Host=YOUR_HOST.neon.tech;Port=5432;Database=YOUR_DB;Username=YOUR_USER;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

### **Railway:**
```
Host=YOUR_HOST.railway.app;Port=5432;Database=railway;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

### **AWS RDS:**
```
Host=YOUR_INSTANCE.YOUR_REGION.rds.amazonaws.com;Port=5432;Database=eydgateway;Username=YOUR_USER;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

## **📋 Quick Steps:**

1. **Get your PostgreSQL connection details** from your provider
2. **Choose Method 1** (easiest) - update `appsettings.Development.json`
3. **Run:** `dotnet run`
4. **Look for:** "Using PostgreSQL database" in console
5. **Apply migrations:** `dotnet ef database update`
6. **Test:** Open browser to `http://localhost:5000`

## **✅ Success Indicators:**

- Console shows: "Using PostgreSQL database"
- No connection errors in console
- Application starts normally
- Database tables are created automatically

## **❌ Common Issues & Fixes:**

**"SSL connection required"** → Add `SSL Mode=Require;Trust Server Certificate=true`
**"Connection timeout"** → Check firewall and host address
**"Authentication failed"** → Verify username/password
**"Database does not exist"** → Create database first or check name

## **🎯 What Happens Next:**

- ✅ App connects to PostgreSQL instead of SQLite
- ✅ All your TPD/Dean features work with PostgreSQL
- ✅ Data persists in your cloud database
- ✅ Ready for production deployment
- ✅ All existing functionality preserved

**Just update the connection string and you're connected!** 🚀
