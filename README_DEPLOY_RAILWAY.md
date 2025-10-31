# 🚀 Guía de Deployment - Railway con PostgreSQL

Esta guía te ayudará a migrar tu API .NET 8 de SQL Server a PostgreSQL y desplegarla en Railway usando Nixpacks.

## 📋 Resumen de Cambios Realizados

### ✅ 1. Paquetes NuGet Actualizados
- ❌ Eliminado: `Microsoft.EntityFrameworkCore.SqlServer` 
- ✅ Agregado: `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.10
- ✅ Downgraded: Entity Framework Core de 9.0.10 → 8.0.10 (compatibilidad)

### ✅ 2. Program.cs - Railway Compatible
```csharp
// ANTES
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// DESPUÉS  
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
```

**Nuevas características:**
- ✅ Auto-migración en producción: `await db.Database.MigrateAsync()`
- ✅ CORS estricto por variable: `Cors:AllowedOrigins`
- ✅ Healthcheck: `GET /health` → `"OK"`
- ✅ Sin puerto fijo (Railway maneja `PORT`)
- ✅ JWT configuration compatible con Railway variables

### ✅ 3. DbContext - PostgreSQL Compatible  
- ✅ `GETUTCDATE()` → `now()` en defaults SQL
- ✅ Tipos automáticamente mapeados (bit→boolean, datetime→timestamptz)

### ✅ 4. Migraciones PostgreSQL
- ✅ Eliminadas migraciones SQL Server legacy
- ✅ Nueva migración inicial: `InicialPostgres`
- ✅ Compatible con esquema completo de nóminas Guatemala 2025

### ✅ 5. DataMigrator - Herramienta de Migración de Datos
- ✅ Proyecto de consola independiente con Dapper + Npgsql
- ✅ CLI amigable con support para argumentos y variables de entorno
- ✅ Mapeo automático de tipos SQL Server → PostgreSQL

## 🛠️ Pasos de Deployment

### Paso 1: Configurar Base de Datos PostgreSQL en Railway

1. **Crear cuenta en Railway:** https://railway.app
2. **Nuevo proyecto:** "New Project" → "Provision PostgreSQL"
3. **Copiar URL de conexión:**
   ```
   Variables → DATABASE_URL
   postgres://user:password@host:port/database
   ```

### Paso 2: Convertir URL PostgreSQL a Connection String

**URL Railway:**
```
postgres://postgres:password123@monorail.proxy.rlwy.net:12345/railway
```

**Connection String para .NET:**
```
Host=monorail.proxy.rlwy.net;Port=12345;Database=railway;Username=postgres;Password=password123;SSL Mode=Require;Trust Server Certificate=true
```

### Paso 3: Configurar Variables de Entorno en Railway

En tu servicio backend de Railway, agrega estas variables:

```bash
# OBLIGATORIAS
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=TU_HOST;Port=TU_PORT;Database=TU_DB;Username=TU_USER;Password=TU_PASSWORD;SSL Mode=Require;Trust Server Certificate=true

# CORS - Reemplaza con tu dominio frontend  
Cors__AllowedOrigins=https://tu-frontend.netlify.app,https://tu-frontend.vercel.app

# JWT - Genera una clave segura de 64+ caracteres
Jwt__Issuer=https://tu-backend-railway.up.railway.app
Jwt__Audience=https://tu-backend-railway.up.railway.app  
Jwt__Key=TuClaveJWTMuySeguraConMasDe64CaracteresParaProduccion123456789

# OPCIONAL - Logging
Logging__LogLevel__Default=Information
Logging__LogLevel__Microsoft__AspNetCore=Warning
```

### Paso 4: Deploy desde GitHub

1. **Conectar repositorio:**
   - Railway → "New Project" → "Deploy from GitHub repo"
   - Selecciona tu repositorio con el código migrado

2. **Railway detectará automáticamente:**
   - ✅ `nixpacks.toml` → Build configuration  
   - ✅ `.NET 8 project` → Runtime

3. **Verificar build:**
   ```bash
   # Build Command (automático)
   dotnet restore ./ProyectoNomina.Backend
   dotnet publish ./ProyectoNomina.Backend -c Release -o /app/out
   
   # Start Command (automático)
   ASPNETCORE_URLS=http://0.0.0.0:${PORT} dotnet /app/out/ProyectoNomina.Backend.dll
   ```

### Paso 5: Verificar Deployment

1. **Health Check:**
   ```bash
   curl https://tu-backend-railway.up.railway.app/health
   # Esperado: "OK"
   ```

2. **Swagger (opcional):**
   ```
   https://tu-backend-railway.up.railway.app/swagger
   ```

3. **Logs de deployment:**
   - Railway Dashboard → Tu servicio → "View Logs"
   - Buscar: `✅ Migration successful` o errores

## 🔄 Migración de Datos (SQL Server → PostgreSQL)

### Opción 1: Usando DataMigrator (Recomendado)

```bash
# Restaurar paquetes
dotnet restore ./ProyectoNomina.DataMigrator

# Ejecutar migración
dotnet run --project ./ProyectoNomina.DataMigrator \
  --mssql "Server=localhost\SQLEXPRESS;Database=ProyectoNomina2025;Trusted_Connection=True;TrustServerCertificate=True;" \
  --pg "Host=TU_HOST;Port=TU_PORT;Database=TU_DB;Username=TU_USER;Password=TU_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
```

### Opción 2: Variables de Entorno

```bash
# Windows
set MSSQL_CONNSTR=Server=localhost\SQLEXPRESS;Database=ProyectoNomina2025;Trusted_Connection=True;TrustServerCertificate=True;
set POSTGRES_CONNSTR=Host=TU_HOST;Port=TU_PORT;Database=TU_DB;Username=TU_USER;Password=TU_PASSWORD;SSL Mode=Require;Trust Server Certificate=true

dotnet run --project ./ProyectoNomina.DataMigrator
```

### Completar DataMigrator (TODO)

El DataMigrator incluye ejemplos para `Roles`, `Usuarios`, `Departamentos`. **Debes agregar métodos para:**

- `MigratePuestos()`
- `MigrateEmpleados()` ⚠️ (fechas, decimales, FKs)
- `MigrateNominas()` ⚠️ (tipos complejos)
- `MigrateDetalleNominas()`
- `MigrateBonificaciones()` y `MigrateDeducciones()`
- etc.

**Mapeo de tipos importantes:**
- `bit` → `boolean` ✅ (automático)
- `datetime/datetime2` → `timestamptz` ✅ 
- `money` → `numeric(18,2)` ✅
- `uniqueidentifier` → `uuid` ⚠️ (verificar manual)

## 🔧 Configuración Local para Testing

### appsettings.Development.json (crear si no existe)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ProyectoNomina2025_PG;Username=postgres;Password=postgres;SSL Mode=Prefer;"
  },
  "Cors": {
    "AllowedOrigins": "http://localhost:5173,http://localhost:3000"
  },
  "Jwt": {
    "Issuer": "https://localhost:5009",
    "Audience": "https://localhost:5009",
    "Key": "SuperSecretKeyForDevelopmentOnlyWithMoreThan32Characters123456789"
  }
}
```

### Comandos de testing local:
```bash
# Aplicar migraciones localmente
dotnet ef database update --project ./ProyectoNomina.Backend

# Ejecutar localmente
dotnet run --project ./ProyectoNomina.Backend

# Test health
curl http://localhost:5009/health
```

## ⚠️ Troubleshooting Común

### Error: "Jwt:Issuer no configurado"
**Solución:** Verifica variables Railway con doble underscore:
```
Jwt__Issuer=https://tu-backend.railway.app
Jwt__Audience=https://tu-backend.railway.app  
Jwt__Key=TuClaveSegura64+Caracteres
```

### Error: "No connection to PostgreSQL"
**Solución:** 
1. Verifica `ConnectionStrings__DefaultConnection` en Railway
2. Usa `SSL Mode=Require` y `Trust Server Certificate=true`

### Error CORS en frontend
**Solución:**
1. Agrega tu dominio exacto en `Cors__AllowedOrigins`
2. Separa múltiples orígenes con comas: `https://app1.com,https://app2.com`

### Build falla en Railway
**Solución:**
1. Verifica que `nixpacks.toml` esté en la raíz
2. Verifica que el .NET 8 SDK esté especificado
3. Revisa logs de build en Railway Dashboard

## 🎯 Checklist Final

- [ ] ✅ PostgreSQL database creada en Railway
- [ ] ✅ Variables de entorno configuradas (JWT, CORS, ConnectionString)
- [ ] ✅ Repositorio conectado a Railway con deployment automático
- [ ] ✅ Build exitoso (verificar logs)
- [ ] ✅ Health check responde: `GET /health` → `"OK"`
- [ ] ✅ Auto-migración ejecutada en producción (verificar logs)
- [ ] ✅ CORS funciona con tu frontend
- [ ] ✅ Datos migrados desde SQL Server (si aplica)
- [ ] ✅ Swagger accesible (opcional): `/swagger`

## 🔗 Links Útiles

- **Railway Dashboard:** https://railway.app/dashboard
- **Railway Docs - .NET:** https://docs.railway.app/guides/dotnet
- **Npgsql EF Core:** https://www.npgsql.org/efcore/
- **Railway Variables:** https://docs.railway.app/develop/variables

---

🎉 **¡Tu API está lista para producción en Railway con PostgreSQL!**

Para cualquier problema, revisa los logs de Railway o contacta al equipo de desarrollo.