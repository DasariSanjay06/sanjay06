#!/bin/bash

# Build and Run Script for Shimnit Laser Marking System

echo "=========================================="
echo "Shimnit Laser Marking System"
echo "Build and Run Script"
echo "=========================================="

# Set .NET path
export PATH=$PATH:/home/ubuntu/.dotnet

# Clean previous builds
echo "Cleaning previous builds..."
dotnet clean

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore

# Build the application
echo "Building the application..."
dotnet build --configuration Release

# Check if build was successful
if [ $? -eq 0 ]; then
    echo "Build successful!"
    echo ""
    echo "Application is ready to run."
    echo "To run the application, use:"
    echo "dotnet run --project LaserMarkingSystem"
    echo ""
    echo "Or to run the built executable:"
    echo "dotnet LaserMarkingSystem/bin/Release/net6.0-windows/LaserMarkingSystem.dll"
else
    echo "Build failed!"
    exit 1
fi

echo "=========================================="
echo "Build completed successfully!"
echo "=========================================="