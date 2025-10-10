# 📦 Backend - Sistema de Gestión de Nómina y Expedientes (API .NET 8)

Este repositorio contiene el **backend (API RESTful)** del sistema de gestión de nóminas y expedientes desarrollado en **ASP.NET Core 8 (C#)**.  
Proporciona los servicios para administrar empleados, usuarios, documentos y el procesamiento de nóminas, con autenticación segura mediante **JWT** y conexión a **SQL Server**.

---

## 🧱 Arquitectura General

El backend sigue una estructura modular basada en **Clean Architecture**, con separación de capas:

ProyectoNomina.Backend/
│
├── Controllers/ → Endpoints de la API (Empleados, Usuarios, Expedientes, Nómina)
├── Data/ → DbContext y configuración de acceso a base de datos
├── Models/ → Entidades principales del sistema
├── Services/ → Lógica de negocio (empleados, expedientes, nómina, reportes)
├── Filters/ → Filtros de validación y manejo global de errores
├── DTOs/ → Objetos de transferencia de datos
└── Program.cs → Configuración general (JWT, Swagger, CORS, EF Core)


---

## ⚙️ Tecnologías utilizadas

| Componente | Tecnología |
|-------------|-------------|
| Lenguaje | C# |
| Framework | .NET 8 (ASP.NET Core Web API) |
| ORM | Entity Framework Core |
| Base de datos | SQL Server |
| Autenticación | JWT (JSON Web Token) |
| Documentación | Swagger / OpenAPI |
| Reportes PDF | QuestPDF |
| Control de versiones | Git + GitHub |

---

## 🔐 Autenticación y Roles

- Autenticación basada en **JWT Bearer Tokens**.  
- Roles principales:
  - `Administrador`
  - `RRHH`
  - `Empleado`

El backend valida los tokens en cada solicitud protegida.  
Endpoints restringidos usan atributos como:
```csharp
[Authorize(Roles = "Administrador, RRHH")]

```
## 👥 Módulos principales
1. Gestión de Empleados

CRUD completo de empleados.

Campos: datos personales, laborales y académicos.

Validación de documentos requeridos.

2. Expedientes

Subida y almacenamiento de documentos (DPI, certificados, contratos, etc.).

Validación automática del expediente completo o incompleto.

Control de tipos de documentos y relación con empleados.

3. Nóminas

Generación de nómina mensual o quincenal.

Cálculo de salario, bonificaciones, descuentos e IGSS.

Registro de ajustes y auditoría de cambios.

4. Reportes

Generación de reportes PDF y Excel:

Nómina por periodo

Empleados activos/inactivos

Estado de expedientes

Información académica

Implementados con QuestPDF.

5. Seguridad y Logs

Cifrado de contraseñas.

Manejo centralizado de excepciones y errores HTTP.

Auditoría de acciones críticas (modificaciones, cargas, cálculos).

## 🧪 Configuración y ejecución
# 🔧 Requisitos previos

Visual Studio 2022 o VS Code con .NET 8 SDK

SQL Server local o remoto

Git instalado

## ⚙️ Configuración de base de datos

Editar el archivo appsettings.json:
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=NominaDB;Trusted_Connection=True;TrustServerCertificate=True;"
}

## Ejecución local

## Restaurar dependencias
dotnet restore

## Aplicar migraciones
dotnet ef database update --project ProyectoNomina.Backend --startup-project ProyectoNomina.Backend

## Ejecutar la API
dotnet run --project ProyectoNomina.Backend


## Probar en Swagger
https://localhost:5001/swagger

## 🔒 Variables importantes
Variable	Descripción
JwtSettings:SecretKey	Clave secreta para firmar los JWT
JwtSettings:Issuer	Emisor del token
JwtSettings:Audience	Audiencia del token
ConnectionStrings:DefaultConnection	Cadena de conexión SQL Server

## 📚 Endpoints principales
Endpoint	Método	Descripción
/api/auth/login	POST	Inicia sesión y devuelve token JWT
/api/empleados	GET/POST/PUT/DELETE	CRUD de empleados
/api/expedientes	GET/POST	Administración de documentos
/api/nominas	GET/POST	Procesamiento de nóminas
/api/reportes	GET	Exportación PDF/Excel

## 🧩 Estructura de respuestas y manejo de errores

Ejemplo de respuesta exitosa:
{
  "success": true,
  "message": "Empleado creado correctamente",
  "data": { "id": 12, "nombre": "Juan Pérez" }
}

Errores controlados:
{
  "error": "ValidationError",
  "details": {
    "dpi": "El número de DPI es inválido"
  }
}

## 🧰 Seguridad y buenas prácticas

Encriptación de contraseñas con BCrypt.

CORS configurado para el frontend (React/Vite o Blazor).

Políticas de roles y autenticación en Program.cs.

Logs detallados en caso de error (ILogger).

Middleware global para formateo uniforme de errores.

## 🧾 Documentación y despliegue

El proyecto incluye:

Swagger UI (/swagger) para pruebas interactivas.

Dockerfile opcional para despliegue contenedorizado.

Manual técnico (instalación, configuración y mantenimiento).

## Para producción:
dotnet publish -c Release

## 📎 Repositorio del proyecto

Frontend (nuevo) → por definir (React/Vite)

Backend (actual) → Repositorio Backend-Nominas

