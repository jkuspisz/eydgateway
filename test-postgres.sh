#!/bin/bash
# PostgreSQL Connection Test Script

echo "🐘 EYD Gateway PostgreSQL Connection Test"
echo "========================================="

# Check if connection string is provided
if [ -z "$1" ]; then
    echo "Usage: ./test-postgres.sh \"Your_Connection_String\""
    echo ""
    echo "Example:"
    echo "./test-postgres.sh \"Host=db.example.com;Port=5432;Database=eydgateway;Username=user;Password=pass;SSL Mode=Require\""
    exit 1
fi

CONNECTION_STRING="$1"

echo "📋 Testing connection with provided string..."
echo ""

# Set the connection string as environment variable for the test
export ConnectionStrings__DefaultConnection="$CONNECTION_STRING"

echo "🔧 Building application..."
dotnet build --verbosity quiet

if [ $? -eq 0 ]; then
    echo "✅ Build successful!"
    echo ""
    echo "🔗 Testing database connection..."
    echo "   (This will attempt to connect and run migrations)"
    echo ""
    
    # Run the application in test mode
    dotnet run --no-build --environment Development
else
    echo "❌ Build failed! Please check your code for errors."
    exit 1
fi
