using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Models;
using System.Text.Json;

namespace ProyectoNomina.Backend.Data;

public class ReglasLaboralesSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Verificar si ya existen reglas activas para Guatemala 2025
        var existeGuatemala2025 = await context.ReglasLaborales
            .AnyAsync(r => r.Pais == "GT" && r.Activo && r.VigenteDesde.Year == 2025);

        if (existeGuatemala2025)
        {
            Console.WriteLine("ReglasLaborales para Guatemala 2025 ya existen. Saltando seed.");
            return;
        }

        // Tabla progresiva ISR 2025 Guatemala (Decreto 10-2012)
        // Renta imponible anual convertida a mensual
        var isrEscala = new[]
        {
            new
            {
                Desde = 0m,
                Hasta = 300000m / 12, // Q25,000 mensual
                Tasa = 0.05m,
                ExcesoSobre = 0m,
                ImpuestoFijo = 0m
            },
            new
            {
                Desde = 300000m / 12 + 0.01m, // Q25,000.01
                Hasta = 500000m / 12, // Q41,666.67
                Tasa = 0.07m,
                ExcesoSobre = 300000m / 12,
                ImpuestoFijo = (300000m / 12) * 0.05m // Q1,250
            },
            new
            {
                Desde = 500000m / 12 + 0.01m, // Q41,666.68
                Hasta = decimal.MaxValue,
                Tasa = 0.07m, // Tasa máxima
                ExcesoSobre = 500000m / 12,
                ImpuestoFijo = ((300000m / 12) * 0.05m) + ((200000m / 12) * 0.07m) // Q1,250 + Q1,166.67 = Q2,416.67
            }
        };

        var isrEscalaJson = JsonSerializer.Serialize(isrEscala, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var reglasGuatemala = new ReglasLaborales
        {
            Pais = "GT",
            VigenteDesde = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            VigenteHasta = null, // Sin fecha de vencimiento (activa indefinidamente)

            // IGSS (Instituto Guatemalteco de Seguridad Social)
            // Aporte empleado: 4.83% sobre salario ordinario
            IgssEmpleadoPct = 0.0483m,
            // Aporte patronal: 10.67% sobre salario ordinario
            IgssPatronalPct = 0.1067m,
            // Límite máximo base imponible IGSS: Q5,000
            IgssMaximoBase = 5000m,

            // IRTRA (Instituto de Recreación de los Trabajadores de la Empresa Privada de Guatemala)
            // 1% sobre total de planilla ordinaria (patronal)
            IrtraPct = 0.01m,

            // INTECAP (Instituto Técnico de Capacitación y Productividad)
            // 1% sobre total de planilla ordinaria (patronal)
            IntecapPct = 0.01m,

            // ISR (Impuesto Sobre la Renta) - Escala progresiva mensualizada
            IsrEscalaJson = isrEscalaJson,

            // Horas extras ordinarias: 150% (1.5 veces salario hora)
            HorasExtrasPct = 1.5m,

            // Horas extras nocturnas: 200% (2.0 veces salario hora)
            HorasExtrasNocturnasPct = 2.0m,

            // Bono Decreto 37-2001: Q250 mensuales
            // Este bono NO es afecto a IGSS ni ISR
            BonoDecretoMonto = 250.00m,

            // Configuración de redondeo
            RedondeoDecimales = 2,
            PoliticaRedondeo = "Normal", // Normal, Arriba, Abajo

            // Salario mínimo mensual 2025 (Acuerdo Gubernativo)
            // Actividades no agrícolas: Q3,035.14
            // Actividades agrícolas: Q3,035.14
            // Actividades de exportación y maquila: Q2,959.24
            // Usamos el más común (no agrícola)
            SalarioMinimoMensual = 3035.14m,

            // Jornada ordinaria: 8 horas/día × 5.5 días/semana × 4.33 semanas/mes = 190.52 horas/mes
            // O más conservador: 173.33 horas/mes (promedio anual 2080 hrs / 12 meses)
            JornadaOrdinariaHorasMes = 173.33m,

            Activo = true,
            CreadoEn = DateTime.UtcNow,
            CreadoPor = "System - DataSeeder",
            ModificadoEn = null,
            ModificadoPor = null
        };

        context.ReglasLaborales.Add(reglasGuatemala);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ ReglasLaborales para Guatemala 2025 creadas exitosamente:");
        Console.WriteLine($"   - IGSS Empleado: {reglasGuatemala.IgssEmpleadoPct:P2}");
        Console.WriteLine($"   - IGSS Patronal: {reglasGuatemala.IgssPatronalPct:P2}");
        Console.WriteLine($"   - IRTRA: {reglasGuatemala.IrtraPct:P2}");
        Console.WriteLine($"   - INTECAP: {reglasGuatemala.IntecapPct:P2}");
        Console.WriteLine($"   - Horas Extras: {reglasGuatemala.HorasExtrasPct:P0}");
        Console.WriteLine($"   - Bono Decreto: Q{reglasGuatemala.BonoDecretoMonto:N2}");
        Console.WriteLine($"   - Salario Mínimo: Q{reglasGuatemala.SalarioMinimoMensual:N2}");
        Console.WriteLine($"   - ISR Escala: {isrEscala.Length} tramos progresivos");
    }
}
