using ProyectoNomina.Backend.Models;
using ProyectoNomina.Backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace ProyectoNomina.Backend.Services
{
    public class NominaService
    {
        private readonly AppDbContext _context;

        // Tipo auxiliar para horas/ tarifas por empleado (opcional)
        public readonly record struct HorasTarifas(
            decimal HorasRegulares,
            decimal HorasExtra,
            decimal TarifaHora,
            decimal TarifaExtra
        );

        // Delegate opcional para parámetros legales por fecha (IGSS/IRTRA/ISR, etc.)
        // Devuelve null si no hay valor vigente y el servicio hará fallback.
        public delegate decimal? ParametrosResolver(string clave, DateTime fechaCorte);

        public NominaService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Mantiene el comportamiento actual (salario mensual, IGSS 4.83%, bonificación fija 250).
        /// </summary>
        public async Task Calcular(Nomina nomina)
        {
            await CalcularV2(
                nomina: nomina,
                periodoInicio: null,
                periodoFin: null,
                horasPorEmpleado: null,
                comisionesPorEmpleado: null,
                resolver: null
            );
        }

        /// <summary>
        /// Versión extendida: soporta horas, comisiones y parámetros legales (resolver).
        /// </summary>
        public async Task CalcularV2(
            Nomina nomina,
            DateTime? periodoInicio,
            DateTime? periodoFin,
            IDictionary<int, HorasTarifas>? horasPorEmpleado,
            IDictionary<int, decimal>? comisionesPorEmpleado,
            ParametrosResolver? resolver
        )
        {
            var empleados = await _context.Empleados
                .Where(e => e.EstadoLaboral == "Activo")
                .Include(e => e.Deducciones)
                .Include(e => e.Bonificaciones)
                .ToListAsync();

            var fechaCorte = periodoFin ?? DateTime.Today;

            // Parámetros con fallback (compatibles con tu lógica actual)
            decimal igssPctBase  = resolver?.Invoke("IGSS",  fechaCorte) ?? 0.0483M; // 4.83%
            decimal irtraPctBase = resolver?.Invoke("IRTRA", fechaCorte) ?? 0M;
            decimal isrPctBase   = resolver?.Invoke("ISR",   fechaCorte) ?? 0M;

            nomina.DetallesNomina ??= new List<DetalleNomina>();

            foreach (var empleado in empleados)
            {
                // === Base de cálculo ===
                decimal salarioBase;
                decimal? horasReg = null, horasExt = null, tarifa = null, tarifaExt = null;

                if (horasPorEmpleado != null && horasPorEmpleado.TryGetValue(empleado.Id, out var h))
                {
                    if (h.HorasRegulares < 0 || h.HorasExtra < 0)
                        throw new InvalidOperationException("Las horas no pueden ser negativas.");

                    if (h.TarifaHora <= 0)
                        throw new InvalidOperationException("La tarifa por hora debe ser > 0.");

                    var tarifaExtraEf = h.TarifaExtra > 0 ? h.TarifaExtra : (h.TarifaHora * 1.5M);
                    salarioBase = (h.HorasRegulares * h.TarifaHora) + (h.HorasExtra * tarifaExtraEf);

                    horasReg = h.HorasRegulares;
                    horasExt = h.HorasExtra;
                    tarifa   = h.TarifaHora;
                    tarifaExt= tarifaExtraEf;
                }
                else
                {
                    salarioBase = empleado.SalarioMensual;
                }

                // === Bonificaciones + Comisiones ===
                decimal bonificacionFija = 250M; // igual que tu versión actual
                decimal otrasBonificaciones = empleado.Bonificaciones?.Sum(b => b.Monto) ?? 0M;

                decimal comisionPeriodo = 0M;
                if (comisionesPorEmpleado != null && comisionesPorEmpleado.TryGetValue(empleado.Id, out var comi))
                {
                    if (comi < 0) throw new InvalidOperationException("La comisión no puede ser negativa.");
                    comisionPeriodo = comi;
                }

                decimal totalBonificaciones = bonificacionFija + otrasBonificaciones + comisionPeriodo;

                // === Deducciones ===
                decimal baseImponible = salarioBase + totalBonificaciones;

                decimal igss  = Math.Round(baseImponible * igssPctBase, 2);
                decimal irtra = Math.Round(baseImponible * irtraPctBase, 2);
                decimal isr   = Math.Round(baseImponible * isrPctBase,   2);

                decimal otrasDeducciones = empleado.Deducciones?.Sum(d => d.Monto) ?? 0M;

                decimal totalDeducciones = igss + irtra + isr + otrasDeducciones;

                // === Totales ===
                decimal salarioBruto = baseImponible;
                decimal salarioNeto  = salarioBruto - totalDeducciones;

                // === Desglose ===
                var desglose = new StringBuilder().Append($"IGSS: Q{igss:F2}");
                if (irtra > 0) desglose.Append($", IRTRA: Q{irtra:F2}");
                if (isr   > 0) desglose.Append($", ISR: Q{isr:F2}");
                if (otrasDeducciones > 0) desglose.Append($", Otras: Q{otrasDeducciones:F2}");

                // === Crear detalle ===
                var detalle = new DetalleNomina
                {
                    EmpleadoId = empleado.Id,

                    // NUEVO: solo asignamos si venían horas; si no, queda 0 por defecto
                    HorasRegulares = horasReg ?? 0m,
                    HorasExtra     = horasExt ?? 0m,
                    TarifaHora     = tarifa   ?? 0m,
                    TarifaExtra    = tarifaExt?? 0m,

                    SalarioBruto = salarioBruto,
                    Deducciones = totalDeducciones,
                    Bonificaciones = totalBonificaciones,
                    SalarioNeto = salarioNeto,
                    DesgloseDeducciones = desglose.ToString()
                };

                nomina.DetallesNomina.Add(detalle);
            }

            _context.Nominas.Update(nomina);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Genera un PDF de la nómina
        /// </summary>
        public async Task<byte[]> GenerarPdfAsync(Nomina nomina)
        {
            // Implementación básica - puedes mejorarla con una librería como QuestPDF
            var content = GenerarContenidoNomina(nomina);
            
            // Por ahora retornamos un PDF simple con texto plano
            // En una implementación real usarías una librería como QuestPDF, iTextSharp, etc.
            var bytes = System.Text.Encoding.UTF8.GetBytes($"NÓMINA PDF\n{content}");
            return await Task.FromResult(bytes);
        }

        /// <summary>
        /// Genera un Excel de la nómina
        /// </summary>
        public async Task<byte[]> GenerarExcelAsync(Nomina nomina)
        {
            // Implementación básica - puedes mejorarla con una librería como EPPlus
            var content = GenerarContenidoNomina(nomina);
            
            // Por ahora retornamos un CSV simple
            // En una implementación real usarías EPPlus, ClosedXML, etc.
            var bytes = System.Text.Encoding.UTF8.GetBytes($"NÓMINA EXCEL\n{content}");
            return await Task.FromResult(bytes);
        }

        /// <summary>
        /// Envía la nómina por email
        /// </summary>
        public async Task<bool> EnviarNominaPorEmailAsync(Nomina nomina, string email, string formato, string? mensaje = null)
        {
            try
            {
                // Implementación básica - en una implementación real usarías un servicio de email
                // como SendGrid, SMTP, etc.
                
                var contenido = GenerarContenidoNomina(nomina);
                var asunto = $"Nómina {nomina.Periodo ?? nomina.FechaGeneracion.ToString("yyyy-MM")} - {nomina.Descripcion}";
                
                // Simular envío de email
                Console.WriteLine($"Enviando email a: {email}");
                Console.WriteLine($"Asunto: {asunto}");
                Console.WriteLine($"Formato: {formato}");
                Console.WriteLine($"Mensaje: {mensaje ?? "Sin mensaje adicional"}");
                Console.WriteLine($"Contenido: {contenido}");
                
                // Simular delay de envío
                await Task.Delay(100);
                
                return true; // Retorna true si se envió exitosamente
            }
            catch (Exception)
            {
                return false; // Retorna false si hubo error
            }
        }

        /// <summary>
        /// Genera el contenido básico de la nómina para reportes
        /// </summary>
        private string GenerarContenidoNomina(Nomina nomina)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ID: {nomina.Id}");
            sb.AppendLine($"Descripción: {nomina.Descripcion}");
            sb.AppendLine($"Fecha de Generación: {nomina.FechaGeneracion:dd/MM/yyyy}");
            sb.AppendLine($"Período: {nomina.Periodo ?? "N/A"}");
            sb.AppendLine($"Estado: {nomina.Estado}");
            sb.AppendLine($"Monto Total: Q{nomina.MontoTotal:N2}");
            sb.AppendLine($"Total Bruto: Q{nomina.TotalBruto:N2}");
            sb.AppendLine($"Total Deducciones: Q{nomina.TotalDeducciones:N2}");
            sb.AppendLine($"Total Bonificaciones: Q{nomina.TotalBonificaciones:N2}");
            sb.AppendLine($"Total Neto: Q{nomina.TotalNeto:N2}");
            
            if (nomina.DetallesNomina?.Any() == true)
            {
                sb.AppendLine("\nDETALLE DE EMPLEADOS:");
                foreach (var detalle in nomina.DetallesNomina)
                {
                    sb.AppendLine($"- Empleado {detalle.EmpleadoId}: Q{detalle.SalarioNeto:N2}");
                }
            }

            return sb.ToString();
        }
    }
}
