# 📊 Proyecto Nómina - Sistema de Gestión de Empleados y Documentos

Este sistema web desarrollado con **Blazor WebAssembly + ASP.NET Core 8** permite la gestión integral de usuarios, empleados y documentos en una organización, incluyendo autenticación segura, control de roles, validación de expedientes y generación de reportes.

---

## ✅ Avances actuales

### 🔐 Autenticación y Autorización con JWT

- Implementado sistema de registro y login con **JWT**.
- Los usuarios se autentican con token y su sesión persiste.
- Los roles `Admin`, `RRHH` y `Usuario` se usan para proteger vistas y endpoints.

### 👥 Asociación entre Usuario y Empleado

- Cada usuario puede tener vinculado un `EmpleadoId` (relación 1:1).
- Los roles `Admin` y `RRHH` pueden asignar empleados a usuarios.
- Nuevo endpoint: `GET api/usuarios/empleado-actual` permite obtener el `EmpleadoId` desde el frontend.


### 🧠 Manejo de estado de usuario en el frontend

- `MainLayout.razor` muestra dinámicamente el nombre del usuario y permite cerrar sesión.
- Al iniciar sesión, el sistema consulta el `EmpleadoId` automáticamente para usarlo en formularios como subir documentos o visualizar nóminas.

---

## ⚙️ Tecnologías utilizadas

- ✅ Blazor WebAssembly (Frontend)
- ✅ ASP.NET Core 8 (Backend/API)
- ✅ Entity Framework Core (ORM)
- ✅ SQL Server (Base de datos)
- ✅ JWT (Autenticación)
- ✅ Bootstrap y TailwindCSS (Estilos y diseño)
- ✅ Git + GitHub (Control de versiones)


## 🧪 Cómo probar el proyecto

1. Clonar el repositorio y abrir la solución en **Visual Studio 2022**.
2. Configurar la cadena de conexión en `appsettings.json`.
3. Ejecutar migraciones con:

```bash
dotnet ef database update --project ProyectoNomina.Backend --startup-project ProyectoNomina.Backend
