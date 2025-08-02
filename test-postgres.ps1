# PowerShell script to test PostgreSQL connection
param(
    [Parameter(Mandatory=$true)]
    [string]$ConnectionString
)

Write-Host "🐘 EYD Gateway PostgreSQL Connection Test" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "📋 Testing connection with provided string..." -ForegroundColor Yellow
Write-Host "Connection: $ConnectionString" -ForegroundColor Gray
Write-Host ""

# Set the connection string as environment variable for the test
$env:ConnectionStrings__DefaultConnection = $ConnectionString

Write-Host "🔧 Building application..." -ForegroundColor Yellow
$buildResult = dotnet build --verbosity quiet
$buildExitCode = $LASTEXITCODE

if ($buildExitCode -eq 0) {
    Write-Host "✅ Build successful!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🔗 Testing database connection..." -ForegroundColor Yellow
    Write-Host "   (This will attempt to connect and run migrations)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   Press Ctrl+C to stop once you see the application starting..." -ForegroundColor Gray
    Write-Host ""
    
    # Run the application in test mode
    dotnet run --no-build --environment Development
} else {
    Write-Host "❌ Build failed! Please check your code for errors." -ForegroundColor Red
    exit 1
}

# Usage example:
# .\test-postgres.ps1 "Host=your-host.com;Port=5432;Database=eydgateway;Username=user;Password=pass;SSL Mode=Require"
