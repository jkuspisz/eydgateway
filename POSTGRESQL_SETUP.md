# 🐘 Connecting to Your Public PostgreSQL Database

## 📋 **Configuration Options**

You have **3 ways** to connect your EYD Gateway Platform to your public PostgreSQL database:

### **Option 1: Update appsettings.Development.json (Recommended for Development)**

Replace the `DefaultConnection` in `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_HOST;Port=5432;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

### **Option 2: Environment Variable (Recommended for Production)**

Set the `DATABASE_URL` environment variable:

**Windows (PowerShell):**
```powershell
$env:DATABASE_URL = "postgres://username:password@host:5432/database"
```

**Windows (Command Prompt):**
```cmd
set DATABASE_URL=postgres://username:password@host:5432/database
```

**Linux/Mac:**
```bash
export DATABASE_URL="postgres://username:password@host:5432/database"
```

### **Option 3: appsettings.Production.json (For Production Deployment)**

Update `appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_HOST;Port=5432;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

## 🔧 **Connection String Formats**

### **Standard PostgreSQL Connection String:**
```
Host=your-host.com;Port=5432;Database=eydgateway;Username=your_user;Password=your_password;SSL Mode=Require;Trust Server Certificate=true
```

### **PostgreSQL URL Format (Railway/Heroku style):**
```
postgres://username:password@host:5432/database
```

### **Common Public PostgreSQL Providers:**

#### **Supabase:**
```
Host=db.your-project.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your_password;SSL Mode=Require;Trust Server Certificate=true
```

#### **Neon:**
```
Host=your-host.neon.tech;Port=5432;Database=your_db;Username=your_user;Password=your_password;SSL Mode=Require;Trust Server Certificate=true
```

#### **AWS RDS:**
```
Host=your-instance.region.rds.amazonaws.com;Port=5432;Database=eydgateway;Username=your_user;Password=your_password;SSL Mode=Require;Trust Server Certificate=true
```

#### **Google Cloud SQL:**
```
Host=your-instance-ip;Port=5432;Database=eydgateway;Username=your_user;Password=your_password;SSL Mode=Require;Trust Server Certificate=true
```

## 🛠️ **Step-by-Step Setup**

### **Step 1: Get Your PostgreSQL Connection Details**

You'll need:
- **Host/Server**: Your PostgreSQL server address
- **Port**: Usually 5432
- **Database Name**: Your database name
- **Username**: Your PostgreSQL username  
- **Password**: Your PostgreSQL password

### **Step 2: Choose Your Configuration Method**

**For Development Testing:**
1. Update `appsettings.Development.json` with your connection string
2. Run `dotnet run` - it will automatically use PostgreSQL

**For Production/Railway:**
1. Set `DATABASE_URL` environment variable
2. Deploy to Railway - it will auto-detect PostgreSQL

### **Step 3: Run Database Migrations**

After connecting to PostgreSQL, run migrations:

```bash
# Create and apply migrations to PostgreSQL
dotnet ef database update
```

### **Step 4: Verify Connection**

Run the application:
```bash
dotnet run
```

Look for this console output:
```
Using PostgreSQL database
```

## 🔍 **Testing Your Connection**

### **Quick Connection Test:**

1. **Update appsettings.Development.json** with your PostgreSQL connection
2. **Run the application:**
   ```bash
   dotnet run
   ```
3. **Check the console** for "Using PostgreSQL database"
4. **Open your browser** and go to `http://localhost:5000`
5. **Check if the app starts** without database errors

### **Database Migration Test:**

```bash
# This will create tables in your PostgreSQL database
dotnet ef database update
```

## ⚠️ **Security Best Practices**

### **For Development:**
- Use a dedicated development database
- Don't use production credentials in appsettings files
- Consider using environment variables even in development

### **For Production:**
- Always use environment variables (`DATABASE_URL`)
- Never commit passwords to version control
- Use SSL/TLS connections (`SSL Mode=Require`)
- Consider using connection pooling for high traffic

## 🔧 **Troubleshooting**

### **Common Issues:**

#### **SSL Certificate Errors:**
Add to connection string:
```
SSL Mode=Require;Trust Server Certificate=true
```

#### **Firewall Issues:**
- Ensure your PostgreSQL server allows connections from your IP
- Check if port 5432 is open

#### **Authentication Errors:**
- Verify username and password
- Check if user has proper database permissions

#### **Connection Timeout:**
Add to connection string:
```
Timeout=30;Command Timeout=60
```

### **Testing Connection String:**

You can test your connection string with `psql`:
```bash
psql "Host=YOUR_HOST;Port=5432;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"
```

## 📝 **Example Configuration**

Here's a complete example for Supabase:

**appsettings.Development.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.abcdefghijk.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your_supabase_password;SSL Mode=Require;Trust Server Certificate=true"
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

## 🚀 **Ready to Deploy**

Once connected locally:

1. **Test thoroughly** with your PostgreSQL database
2. **Set up environment variables** for production
3. **Deploy to Railway** - it will use your PostgreSQL database
4. **Run migrations** in production: `dotnet ef database update`

Your EYD Gateway Platform will automatically switch between SQLite (local) and PostgreSQL (production) based on the connection string! 🎉
