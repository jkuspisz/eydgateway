# Railway Production Deployment Fix

## Issue
The application was failing in production with:
```
No database connection string found for production environment.
```

## Solution
We've updated the configuration to work in two ways:

### Option 1: Using appsettings.Production.json (Recommended)
The `appsettings.Production.json` now contains the Railway PostgreSQL connection string directly.

### Option 2: Using Environment Variables
Set this environment variable in your Railway project:

```
DATABASE_URL=postgresql://postgres:CIfjNPUQiVHKAzFUWxHJAFcJtviQbbKa@interchange.proxy.rlwy.net:54613/railway
```

## How to Set Environment Variables in Railway

1. Go to your Railway project dashboard
2. Click on your service
3. Go to the "Variables" tab
4. Add the environment variable:
   - **Key**: `DATABASE_URL`
   - **Value**: `postgresql://postgres:CIfjNPUQiVHKAzFUWxHJAFcJtviQbbKa@interchange.proxy.rlwy.net:54613/railway`

## Deployment Commands

```bash
# Deploy to Railway
railway up

# Or using Docker
docker build -t eydgateway .
railway deploy
```

## Verification

After deployment, your application should:
1. Connect to PostgreSQL automatically
2. Run database migrations
3. Start successfully

The app will use the Railway PostgreSQL database in production and the local connection for development.
