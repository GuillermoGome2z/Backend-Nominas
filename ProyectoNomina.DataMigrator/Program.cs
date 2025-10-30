using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Transactions;

namespace ProyectoNomina.DataMigrator;

public class Program
{
    private static readonly IConfiguration _config = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();

    public static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 ProyectoNomina DataMigrator - SQL Server → PostgreSQL");
        Console.WriteLine("========================================================");

        // Parse command line arguments
        var mssqlConnStr = GetConnectionString(args, "--mssql", "MSSQL_CONNSTR");
        var pgConnStr = GetConnectionString(args, "--pg", "POSTGRES_CONNSTR");

        if (string.IsNullOrEmpty(mssqlConnStr) || string.IsNullOrEmpty(pgConnStr))
        {
            ShowUsage();
            Environment.Exit(1);
        }

        try
        {
            Console.WriteLine("📋 Iniciando migración de datos...");
            
            // Test connections
            await TestConnections(mssqlConnStr, pgConnStr);
            
            // Migrate data
            await MigrateAllData(mssqlConnStr, pgConnStr);
            
            Console.WriteLine("✅ Migración completada exitosamente!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error durante la migración: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }

    private static string? GetConnectionString(string[] args, string argName, string envName)
    {
        // Check command line arguments first
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == argName)
                return args[i + 1];
        }
        
        // Fall back to environment variable
        return _config[envName];
    }

    private static void ShowUsage()
    {
        Console.WriteLine("Uso:");
        Console.WriteLine("dotnet run --project ./ProyectoNomina.DataMigrator \\");
        Console.WriteLine("  --mssql \"Server=localhost\\SQLEXPRESS;Database=ProyectoNomina2025;Trusted_Connection=True;TrustServerCertificate=True;\" \\");
        Console.WriteLine("  --pg \"Host=localhost;Port=5432;Database=ProyectoNomina2025_PG;Username=postgres;Password=postgres;SSL Mode=Prefer;\"");
        Console.WriteLine();
        Console.WriteLine("O usando variables de entorno:");
        Console.WriteLine("set MSSQL_CONNSTR=Server=localhost\\SQLEXPRESS;Database=...");
        Console.WriteLine("set POSTGRES_CONNSTR=Host=localhost;Port=5432;Database=...");
        Console.WriteLine("dotnet run --project ./ProyectoNomina.DataMigrator");
    }

    private static async Task TestConnections(string mssqlConnStr, string pgConnStr)
    {
        Console.WriteLine("🔌 Probando conexiones...");
        
        // Test SQL Server
        using (var sqlConn = new SqlConnection(mssqlConnStr))
        {
            await sqlConn.OpenAsync();
            var sqlVersion = await sqlConn.QuerySingleAsync<string>("SELECT @@VERSION");
            Console.WriteLine($"✅ SQL Server conectado: {sqlVersion.Split('\n')[0]}");
        }
        
        // Test PostgreSQL
        using (var pgConn = new NpgsqlConnection(pgConnStr))
        {
            await pgConn.OpenAsync();
            var pgVersion = await pgConn.QuerySingleAsync<string>("SELECT version()");
            Console.WriteLine($"✅ PostgreSQL conectado: {pgVersion.Split(',')[0]}");
        }
        
        Console.WriteLine();
    }

    private static async Task MigrateAllData(string mssqlConnStr, string pgConnStr)
    {
        using var sqlConn = new SqlConnection(mssqlConnStr);
        using var pgConn = new NpgsqlConnection(pgConnStr);
        
        await sqlConn.OpenAsync();
        await pgConn.OpenAsync();
        
        // Order matters for foreign key dependencies
        var migrationOrder = new[]
        {
            // Core catalogs first (no dependencies)
            ("Roles", "Roles"),
            ("Departamentos", "Departamentos"), 
            ("Puestos", "Puestos"),
            ("TiposDocumento", "TiposDocumento"),
            ("ReglasLaborales", "ReglasLaborales"),
            
            // User and employee data
            ("Usuarios", "Usuarios"),
            ("UsuarioRoles", "UsuarioRoles"),
            ("Empleados", "Empleados"),
            ("InformacionAcademica", "InformacionAcademica"),
            ("DocumentosEmpleado", "DocumentosEmpleado"),
            
            // Payroll catalogs and data
            ("Bonificaciones", "Bonificaciones"), 
            ("Deducciones", "Deducciones"),
            ("Nominas", "Nominas"),
            ("DetalleNominas", "DetalleNominas"),
            ("DetalleNominaHistorial", "DetalleNominaHistorial"),
            
            // Additional tables
            ("AjustesManuales", "AjustesManuales"),
            ("RefreshTokens", "RefreshTokens"),
            ("Auditoria", "Auditoria"),
            ("ObservacionesExpediente", "ObservacionesExpediente")
        };

        foreach (var (sqlTable, pgTable) in migrationOrder)
        {
            await MigrateTable(sqlConn, pgConn, sqlTable, pgTable);
        }
    }

    private static async Task MigrateTable(SqlConnection sqlConn, NpgsqlConnection pgConn, string sqlTable, string pgTable)
    {
        Console.WriteLine($"📊 Migrando tabla: {sqlTable} → {pgTable}");

        try
        {
            // Get row count from SQL Server
            var totalRows = await sqlConn.QuerySingleAsync<int>($"SELECT COUNT(*) FROM [{sqlTable}]");
            
            if (totalRows == 0)
            {
                Console.WriteLine($"   ⚠️  Tabla {sqlTable} está vacía, omitiendo...");
                return;
            }

            // Clear destination table
            await pgConn.ExecuteAsync($"TRUNCATE TABLE \"{pgTable}\" RESTART IDENTITY CASCADE");

            // Example migration for Roles table (you'll need to customize for each table)
            if (sqlTable == "Roles")
            {
                await MigrateRoles(sqlConn, pgConn);
            }
            else if (sqlTable == "Usuarios")
            {
                await MigrateUsuarios(sqlConn, pgConn);
            }
            else if (sqlTable == "Departamentos")
            {
                await MigrateDepartamentos(sqlConn, pgConn);
            }
            // TODO: Add more table-specific migrations here
            else
            {
                Console.WriteLine($"   ⚠️  Migración para tabla {sqlTable} no implementada aún");
                Console.WriteLine($"      👉 Agregar método Migrate{sqlTable}() en el código");
            }

            Console.WriteLine($"   ✅ {sqlTable}: migración completada");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Error migrando {sqlTable}: {ex.Message}");
            throw;
        }
    }

    // Example table migrations - customize based on your schema
    private static async Task MigrateRoles(SqlConnection sqlConn, NpgsqlConnection pgConn)
    {
        var roles = await sqlConn.QueryAsync(@"
            SELECT Id, Nombre, Descripcion, Activo, FechaCreacion, FechaModificacion 
            FROM Roles");

        const string insertSql = @"
            INSERT INTO ""Roles"" (""Id"", ""Nombre"", ""Descripcion"", ""Activo"", ""FechaCreacion"", ""FechaModificacion"")
            VALUES (@Id, @Nombre, @Descripcion, @Activo, @FechaCreacion, @FechaModificacion)";

        foreach (var role in roles)
        {
            await pgConn.ExecuteAsync(insertSql, new
            {
                Id = role.Id,
                Nombre = role.Nombre,
                Descripcion = role.Descripcion,
                Activo = role.Activo, // bit → boolean automatic conversion
                FechaCreacion = role.FechaCreacion,
                FechaModificacion = role.FechaModificacion
            });
        }

        Console.WriteLine($"      → {roles.Count()} roles migrados");
    }

    private static async Task MigrateUsuarios(SqlConnection sqlConn, NpgsqlConnection pgConn)
    {
        var usuarios = await sqlConn.QueryAsync(@"
            SELECT Id, Nombre, Email, PasswordHash, Activo, FechaCreacion, FechaModificacion
            FROM Usuarios");

        const string insertSql = @"
            INSERT INTO ""Usuarios"" (""Id"", ""Nombre"", ""Email"", ""PasswordHash"", ""Activo"", ""FechaCreacion"", ""FechaModificacion"")
            VALUES (@Id, @Nombre, @Email, @PasswordHash, @Activo, @FechaCreacion, @FechaModificacion)";

        foreach (var usuario in usuarios)
        {
            await pgConn.ExecuteAsync(insertSql, new
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                PasswordHash = usuario.PasswordHash,
                Activo = usuario.Activo,
                FechaCreacion = usuario.FechaCreacion,
                FechaModificacion = usuario.FechaModificacion
            });
        }

        Console.WriteLine($"      → {usuarios.Count()} usuarios migrados");
    }

    private static async Task MigrateDepartamentos(SqlConnection sqlConn, NpgsqlConnection pgConn)
    {
        var departamentos = await sqlConn.QueryAsync(@"
            SELECT Id, Nombre, Descripcion, Activo, FechaCreacion, FechaModificacion
            FROM Departamentos");

        const string insertSql = @"
            INSERT INTO ""Departamentos"" (""Id"", ""Nombre"", ""Descripcion"", ""Activo"", ""FechaCreacion"", ""FechaModificacion"")
            VALUES (@Id, @Nombre, @Descripcion, @Activo, @FechaCreacion, @FechaModificacion)";

        foreach (var depto in departamentos)
        {
            await pgConn.ExecuteAsync(insertSql, new
            {
                Id = depto.Id,
                Nombre = depto.Nombre,
                Descripcion = depto.Descripcion,
                Activo = depto.Activo,
                FechaCreacion = depto.FechaCreacion,
                FechaModificacion = depto.FechaModificacion
            });
        }

        Console.WriteLine($"      → {departamentos.Count()} departamentos migrados");
    }

    // TODO: Add more Migrate[TableName] methods for your specific tables:
    // - MigratePuestos()
    // - MigrateEmpleados() (handle DateTime, decimal, foreign keys)
    // - MigrateNominas() (handle complex types)
    // - MigrateDetalleNominas() 
    // - etc.
    
    // Key mapping considerations:
    // - SQL Server bit → PostgreSQL boolean
    // - SQL Server datetime/datetime2 → PostgreSQL timestamptz
    // - SQL Server money → PostgreSQL numeric(18,2)  
    // - SQL Server uniqueidentifier → PostgreSQL uuid
    // - SQL Server identity → PostgreSQL GENERATED BY DEFAULT AS IDENTITY
}
