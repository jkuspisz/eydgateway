# EYD Gateway - Docker & Railway Deployment Guide

## 🐳 Docker Setup

### Prerequisites
- Docker installed on your system
- Docker Compose (included with Docker Desktop)

### Local Development with Docker

#### Option 1: Using SQLite (Development)
```bash
# Build and run with SQLite
dotnet run
```

#### Option 2: Using PostgreSQL (Production-like)
```bash
# Start PostgreSQL and the application
docker-compose up -d

# View logs
docker-compose logs -f app

# Stop services
docker-compose down
```

### Building Docker Image
```bash
# Build the Docker image
docker build -t eydgateway .

# Run the container with SQLite
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development eydgateway

# Run with PostgreSQL (requires running PostgreSQL container)
docker run -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e DATABASE_URL=postgres://user:password@host:5432/database \
  eydgateway
```

## 🚂 Railway Deployment

### Prerequisites
- Railway account (https://railway.app)
- Railway CLI installed (optional)

### Deployment Steps

#### 1. Prepare Your Repository
Ensure these files are in your repository:
- `Dockerfile` ✅
- `.dockerignore` ✅
- `appsettings.Production.json` ✅

#### 2. Create Railway Project
1. Go to https://railway.app
2. Click "New Project"
3. Choose "Deploy from GitHub repo"
4. Select your repository

#### 3. Add PostgreSQL Database
1. In your Railway project dashboard
2. Click "New Service"
3. Choose "Database" → "PostgreSQL"
4. Railway will automatically create a PostgreSQL instance

#### 4. Configure Environment Variables
Railway will automatically set:
- `DATABASE_URL` (from PostgreSQL service)
- `PORT` (Railway's dynamic port)

Optional environment variables you can set:
- `ASPNETCORE_ENVIRONMENT=Production`

#### 5. Deploy
1. Railway will automatically detect your Dockerfile
2. Push to your main branch to trigger deployment
3. Railway will build and deploy your application

### Railway Configuration

#### Automatic Configuration
Railway automatically provides:
- `DATABASE_URL` - PostgreSQL connection string
- `PORT` - Dynamic port for your application
- SSL certificates and HTTPS

#### Custom Domain (Optional)
1. Go to your Railway project
2. Click on your web service
3. Go to "Settings" → "Domains"
4. Add your custom domain

### Database Migrations

Railway will run migrations automatically on deployment. The application includes automatic migration logic in `Program.cs`.

#### Manual Migration (if needed)
```bash
# Connect to Railway via CLI
railway login
railway link

# Run migrations
railway run dotnet ef database update
```

## 🔧 Troubleshooting

### Common Issues

#### 1. Database Connection Issues
- Check that PostgreSQL service is running in Railway
- Verify `DATABASE_URL` environment variable is set
- Check application logs in Railway dashboard

#### 2. Port Issues
- Railway automatically sets the `PORT` environment variable
- The application is configured to use this port automatically

#### 3. Build Issues
- Check Dockerfile syntax
- Ensure all required files are not in `.dockerignore`
- Check build logs in Railway dashboard

### Viewing Logs
```bash
# Railway CLI
railway logs

# Docker Compose
docker-compose logs -f app

# Docker container
docker logs <container-id>
```

## 📊 Database Management

### Development (SQLite)
- Database file: `local_eyd.db`
- Location: Project root directory
- Backup: Copy the `.db` file

### Production (PostgreSQL on Railway)
- Managed by Railway
- Automatic backups
- Access via Railway dashboard or CLI

### Migration Commands
```bash
# Add new migration
dotnet ef migrations add <MigrationName>

# Update database
dotnet ef database update

# Railway deployment (automatic)
railway run dotnet ef database update
```

## 🔐 Security Considerations

### Production Settings
- HTTPS is automatically enabled by Railway
- Environment variables are securely managed
- Database connections use SSL

### Recommended Environment Variables
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
```

## 📈 Monitoring

### Railway Dashboard
- Application metrics
- Database performance
- Error logs
- Resource usage

### Health Checks
The application includes basic health endpoints at:
- `/health` (if implemented)
- Application root `/` returns home page

---

## Support

For deployment issues:
1. Check Railway documentation: https://docs.railway.app
2. Review application logs in Railway dashboard
3. Check this README for common solutions

For application issues:
1. Review the main project documentation
2. Check controller and model implementations
3. Verify database schema and migrations
