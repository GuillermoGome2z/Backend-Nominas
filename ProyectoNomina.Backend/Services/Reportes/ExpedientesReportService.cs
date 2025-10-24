using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;

namespace ProyectoNomina.Backend.Services.Reportes
{
    public class ExpedientesReportService
    {
        private readonly AppDbContext _context;

        public ExpedientesReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ExpedienteRow>> ObtenerExpedientesAsync(string? estado = null, int? departamentoId = null)
        {
            try
            {
                // Obtener todos los tipos de documentos requeridos
                var tiposRequeridos = await _context.TiposDocumento
                    .Where(t => t.EsRequerido)
                    .Select(t => t.Id)
                    .ToListAsync();

                // Query base de empleados con filtros
                var empleadosQuery = _context.Empleados
                    .Include(e => e.Departamento)
                    .AsQueryable();

                // Aplicar filtro por departamento si se especifica
                if (departamentoId.HasValue)
                {
                    empleadosQuery = empleadosQuery.Where(e => e.DepartamentoId == departamentoId.Value);
                }

                var empleados = await empleadosQuery.ToListAsync();
                var resultado = new List<ExpedienteRow>();

                foreach (var empleado in empleados)
                {
                    // Obtener documentos presentados por este empleado
                    var documentosPresentados = await _context.DocumentosEmpleado
                        .Where(d => d.EmpleadoId == empleado.Id && !string.IsNullOrEmpty(d.RutaArchivo))
                        .Select(d => d.TipoDocumentoId)
                        .Distinct()
                        .ToListAsync();

                    var faltantes = tiposRequeridos.Except(documentosPresentados).ToList();

                    // Determinar el estado del expediente
                    var estadoExpediente = faltantes.Count == 0 ? "Completo"
                                        : documentosPresentados.Count == 0 ? "Incompleto"
                                        : "En proceso";

                    // Solo incluir si coincide con el filtro de estado (si se especifica)
                    if (!string.IsNullOrEmpty(estado) && estadoExpediente != estado)
                        continue;

                    resultado.Add(new ExpedienteRow
                    {
                        Empleado = empleado.NombreCompleto,
                        Estado = estadoExpediente,
                        Requeridos = tiposRequeridos.Count,
                        Presentados = documentosPresentados.Count,
                        Faltantes = faltantes.Count
                    });
                }

                // Ordenar por estado (Incompleto, En proceso, Completo) y luego por nombre
                return resultado
                    .OrderBy(x => GetEstadoPrioridad(x.Estado))
                    .ThenBy(x => x.Empleado)
                    .ToList();
            }
            catch (Exception ex)
            {
                // Log del error (podrías usar ILogger aquí)
                throw new InvalidOperationException($"Error al obtener datos de expedientes: {ex.Message}", ex);
            }
        }

        public async Task<Dictionary<string, int>> ObtenerEstadisticasAsync(int? departamentoId = null)
        {
            try
            {
                var expedientes = await ObtenerExpedientesAsync(null, departamentoId);
                
                return new Dictionary<string, int>
                {
                    ["Total"] = expedientes.Count,
                    ["Completo"] = expedientes.Count(x => x.Estado == "Completo"),
                    ["EnProceso"] = expedientes.Count(x => x.Estado == "En proceso"),
                    ["Incompleto"] = expedientes.Count(x => x.Estado == "Incompleto"),
                    ["TotalRequeridos"] = expedientes.Sum(x => x.Requeridos),
                    ["TotalPresentados"] = expedientes.Sum(x => x.Presentados),
                    ["TotalFaltantes"] = expedientes.Sum(x => x.Faltantes)
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al obtener estadísticas de expedientes: {ex.Message}", ex);
            }
        }

        private static int GetEstadoPrioridad(string estado)
        {
            return estado switch
            {
                "Incompleto" => 0,
                "En proceso" => 1,
                "Completo" => 2,
                _ => 3
            };
        }
    }
}