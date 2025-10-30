#!/bin/bash
set -e

echo "=== Starting .NET 8 application ==="

# Setup .NET environment
export PATH=$HOME/.dotnet:$PATH
export DOTNET_ROOT=$HOME/.dotnet

echo "Current directory: $(pwd)"
echo "Files in directory: $(ls -la)"

# Check .NET installation
if [ -f "$HOME/.dotnet/dotnet" ]; then
    echo ".NET version: $($HOME/.dotnet/dotnet --version)"
else
    echo "ERROR: .NET not found at $HOME/.dotnet/dotnet"
    exit 1
fi

# Navigate to dist directory and start the application
if [ -d "dist" ]; then
    cd dist
    echo "Starting ProyectoNomina.Backend.dll..."
    exec $HOME/.dotnet/dotnet ProyectoNomina.Backend.dll --urls "http://0.0.0.0:${PORT:-8080}"
else
    echo "ERROR: dist directory not found"
    exit 1
fi