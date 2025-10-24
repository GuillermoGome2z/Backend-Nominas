using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Services;
using ProyectoNomina.Shared.Models.DTOs;
using ProyectoNomina.Backend.Models;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,RRHH")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class ReportesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ReporteService _reporteService;

        public ReportesController(AppDbContext context, ReporteService reporteService)
        {
            _context = context;
            _reporteService = reporteService;
        }

        // ============================
        //    REPORTES GENERALES
        // ============================

        [HttpGet("general")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> ObtenerReporteGeneral([FromQuery] string? estadoLaboral = null)
        {
            try
            {
                var query = _context.Empleados.AsQueryable();

                // Filtrar por estado laboral si se proporciona
                if (!string.IsNullOrWhiteSpace(estadoLaboral))
                {
                    query = query.Where(e => e.EstadoLaboral == estadoLaboral);
                }

                var empleados = await query
                    .Include(e => e.Departamento)
                    .Include(e => e.Puesto)
                    .Select(e => new 
                    {
                        Id = e.Id,
                        NombreCompleto = e.NombreCompleto,
                        Correo = e.Correo,
                        Telefono = e.Telefono,
                        SalarioMensual = e.SalarioMensual,
                        FechaContratacion = e.FechaContratacion,
                        EstadoLaboral = e.EstadoLaboral,
                        Departamento = e.Departamento != null ? e.Departamento.Nombre : "Sin Departamento",
                        Puesto = e.Puesto != null ? e.Puesto.Nombre : "Sin Puesto"
                    })
                    .ToListAsync();

                return Ok(new 
                {
                    totalEmpleados = empleados.Count,
                    empleados = empleados,
                    estadoFiltro = estadoLaboral ?? "Todos"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al generar reporte general", error = ex.Message });
            }
        }

        [HttpGet("excel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ExportarExcelGeneral([FromQuery] string? estadoLaboral = null)
        {
            return Ok(new { message = "Endpoint Excel disponible", redirect = "/api/reportes/empleados.xlsx" });
        }

        [HttpGet("pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ExportarPdfGeneral([FromQuery] string? estadoLaboral = null)
        {
            return Ok(new { message = "Endpoint PDF disponible", redirect = "/api/reportes/Expedientes/pdf" });
        }

        // ============================
        //    REPORTES EXISTENTES
        // ============================

        [HttpGet("Nominas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ReporteNominaDto>>> ObtenerReporteNominas()
        {
            var nominas = await _context.Nominas
                .Include(n => n.Detalles)
                .ToListAsync();

            if (!nominas.Any())
                return NotFound("No se encontraron nóminas registradas.");

            var reporte = nominas.Select(n => new ReporteNominaDto
            {
                NominaId = n.Id,
                Descripcion = n.Descripcion,
                FechaGeneracion = n.FechaGeneracion,
                TotalSalarios = n.Detalles.Sum(d => d.SalarioBruto),
                TotalBonificaciones = n.Detalles.Sum(d => d.Bonificaciones),
                TotalDeducciones = n.Detalles.Sum(d => d.Deducciones),
                TotalNeto = n.Detalles.Sum(d => d.SalarioNeto)
            }).ToList();

            return Ok(reporte);
        }

        [HttpGet("Expedientes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<ReporteExpedienteDto>>> ObtenerReporteExpedientes()
        {
            var empleados = await _context.Empleados.ToListAsync();
            var tiposRequeridos = await _context.TiposDocumento
                .Where(t => t.EsRequerido)
                .Select(t => t.Id)
                .ToListAsync();

            var reporte = new List<ReporteExpedienteDto>();

            foreach (var emp in empleados)
            {
                var entregados = await _context.DocumentosEmpleado
                    .Where(d => d.EmpleadoId == emp.Id)
                    .Select(d => d.TipoDocumentoId)
                    .Distinct()
                    .ToListAsync();

                var faltantes = tiposRequeridos.Except(entregados).ToList();

                var estado = faltantes.Count == 0 ? "Completo"
                           : entregados.Count == 0 ? "Incompleto"
                           : "En proceso";

                reporte.Add(new ReporteExpedienteDto
                {
                    Empleado = emp.NombreCompleto,
                    EstadoExpediente = estado,
                    DocumentosRequeridos = tiposRequeridos.Count,
                    DocumentosPresentados = entregados.Count,
                    DocumentosFaltantes = faltantes.Count
                });
            }

            return Ok(reporte);
        }

        [HttpGet("Expedientes/pdf")]
        [Produces("application/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenerarPdfExpedientes()
        {
            var empleados = await _context.Empleados.ToListAsync();
            var tiposRequeridos = await _context.TiposDocumento
                .Where(t => t.EsRequerido)
                .Select(t => t.Id)
                .ToListAsync();

            var reporte = new List<ReporteExpedienteDto>();

            foreach (var emp in empleados)
            {
                var entregados = await _context.DocumentosEmpleado
                    .Where(d => d.EmpleadoId == emp.Id)
                    .Select(d => d.TipoDocumentoId)
                    .Distinct()
                    .ToListAsync();

                var faltantes = tiposRequeridos.Except(entregados).ToList();

                var estado = faltantes.Count == 0 ? "Completo"
                           : entregados.Count == 0 ? "Incompleto"
                           : "En proceso";

                reporte.Add(new ReporteExpedienteDto
                {
                    Empleado = emp.NombreCompleto,
                    EstadoExpediente = estado,
                    DocumentosRequeridos = tiposRequeridos.Count,
                    DocumentosPresentados = entregados.Count,
                    DocumentosFaltantes = faltantes.Count
                });
            }

            var pdf = _reporteService.GenerarReporteExpediente(reporte);
            return File(pdf, "application/pdf", "ReporteExpediente.pdf");
        }

        [HttpGet("InformacionAcademica/pdf")]
        [Produces("application/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GenerarReporteInformacionAcademicaPdf()
        {
            var datos = await _context.InformacionAcademica.Include(i => i.Empleado).ToListAsync();

            if (!datos.Any())
                return NotFound("No hay registros de información académica.");

            var pdf = _reporteService.GenerarReporteInformacionAcademica(datos);
            return File(pdf, "application/pdf", "ReporteInformacionAcademica.pdf");
        }

        [HttpGet("Ajustes/pdf")]
        [Produces("application/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GenerarReporteAjustesPdf()
        {
            var ajustes = await _context.AjustesManuales.Include(a => a.Empleado).ToListAsync();

            if (!ajustes.Any())
                return NotFound("No hay ajustes manuales registrados.");

            var pdf = _reporteService.GenerarReporteAjustesManuales(ajustes);
            return File(pdf, "application/pdf", "ReporteAjustes.pdf");
        }

        [HttpGet("Auditoria/pdf")]
        [Produces("application/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GenerarReporteAuditoriaPdf()
        {
            var auditoria = await _context.Auditoria.ToListAsync();

            if (!auditoria.Any())
                return NotFound("No hay registros de auditoría.");

            var pdf = _reporteService.GenerarReporteAuditoria(auditoria);
            return File(pdf, "application/pdf", "ReporteAuditoria.pdf");
        }

        [HttpGet("PorTipoDocumento")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ItemDocumentoDto>>> ObtenerReportePorTipoDocumento()
        {
            var resultado = await _context.TiposDocumento
                .Select(td => new ItemDocumentoDto
                {
                    Tipo = td.Nombre,
                    TotalRequeridos = td.DocumentosEmpleados.Count(),
                    Entregados = td.DocumentosEmpleados.Count(d => d.RutaArchivo != null),
                    Faltantes = td.DocumentosEmpleados.Count(d => d.RutaArchivo == null)
                }).ToListAsync();

            return Ok(resultado);
        }

        [HttpGet("DocumentosPorEmpleado")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<List<ReporteDocumentosEmpleadoDto>>> ObtenerDocumentosPorEmpleado()
{
    var empleados = await _context.Empleados
        .Include(e => e.Documentos!) // <- null-forgiving para navegación
        .ThenInclude(d => d.TipoDocumento)
        .ToListAsync();

    var resultado = empleados.Select(e => new ReporteDocumentosEmpleadoDto
    {
        NombreEmpleado = e.NombreCompleto,
        Documentos = (e.Documentos ?? new List<DocumentoEmpleado>()) // <- coalesce seguro
            .Select(d => new ItemDocumentoResumenDto
            {
                Tipo = d.TipoDocumento.Nombre,
                Fecha = d.FechaSubida
            }).ToList()
    }).ToList();

    return Ok(resultado);
}

        // =========================================
        // EXPORTACIONES CSV / XLSX 
        // =========================================

        // GET /api/reportes/empleados.csv
        [HttpGet("empleados.csv")]
        [Produces("text/csv")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ExportEmpleadosCsv([FromQuery] int? departamentoId, [FromQuery] int? puestoId)
        {
            var q = _context.Empleados
                .Include(e => e.Departamento)
                .Include(e => e.Puesto)
                .AsNoTracking()
                .AsQueryable();

            if (departamentoId.HasValue) q = q.Where(e => e.DepartamentoId == departamentoId.Value);
            if (puestoId.HasValue) q = q.Where(e => e.PuestoId == puestoId.Value);

            var data = await q.Select(e => new
            {
                e.Id,
                Nombre = e.NombreCompleto,
                Departamento = e.Departamento != null ? e.Departamento.Nombre : "",
                Puesto = e.Puesto != null ? e.Puesto.Nombre : ""
            }).ToListAsync();

            if (data.Count == 0) return NoContent();

            var cols = new (string, Func<dynamic, object?>)[]
            {
                ("Id", d => d.Id),
                ("Nombre", d => d.Nombre),
                ("Departamento", d => d.Departamento),
                ("Puesto", d => d.Puesto)
            };

            var bytes = ExportService.ToCsv(data, cols);
            var fileName = $"empleados_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
            return File(bytes, "text/csv", fileName);
        }

        // GET /api/reportes/empleados.xlsx
        [HttpGet("empleados.xlsx")]
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ExportEmpleadosXlsx([FromQuery] int? departamentoId, [FromQuery] int? puestoId)
        {
            var q = _context.Empleados
                .Include(e => e.Departamento)
                .Include(e => e.Puesto)
                .AsNoTracking()
                .AsQueryable();

            if (departamentoId.HasValue) q = q.Where(e => e.DepartamentoId == departamentoId.Value);
            if (puestoId.HasValue) q = q.Where(e => e.PuestoId == puestoId.Value);

            var data = await q.Select(e => new
            {
                e.Id,
                Nombre = e.NombreCompleto,
                Departamento = e.Departamento != null ? e.Departamento.Nombre : "",
                Puesto = e.Puesto != null ? e.Puesto.Nombre : ""
            }).ToListAsync();

            if (data.Count == 0) return NoContent();

            var cols = new (string, Func<dynamic, object?>)[]
            {
                ("Id", d => d.Id),
                ("Nombre", d => d.Nombre),
                ("Departamento", d => d.Departamento),
                ("Puesto", d => d.Puesto)
            };

            var bytes = ExportService.ToXlsx(data, cols, "Empleados");
            var fileName = $"empleados_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // GET /api/reportes/nomina.csv?nominaId=123
        [HttpGet("nomina.csv")]
        [Produces("text/csv")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ExportNominaCsv([FromQuery] int nominaId)
        {
            var data = await _context.DetalleNominas
                .Include(d => d.Empleado)
                .AsNoTracking()
                .Where(d => d.NominaId == nominaId)
                .Select(d => new
                {
                    d.Id,
                    d.NominaId,
                    d.EmpleadoId,
                    Empleado = d.Empleado.NombreCompleto,
                    d.SalarioBruto,
                    d.Bonificaciones,
                    d.Deducciones,
                    d.SalarioNeto
                })
                .ToListAsync();

            if (data.Count == 0) return NoContent();

            var cols = new (string, Func<dynamic, object?>)[]
            {
                ("DetalleId", d => d.Id),
                ("NominaId", d => d.NominaId),
                ("EmpleadoId", d => d.EmpleadoId),
                ("Empleado", d => d.Empleado),
                ("SalarioBruto", d => d.SalarioBruto),
                ("Bonificaciones", d => d.Bonificaciones),
                ("Deducciones", d => d.Deducciones),
                ("SalarioNeto", d => d.SalarioNeto)
            };

            var bytes = ExportService.ToCsv(data, cols);
            var fileName = $"nomina_{nominaId}_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
            return File(bytes, "text/csv", fileName);
        }

        // GET /api/reportes/nomina.xlsx?nominaId=123
        [HttpGet("nomina.xlsx")]
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ExportNominaXlsx([FromQuery] int nominaId)
        {
            var data = await _context.DetalleNominas
                .Include(d => d.Empleado)
                .AsNoTracking()
                .Where(d => d.NominaId == nominaId)
                .Select(d => new
                {
                    d.Id,
                    d.NominaId,
                    d.EmpleadoId,
                    Empleado = d.Empleado.NombreCompleto,
                    d.SalarioBruto,
                    d.Bonificaciones,
                    d.Deducciones,
                    d.SalarioNeto
                })
                .ToListAsync();

            if (data.Count == 0) return NoContent();

            var cols = new (string, Func<dynamic, object?>)[]
            {
                ("DetalleId", d => d.Id),
                ("NominaId", d => d.NominaId),
                ("EmpleadoId", d => d.EmpleadoId),
                ("Empleado", d => d.Empleado),
                ("SalarioBruto", d => d.SalarioBruto),
                ("Bonificaciones", d => d.Bonificaciones),
                ("Deducciones", d => d.Deducciones),
                ("SalarioNeto", d => d.SalarioNeto)
            };

            var bytes = ExportService.ToXlsx(data, cols, $"Nomina_{nominaId}");
            var fileName = $"nomina_{nominaId}_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // =========================================
        // PASO 17: PDF NÓMINA (NUEVO)
        // =========================================

        // GET /api/reportes/nomina/{nominaId}/pdf  → PDF global (resumen por empleado)
        [HttpGet("nomina/{nominaId:int}/pdf")]
        [Produces("application/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GenerarNominaGlobalPdf(int nominaId)
        {
            var nomina = await _context.Nominas
                .Include(n => n.Detalles)
                    .ThenInclude(d => d.Empleado)
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == nominaId);

            if (nomina == null || nomina.Detalles == null || nomina.Detalles.Count == 0)
                return NotFound("No se encontró la nómina o no tiene detalles.");

            var pdf = _reporteService.GenerarNominaGeneral(nomina, nomina.Detalles);
            var fileName = $"Nomina_{nominaId}_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
            return File(pdf, "application/pdf", fileName);
        }

        // GET /api/reportes/nomina/{nominaId}/empleado/{empleadoId}/pdf  → Recibo individual
        [HttpGet("nomina/{nominaId:int}/empleado/{empleadoId:int}/pdf")]
        [Produces("application/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GenerarReciboEmpleadoPdf(int nominaId, int empleadoId)
        {
            var nomina = await _context.Nominas
                .Include(n => n.Detalles)
                    .ThenInclude(d => d.Empleado)
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == nominaId);

            if (nomina == null) return NotFound("Nómina no encontrada.");

            var detalle = nomina.Detalles?.FirstOrDefault(d => d.EmpleadoId == empleadoId);
            if (detalle == null) return NotFound("No existe detalle de nómina para el empleado indicado.");

            var pdf = _reporteService.GenerarReciboNominaEmpleado(nomina, detalle);
            var fileName = $"Recibo_{empleadoId}_Nomina_{nominaId}_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
            return File(pdf, "application/pdf", fileName);
        }
    }
}
