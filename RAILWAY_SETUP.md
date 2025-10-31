# Railway Deployment Guide - Backend Nóminas

## Variables de Entorno Requeridas en Railway

Configura las siguientes variables de entorno en tu proyecto de Railway:

### Database
```
DATABASE_URL=postgresql://username:password@host:port/database
```
*(Railway proporciona automáticamente esta variable si conectas una base de datos PostgreSQL)*

### JWT Configuration
```
JWT_SECRET_KEY=tu_clave_jwt_super_secreta_de_minimo_64_caracteres_aqui
JWT_ISSUER=https://tu-backend.railway.app
JWT_AUDIENCE=https://tu-backend.railway.app
```

### CORS Configuration
```
FRONTEND_URL=https://tu-frontend.netlify.app,https://tu-frontend.vercel.app
```

### Application Environment
```
ASPNETCORE_ENVIRONMENT=Production
```

## Configuración de Base de Datos

### 1. Crear Servicio PostgreSQL en Railway
1. Ve a tu proyecto en Railway
2. Haz clic en "New Service" → "Database" → "PostgreSQL"
3. Railway automáticamente creará la variable `DATABASE_URL`

### 2. Migraciones Automáticas
El backend está configurado para aplicar migraciones automáticamente en producción durante el startup.

### 3. Verificar Conexión
Una vez desplegado, visita:
- `https://tu-backend.railway.app/health` - Para verificar que el servicio está funcionando
- `https://tu-backend.railway.app/swagger` - Para ver la documentación de la API

## Archivos de Configuración

### `nixpacks.toml`
Configura el build de .NET 8 en Railway con Nixpacks

### `railway.json`
Especifica el builder de Nixpacks y configuraciones de deployment

### `global.json`
Especifica la versión exacta del SDK de .NET 8

### `migrate.sh`
Script para ejecutar migraciones manualmente si es necesario

## Comando de Deploy Manual

Si necesitas redesplegar manualmente:
```bash
git add .
git commit -m "Deploy update"
git push origin main
```

## Troubleshooting

### Error de SDK de .NET
Si Railway usa una versión preview del SDK, verifica que `global.json` tenga:
```json
{
  "sdk": {
    "version": "8.0.403",
    "rollForward": "latestMinor",
    "allowPrerelease": false
  }
}
```

### Error de Migraciones
Verifica que la variable `DATABASE_URL` esté configurada correctamente y que la base de datos PostgreSQL esté funcionando.

### Error 500 al iniciar
Revisa los logs en Railway y verifica que todas las variables de entorno estén configuradas.