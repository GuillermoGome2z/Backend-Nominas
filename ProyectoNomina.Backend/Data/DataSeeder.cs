using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Models;

namespace ProyectoNomina.Backend.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Asegura que la BD exista y esté migrada
            await context.Database.MigrateAsync();

            // 1) Roles base
            var rolesNecesarios = new[] { "Admin", "RRHH", "Empleado" };
            var rolesExistentes = await context.Roles.Select(r => r.Nombre).ToListAsync();
            var rolesFaltantes = rolesNecesarios
                .Except(rolesExistentes, StringComparer.OrdinalIgnoreCase)
                .Select(nombre => new Rol { Nombre = nombre })
                .ToList();

            if (rolesFaltantes.Any())
            {
                context.Roles.AddRange(rolesFaltantes);
                await context.SaveChangesAsync();
            }

            // Relee roles para obtener IDs
            var rolAdmin = await context.Roles.FirstAsync(r => r.Nombre == "Admin");

            // 2) Usuario admin (solo si no existe)
            var admin = await context.Usuarios.SingleOrDefaultAsync(u => u.Correo == "admin@empresa.com");
            if (admin == null)
            {
                admin = new Usuario
                {
                    NombreCompleto = "Administrador General",
                    Correo = "admin@empresa.com",

                    ClaveHash = "Admin123!",

                    Rol = "Admin",
                    EmpleadoId = null
                };
                context.Usuarios.Add(admin);
                await context.SaveChangesAsync();
            }

            // 3) Asegurar asignación del rol Admin al admin (tabla puente)
            var tieneAsignacionAdmin = await context.UsuarioRoles
                .AnyAsync(ur => ur.UsuarioId == admin.Id && ur.RolId == rolAdmin.Id);

            if (!tieneAsignacionAdmin)
            {
                context.UsuarioRoles.Add(new UsuarioRol
                {
                    UsuarioId = admin.Id,
                    RolId = rolAdmin.Id
                });
                await context.SaveChangesAsync();
            }

            // (Opcional) sembrar catálogos aquí (Departamentos, Puestos) si los necesitas.
        }
    }
}
