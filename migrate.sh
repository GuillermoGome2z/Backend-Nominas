#!/bin/bash
set -e

echo "=== Starting database migration ==="

# Verificar si las variables de entorno están configuradas
if [ -z "$DATABASE_URL" ]; then
    echo "ERROR: DATABASE_URL environment variable is not set"
    exit 1
fi

echo "Database URL configured"

# Navegar al directorio de la aplicación
cd /app/dist

# Aplicar migraciones
echo "Applying database migrations..."
dotnet ProyectoNomina.Backend.dll --migrate

echo "=== Database migration completed successfully ==="