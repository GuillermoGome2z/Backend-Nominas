# 💼 Sistema de Gestión de Nóminas - Backend

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server)
![Azure](https://img.shields.io/badge/Azure-0078D4?style=for-the-badge&logo=microsoft-azure)
![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=JSON%20web%20tokens)

Sistema completo de gestión de nóminas empresariales desarrollado con ASP.NET Core 8.0, diseñado para automatizar el cálculo, procesamiento y distribución de nóminas con seguridad empresarial y trazabilidad completa.

---

## 📋 Tabla de Contenidos

- [Características Principales](#-características-principales)
- [Tecnologías](#️-tecnologías)
- [Arquitectura](#-arquitectura)
- [Requisitos Previos](#-requisitos-previos)
- [Instalación](#-instalación)
- [Configuración](#️-configuración)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Endpoints Principales](#-endpoints-principales)
- [Base de Datos](#️-base-de-datos)
- [Seguridad](#-seguridad)
- [Reportes](#-reportes)
- [Testing](#-testing)
- [Deployment](#-deployment)
- [Contribución](#-contribución)
- [Licencia](#-licencia)

---

## ✨ Características Principales

### 🎯 Gestión de Empleados
- ✅ CRUD completo de empleados con validaciones
- ✅ Gestión de departamentos y puestos
- ✅ Expedientes digitales con Azure Blob Storage
- ✅ Información académica y laboral
- ✅ Activación/desactivación con validaciones de integridad

### 💰 Procesamiento de Nóminas
- ✅ Cálculo automático con bonificaciones y deducciones
- ✅ Soporte para nóminas ordinarias y extraordinarias
- ✅ Cálculo por departamento, empleado o empresa completa
- ✅ Estados: Borrador, Procesada, Aprobada, Pagada, Anulada
- ✅ Validación de duplicados por período
- ✅ Historial completo de cambios

### 📊 Reportes y Documentación
- ✅ 16+ tipos de reportes (PDF, Excel, CSV)
- ✅ Reportes de nóminas, empleados, deducciones, bonificaciones
- ✅ Reportes de expedientes con filtros avanzados
- ✅ Dashboard con métricas en tiempo real
- ✅ Estadísticas por departamento

### 🔐 Seguridad y Auditoría
- ✅ Autenticación JWT con refresh tokens
- ✅ Autorización basada en roles (Admin, RRHH, Usuario)
- ✅ Auditoría completa de todas las operaciones
- ✅ Logs detallados con información del usuario
- ✅ Middleware de manejo de errores centralizado

### 📁 Gestión Documental
- ✅ Almacenamiento en Azure Blob Storage
- ✅ URLs SAS para acceso seguro y temporal
- ✅ Validación de tipos de documento
- ✅ Historial de documentos por empleado
- ✅ Observaciones y validaciones de expedientes

---

## 🛠️ Tecnologías

### Backend
- **Framework:** ASP.NET Core 8.0
- **Lenguaje:** C# 12
- **ORM:** Entity Framework Core 8.0
- **Base de Datos:** SQL Server 2019+
- **Autenticación:** JWT Bearer Tokens
- **Documentación API:** Swagger/OpenAPI

### Librerías Principales
- **QuestPDF** - Generación de reportes PDF profesionales
- **ClosedXML** - Exportación a Excel
- **BCrypt.Net** - Hashing seguro de contraseñas
- **Azure.Storage.Blobs** - Almacenamiento en la nube

### Herramientas de Desarrollo
- **Visual Studio 2022** / **VS Code**
- **SQL Server Management Studio (SSMS)**
- **Postman** / **Swagger UI** para testing
- **Git** para control de versiones

---

## 🏗 Arquitectura

```
ProyectoNomina/
│
├── ProyectoNomina.Backend/          # API REST
│   ├── Controllers/                 # Endpoints HTTP
│   ├── Services/                    # Lógica de negocio
│   ├── Data/                        # DbContext y configuración EF
│   ├── Models/                      # Entidades del dominio
│   ├── Middleware/                  # Middleware personalizado
│   ├── Filters/                     # Filtros de acción
│   └── Migrations/                  # Migraciones de base de datos
│
└── ProyectoNomina.Shared/           # DTOs y modelos compartidos
    └── Models/DTOs/                 # Data Transfer Objects
```

### Patrón de Diseño
- **Repository Pattern** con Entity Framework Core
- **Dependency Injection** para servicios
- **DTO Pattern** para transferencia de datos
- **Middleware Pipeline** para manejo de errores y auditoría

---

## 📦 Requisitos Previos

### Software Requerido
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) o superior
- [SQL Server 2019+](https://www.microsoft.com/sql-server) o SQL Server Express
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Opcional
- [SQL Server Management Studio (SSMS)](https://docs.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms)
- [Azure Storage Account](https://azure.microsoft.com/services/storage/) (para almacenamiento de archivos)
- [Postman](https://www.postman.com/) para testing de API

---

## 🚀 Instalación

### 1. Clonar el Repositorio
```bash
git clone https://github.com/GuillermoGome2z/Backend-Nominas.git
cd Backend-Nominas
```

### 2. Restaurar Dependencias
```bash
dotnet restore
```

### 3. Configurar Base de Datos
Edita `appsettings.json` con tu cadena de conexión:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=ProyectoNomina2025;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 4. Aplicar Migraciones
```bash
cd ProyectoNomina.Backend
dotnet ef database update
```

### 5. Cargar Datos Iniciales (Opcional)
Ejecuta el script `SeedData.sql` en SQL Server para cargar datos de prueba.

### 6. Ejecutar el Proyecto
```bash
dotnet run
```

La API estará disponible en: `http://localhost:5009`

Swagger UI: `http://localhost:5009/swagger`

---

## ⚙️ Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=ProyectoNomina2025;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  
  "JwtSettings": {
    "SecretKey": "TU_CLAVE_SECRETA_MUY_SEGURA_DE_AL_MENOS_32_CARACTERES",
    "Issuer": "ProyectoNomina",
    "Audience": "ProyectoNominaUsuarios",
    "ExpirationHours": 1
  },
  
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "https://tu-dominio-produccion.com"
    ],
    "AllowCredentials": true
  },
  
  "AzureBlob": {
    "Enabled": true,
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net",
    "ContainerName": "nomina-docs",
    "DefaultSasMinutes": 15
  },
  
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5009"
      }
    },
    "Limits": {
      "MaxRequestBodySize": 20971520
    }
  }
}
```

### Variables de Entorno (Producción)
```bash
export ConnectionStrings__DefaultConnection="Server=..."
export JwtSettings__SecretKey="tu_clave_super_secreta"
export AzureBlob__ConnectionString="DefaultEndpointsProtocol=https..."
```

---

## 📁 Estructura del Proyecto

### Controllers (Endpoints)
- **AuthController** - Autenticación y gestión de tokens
- **EmpleadosController** - CRUD de empleados
- **DepartamentoController** - Gestión de departamentos
- **PuestoController** - Gestión de puestos de trabajo
- **NominasController** - Cálculo y procesamiento de nóminas
- **BonificacionesController** - Gestión de bonificaciones
- **DeduccionesController** - Gestión de deducciones
- **DocumentoEmpleadoController** - Gestión de documentos
- **ExpedientesController** - Expedientes de empleados
- **ReportesController** - Generación de reportes
- **DashboardController** - Métricas y estadísticas
- **AuditoriaController** - Consulta de logs

### Services (Lógica de Negocio)
- **NominaService** - Cálculo de nóminas con bonificaciones/deducciones
- **ReporteService** - Generación de reportes en múltiples formatos
- **ExpedientesReportService** - Reportes específicos de expedientes
- **FileStorageService** - Gestión de archivos en Azure Blob

### Models (Entidades Principales)
- **Empleado** - Información del empleado
- **Departamento** - Departamentos de la empresa
- **Puesto** - Puestos de trabajo
- **Nomina** - Nómina principal
- **DetalleNomina** - Detalle por empleado
- **Bonificacion / Deduccion** - Conceptos de nómina
- **DocumentoEmpleado** - Documentos digitales
- **Expediente** - Expedientes de empleados
- **Auditoria** - Logs de auditoría
- **Usuario** - Usuarios del sistema

---

## 🌐 Endpoints Principales

### 🔐 Autenticación
```http
POST   /api/Auth/login               # Login con email/password
POST   /api/Auth/refresh             # Renovar token con refresh token
POST   /api/Auth/logout              # Cerrar sesión
GET    /api/Auth/me                  # Información del usuario actual
```

### 👥 Empleados
```http
GET    /api/Empleados                # Listar empleados (paginado)
GET    /api/Empleados/{id}           # Obtener empleado por ID
POST   /api/Empleados                # Crear nuevo empleado
PUT    /api/Empleados/{id}           # Actualizar empleado
DELETE /api/Empleados/{id}           # Eliminar empleado
PUT    /api/Empleados/{id}/activar   # Activar empleado
PUT    /api/Empleados/{id}/desactivar # Desactivar empleado
```

### 💰 Nóminas
```http
GET    /api/Nominas                  # Listar nóminas con filtros
GET    /api/Nominas/{id}             # Obtener nómina por ID
POST   /api/Nominas/calcular         # Calcular preview de nómina
POST   /api/Nominas/generar          # Generar nómina nueva
POST   /api/Nominas/procesar/{id}    # Procesar nómina
POST   /api/Nominas/aprobar/{id}     # Aprobar nómina
POST   /api/Nominas/anular/{id}      # Anular nómina
POST   /api/Nominas/{id}/email       # Enviar nómina por email
```

### 📊 Reportes
```http
GET    /api/Reportes/nominas/pdf                    # Reporte de nóminas en PDF
GET    /api/Reportes/nominas/excel                  # Reporte de nóminas en Excel
GET    /api/Reportes/empleados/pdf                  # Reporte de empleados en PDF
GET    /api/Reportes/expedientes/pdf                # Reporte de expedientes con filtros
GET    /api/Reportes/expedientes/estadisticas       # Estadísticas de expedientes
```

### 📈 Dashboard
```http
GET    /api/Dashboard/estadisticas   # Métricas generales del sistema
```

### 📁 Documentos
```http
GET    /api/DocumentoEmpleado                       # Listar documentos
GET    /api/DocumentoEmpleado/empleado/{id}         # Documentos de un empleado
POST   /api/DocumentoEmpleado/upload                # Subir documento
DELETE /api/DocumentoEmpleado/{id}                  # Eliminar documento
```

---

## 🗄️ Base de Datos

### Diagrama ER Simplificado
```
Empleados ──┬─→ Departamentos
            ├─→ Puestos
            └─→ DocumentosEmpleado

Nominas ────→ DetalleNominas ──→ Empleados
            ├─→ Bonificaciones
            └─→ Deducciones

Usuarios ───→ RefreshTokens
            └─→ Auditoria
```

### Migraciones
```bash
# Crear nueva migración
dotnet ef migrations add NombreDeLaMigracion

# Aplicar migraciones
dotnet ef database update

# Revertir migración
dotnet ef database update MigracionAnterior

# Ver historial
dotnet ef migrations list
```

---

## 🔐 Seguridad

### Autenticación JWT
El sistema utiliza JWT (JSON Web Tokens) con refresh tokens para autenticación:

1. **Login** → Recibe Access Token (1 hora) + Refresh Token (7 días)
2. **Requests** → Incluir header: `Authorization: Bearer {access_token}`
3. **Renovación** → Usar `/api/Auth/refresh` con refresh token
4. **Logout** → Revoca el refresh token activo

### Roles y Permisos
- **Admin** - Acceso completo al sistema
- **RRHH** - Gestión de nóminas, empleados y reportes
- **Usuario** - Consulta de información limitada

### Headers Requeridos
```http
Authorization: Bearer {token}
Content-Type: application/json
```

### Ejemplo de Autenticación
```bash
# Login
curl -X POST http://localhost:5009/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "correo": "admin@empresa.com",
    "clave": "Admin123"
  }'

# Usar el token
curl -X GET http://localhost:5009/api/Empleados \
  -H "Authorization: Bearer {tu_token_aqui}"
```

---

## 📄 Reportes

### Tipos de Reportes Disponibles

#### 1. Reportes de Nóminas
- **PDF** - Formato profesional con detalles completos
- **Excel** - Editable, múltiples hojas con resúmenes
- **CSV** - Compatible con importación/exportación

#### 2. Reportes de Empleados
- Listado completo con departamentos
- Filtros por estado, departamento, puesto
- Exportación masiva

#### 3. Reportes de Expedientes
- Estado de documentación por empleado
- Documentos requeridos vs presentados
- Estadísticas de cumplimiento por departamento

#### 4. Reportes Financieros
- Deducciones por período
- Bonificaciones por empleado/departamento
- Análisis de nómina histórica

### Generación de Reportes
```csharp
// Ejemplo: Reporte de expedientes con filtros
GET /api/Reportes/expedientes/pdf?estado=Incompleto&departamentoId=1
```

---

## 🧪 Testing

### Ejecutar Tests
```bash
dotnet test
```

### Testing Manual con Swagger
1. Navegar a `http://localhost:5009/swagger`
2. Autenticarse usando `/api/Auth/login`
3. Copiar el token en el botón "Authorize"
4. Probar endpoints directamente

### Testing con Postman
Importar la colección desde `postman_collection.json` (si está disponible)

---

## 🚀 Deployment

### Publicación Local
```bash
dotnet publish -c Release -o ./publish
```

### Docker (Opcional)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5009

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProyectoNomina.Backend.dll"]
```

### Azure App Service
1. Crear App Service en Azure Portal
2. Configurar cadenas de conexión en Application Settings
3. Desplegar desde Visual Studio o Azure DevOps

### Variables de Entorno Producción
```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection="..."
JwtSettings__SecretKey="..."
AzureBlob__ConnectionString="..."
```

---

## 📝 Logs y Auditoría

### Sistema de Auditoría
Todas las operaciones quedan registradas con:
- ✅ Usuario que ejecutó la acción
- ✅ Endpoint accedido
- ✅ Método HTTP utilizado
- ✅ Detalles de la operación
- ✅ Timestamp preciso

### Consultar Logs
```http
GET /api/Auditoria?page=1&pageSize=50
GET /api/Auditoria/usuario/{userId}
GET /api/Auditoria?fechaInicio=2025-01-01&fechaFin=2025-12-31
```

---

## 🤝 Contribución

### Flujo de Trabajo
1. Fork el repositorio
2. Crear branch para feature: `git checkout -b feature/nueva-funcionalidad`
3. Commit cambios: `git commit -m 'feat: Agregar nueva funcionalidad'`
4. Push al branch: `git push origin feature/nueva-funcionalidad`
5. Crear Pull Request

### Convención de Commits
- `feat:` Nueva funcionalidad
- `fix:` Corrección de bug
- `docs:` Cambios en documentación
- `style:` Formato, punto y coma faltante, etc.
- `refactor:` Refactorización de código
- `test:` Agregar tests
- `chore:` Actualizar dependencias, tareas de mantenimiento

---

## 📞 Soporte

### Recursos
- **Documentación API:** `http://localhost:5009/swagger`
- **Issues:** [GitHub Issues](https://github.com/GuillermoGome2z/Backend-Nominas/issues)
- **Wiki:** [GitHub Wiki](https://github.com/GuillermoGome2z/Backend-Nominas/wiki)

### Contacto
- **Desarrollador:** Guillermo Gómez
- **GitHub:** [@GuillermoGome2z](https://github.com/GuillermoGome2z)

---

## 📜 Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para más detalles.

---

## 🎯 Roadmap

### Versión Actual (v1.0)
- ✅ CRUD completo de empleados
- ✅ Sistema de nóminas con cálculos automáticos
- ✅ Reportes en PDF/Excel/CSV
- ✅ Autenticación JWT
- ✅ Auditoría completa

### Próximas Versiones
- 🔜 API de notificaciones (email/SMS)
- 🔜 Integración con sistemas contables
- 🔜 Portal de autogestión para empleados
- 🔜 App móvil (React Native)
- 🔜 Análisis predictivo con ML
- 🔜 Integración con biométricos

---

## 🙏 Agradecimientos

Gracias a todos los que han contribuido a este proyecto y a las siguientes tecnologías que lo hacen posible:
- ASP.NET Core Team
- Entity Framework Core Team
- QuestPDF Community
- Azure Storage Team

---

## 📊 Estado del Proyecto

![GitHub last commit](https://img.shields.io/github/last-commit/GuillermoGome2z/Backend-Nominas)
![GitHub issues](https://img.shields.io/github/issues/GuillermoGome2z/Backend-Nominas)
![GitHub stars](https://img.shields.io/github/stars/GuillermoGome2z/Backend-Nominas)
![GitHub forks](https://img.shields.io/github/forks/GuillermoGome2z/Backend-Nominas)

---

<div align="center">
  <p>Desarrollado con ❤️ usando ASP.NET Core 8.0</p>
  <p>© 2025 Sistema de Gestión de Nóminas. Todos los derechos reservados.</p>
</div>
