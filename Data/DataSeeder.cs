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

            // 4) Sembrar Departamentos de ejemplo
            if (!await context.Departamentos.AnyAsync())
            {
                var departamentos = new[]
                {
                    new Departamento { Nombre = "Recursos Humanos", Activo = true },
                    new Departamento { Nombre = "Tecnología", Activo = true },
                    new Departamento { Nombre = "Ventas", Activo = true },
                    new Departamento { Nombre = "Contabilidad", Activo = true },
                    new Departamento { Nombre = "Operaciones", Activo = true }
                };

                context.Departamentos.AddRange(departamentos);
                await context.SaveChangesAsync();
            }

            // 5) Sembrar Puestos de ejemplo
            if (!await context.Puestos.AnyAsync())
            {
                var deptoTI = await context.Departamentos.FirstOrDefaultAsync(d => d.Nombre == "Tecnología");
                var deptoRRHH = await context.Departamentos.FirstOrDefaultAsync(d => d.Nombre == "Recursos Humanos");
                var deptoVentas = await context.Departamentos.FirstOrDefaultAsync(d => d.Nombre == "Ventas");
                var deptoContabilidad = await context.Departamentos.FirstOrDefaultAsync(d => d.Nombre == "Contabilidad");

                var puestos = new List<Puesto>();

                if (deptoTI != null)
                {
                    puestos.AddRange(new[]
                    {
                        new Puesto { Nombre = "Desarrollador Senior", SalarioBase = 12000m, Activo = true, DepartamentoId = deptoTI.Id },
                        new Puesto { Nombre = "Desarrollador Junior", SalarioBase = 7000m, Activo = true, DepartamentoId = deptoTI.Id },
                        new Puesto { Nombre = "Analista de Sistemas", SalarioBase = 9000m, Activo = true, DepartamentoId = deptoTI.Id }
                    });
                }

                if (deptoRRHH != null)
                {
                    puestos.AddRange(new[]
                    {
                        new Puesto { Nombre = "Gerente de RRHH", SalarioBase = 10000m, Activo = true, DepartamentoId = deptoRRHH.Id },
                        new Puesto { Nombre = "Reclutador", SalarioBase = 6000m, Activo = true, DepartamentoId = deptoRRHH.Id }
                    });
                }

                if (deptoVentas != null)
                {
                    puestos.AddRange(new[]
                    {
                        new Puesto { Nombre = "Ejecutivo de Ventas", SalarioBase = 8000m, Activo = true, DepartamentoId = deptoVentas.Id },
                        new Puesto { Nombre = "Gerente de Ventas", SalarioBase = 11000m, Activo = true, DepartamentoId = deptoVentas.Id }
                    });
                }

                if (deptoContabilidad != null)
                {
                    puestos.Add(new Puesto { Nombre = "Contador", SalarioBase = 8500m, Activo = true, DepartamentoId = deptoContabilidad.Id });
                }

                if (puestos.Any())
                {
                    context.Puestos.AddRange(puestos);
                    await context.SaveChangesAsync();
                }
            }

            // 6) Sembrar Empleados de ejemplo
            if (!await context.Empleados.AnyAsync())
            {
                var deptoTI = await context.Departamentos.FirstOrDefaultAsync(d => d.Nombre == "Tecnología");
                var deptoRRHH = await context.Departamentos.FirstOrDefaultAsync(d => d.Nombre == "Recursos Humanos");
                var puestoDevSenior = await context.Puestos.FirstOrDefaultAsync(p => p.Nombre == "Desarrollador Senior");
                var puestoDevJunior = await context.Puestos.FirstOrDefaultAsync(p => p.Nombre == "Desarrollador Junior");
                var puestoGerenteRRHH = await context.Puestos.FirstOrDefaultAsync(p => p.Nombre == "Gerente de RRHH");

                var empleados = new List<Empleado>();

                if (deptoTI != null && puestoDevSenior != null)
                {
                    empleados.Add(new Empleado
                    {
                        NombreCompleto = "Juan Carlos Pérez",
                        DPI = "1234567890101",
                        NIT = "12345678",
                        Correo = "juan.perez@empresa.com",
                        Telefono = "12345678",
                        Direccion = "Ciudad de Guatemala, Zona 10",
                        FechaNacimiento = new DateTime(1990, 5, 15),
                        FechaContratacion = new DateTime(2020, 1, 10),
                        EstadoLaboral = "ACTIVO",
                        SalarioMensual = 12000m,
                        DepartamentoId = deptoTI.Id,
                        PuestoId = puestoDevSenior.Id
                    });
                }

                if (deptoTI != null && puestoDevJunior != null)
                {
                    empleados.Add(new Empleado
                    {
                        NombreCompleto = "María Fernanda López",
                        DPI = "9876543210101",
                        NIT = "87654321",
                        Correo = "maria.lopez@empresa.com",
                        Telefono = "87654321",
                        Direccion = "Antigua Guatemala",
                        FechaNacimiento = new DateTime(1995, 8, 20),
                        FechaContratacion = new DateTime(2022, 3, 15),
                        EstadoLaboral = "ACTIVO",
                        SalarioMensual = 7000m,
                        DepartamentoId = deptoTI.Id,
                        PuestoId = puestoDevJunior.Id
                    });
                }

                if (deptoRRHH != null && puestoGerenteRRHH != null)
                {
                    empleados.Add(new Empleado
                    {
                        NombreCompleto = "Carlos Eduardo Ramírez",
                        DPI = "1122334455101",
                        NIT = "11223344",
                        Correo = "carlos.ramirez@empresa.com",
                        Telefono = "22334455",
                        Direccion = "Quetzaltenango",
                        FechaNacimiento = new DateTime(1985, 3, 10),
                        FechaContratacion = new DateTime(2018, 6, 1),
                        EstadoLaboral = "ACTIVO",
                        SalarioMensual = 10000m,
                        DepartamentoId = deptoRRHH.Id,
                        PuestoId = puestoGerenteRRHH.Id
                    });
                }

                if (empleados.Any())
                {
                    context.Empleados.AddRange(empleados);
                    await context.SaveChangesAsync();
                }
            }

            // 7) Sembrar Tipos de Documento
            if (!await context.TiposDocumento.AnyAsync())
            {
                var tiposDocumento = new[]
                {
                    new TipoDocumento { Nombre = "DPI", Descripcion = "Documento Personal de Identificación", EsRequerido = true, Orden = 1 },
                    new TipoDocumento { Nombre = "CV", Descripcion = "Currículum Vitae", EsRequerido = true, Orden = 2 },
                    new TipoDocumento { Nombre = "Título", Descripcion = "Título académico", EsRequerido = false, Orden = 3 },
                    new TipoDocumento { Nombre = "Constancia", Descripcion = "Constancia de trabajo", EsRequerido = false, Orden = 4 },
                    new TipoDocumento { Nombre = "Antecedentes", Descripcion = "Antecedentes penales y policiacos", EsRequerido = true, Orden = 5 }
                };

                context.TiposDocumento.AddRange(tiposDocumento);
                await context.SaveChangesAsync();
            }
        }

        // BCrypt genera hashes que inician con $2a$, $2b$ o $2y$
        private static bool EsHashBcrypt(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return false;
            return valor.StartsWith("$2a$") || valor.StartsWith("$2b$") || valor.StartsWith("$2y$");
        }
    }
}
