#!/bin/bash
set -e

echo "Starting .NET 8 application..."
echo "Current directory: $(pwd)"
echo "Files in directory: $(ls -la)"

# Navigate to dist directory and start the application
cd dist
echo "Starting ProyectoNomina.Backend.dll..."
exec dotnet ProyectoNomina.Backend.dll --urls "http://0.0.0.0:${PORT:-8080}"