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

            // 2) Usuario admin (idempotente)
            var adminCorreo = "admin@empresa.com";
            var admin = await context.Usuarios.SingleOrDefaultAsync(u => u.Correo == adminCorreo);

            // Si no hay admin, lo creamos con contraseña hasheada
            if (admin == null)
            {
                var defaultPwd = Environment.GetEnvironmentVariable("ADMIN_DEFAULT_PASSWORD");
                if (string.IsNullOrWhiteSpace(defaultPwd))
                {
                    // Fallback seguro para desarrollo. En prod, setear ADMIN_DEFAULT_PASSWORD
                    defaultPwd = "Admin123!";
                }

                var hash = BCrypt.Net.BCrypt.HashPassword(defaultPwd);

                admin = new Usuario
                {
                    NombreCompleto = "Administrador General",
                    Correo = adminCorreo,
                    ClaveHash = hash,          // ✅ Hash (no texto plano)
                    Rol = "Admin",             // Mantiene compat. con ClaimTypes.Role
                    EmpleadoId = null
                };

                context.Usuarios.Add(admin);
                await context.SaveChangesAsync();
            }
            else
            {
                // Si existe y su ClaveHash no parece un hash BCrypt, lo migramos a hash
                if (!EsHashBcrypt(admin.ClaveHash))
                {
                    var defaultPwd = Environment.GetEnvironmentVariable("ADMIN_DEFAULT_PASSWORD");
                    if (string.IsNullOrWhiteSpace(defaultPwd))
                    {
                        // No conocemos la contraseña anterior: forzamos una nueva por variable de entorno o fallback dev.
                        defaultPwd = "Admin123!";
                    }

                    admin.ClaveHash = BCrypt.Net.BCrypt.HashPassword(defaultPwd);
                    await context.SaveChangesAsync();
                }

                // Garantiza que su rol string sea "Admin" para la autorización por atributos
                if (!string.Equals(admin.Rol, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    admin.Rol = "Admin";
                    await context.SaveChangesAsync();
                }
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

        // BCrypt genera hashes que inician con $2a$, $2b$ o $2y$
        private static bool EsHashBcrypt(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return false;
            return valor.StartsWith("$2a$") || valor.StartsWith("$2b$") || valor.StartsWith("$2y$");
        }
    }
}
