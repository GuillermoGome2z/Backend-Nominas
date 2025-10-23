using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Backend.Services;
using ProyectoNomina.Shared.Models.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace ProyectoNomina.Backend.Controllers
{
    [Authorize(Roles = "Admin,RRHH")]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    // Por [Authorize], documentamos 401 y 403 a nivel de clase
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class NominasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NominaService _nominaService;

        public NominasController(AppDbContext context, NominaService nominaService)
        {
            _context = context;
            _nominaService = nominaService;
        }

        // ============================================================
        // GET /api/Nominas  (paginado + filtros + 422 por rango inválido) → ahora responde DTO
        // ============================================================
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<NominaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<NominaDto>>> GetNominas(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? departamentoId = null,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            CancellationToken ct = default)
        {
            // saneo de paginación
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            // validación rango fechas → 422
            if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio.Value.Date > fechaFin.Value.Date)
            {
                return UnprocessableEntity(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["rangoFechas"] = new[] { "fechaInicio no puede ser mayor que fechaFin." }
                }));
            }

            // base query (sin Include: proyectamos a DTO)
            var query = _context.Nominas
                .AsNoTracking()
                .AsQueryable();

            // filtros
            if (fechaInicio.HasValue)
                query = query.Where(n => n.FechaGeneracion.Date >= fechaInicio.Value.Date);

            if (fechaFin.HasValue)
                query = query.Where(n => n.FechaGeneracion.Date <= fechaFin.Value.Date);

            if (departamentoId.HasValue)
            {
                // Filtramos por departamento via subconsulta sobre DetalleNominas
                query = query.Where(n => _context.DetalleNominas
                    .Any(d => d.NominaId == n.Id && d.Empleado.DepartamentoId == departamentoId.Value));
            }

            var total = await query.CountAsync(ct);

            // Proyección a DTO "ligero" (Id, Descripcion, FechaGeneracion)
            var nominas = await query
                .OrderByDescending(n => n.FechaGeneracion)
                .ThenBy(n => n.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NominaDto
                {
                    Id = n.Id,
                    Descripcion = n.Descripcion,
                    FechaGeneracion = n.FechaGeneracion
                })
                .ToListAsync(ct);

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(nominas);
        }

        // ============================================================
        // GET /api/Nominas/{id}
        // ============================================================
        [HttpGet("{id}", Name = "GetNominaById")]
        [ProducesResponseType(typeof(Nomina), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Nomina>> GetNomina(int id, CancellationToken ct = default)
        {
            var nomina = await _context.Nominas
                .AsNoTracking()
                .Include(n => n.Detalles)
                    .ThenInclude(d => d.Empleado)
                .FirstOrDefaultAsync(n => n.Id == id, ct);

            if (nomina == null) return NotFound();

            return Ok(nomina);
        }

        // ============================================================
        // GET /api/Nominas/completa  (DTO con detalles + paginado)
        // ============================================================
        [HttpGet("completa")]
        [ProducesResponseType(typeof(IEnumerable<NominaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<NominaDto>>> ObtenerNominasCompletas(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var baseQuery = _context.Nominas
                .AsNoTracking()
                .Include(n => n.Detalles)
                    .ThenInclude(d => d.Empleado);

            var total = await baseQuery.CountAsync(ct);

            var resultado = await baseQuery
                .OrderByDescending(n => n.FechaGeneracion)
                .ThenBy(n => n.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NominaDto
                {
                    Id = n.Id,
                    Descripcion = n.Descripcion,
                    FechaGeneracion = n.FechaGeneracion,
                    Detalles = n.Detalles.Select(d => new DetalleNominaDto
                    {
                        EmpleadoId = d.EmpleadoId,
                        NombreEmpleado = d.Empleado != null ? d.Empleado.NombreCompleto : "",
                        SalarioBruto = d.SalarioBruto,
                        Deducciones = d.Deducciones,
                        Bonificaciones = d.Bonificaciones,
                        SalarioNeto = d.SalarioNeto,
                        DesgloseDeducciones = d.DesgloseDeducciones
                    }).ToList()
                })
                .ToListAsync(ct);

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(resultado);
        }

        // ============================================================
        // GET /api/Nominas/listado  (DTO simple + paginado) – se mantiene por compatibilidad
        // ============================================================
        [HttpGet("listado")]
        [ProducesResponseType(typeof(IEnumerable<NominaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerNominas(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var baseQuery = _context.Nominas.AsNoTracking();

            var total = await baseQuery.CountAsync(ct);

            var nominas = await baseQuery
                .OrderByDescending(n => n.FechaGeneracion)
                .ThenBy(n => n.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NominaDto
                {
                    Id = n.Id,
                    Descripcion = n.Descripcion,
                    FechaGeneracion = n.FechaGeneracion
                })
                .ToListAsync(ct);

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(nominas);
        }

        // ============================================================
        // GET /api/Nominas/{id}/detalle?empleadoId=123 (opcional)
        // ============================================================
        [HttpGet("{id:int}/detalle")]
        [ProducesResponseType(typeof(NominaDetalleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NominaDetalleDto>> GetDetalle(int id, [FromQuery] int? empleadoId = null, CancellationToken ct = default)
        {
            var nomina = await _context.Nominas
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id, ct);

            if (nomina == null)
                return NotFound();

            var qDetalles = _context.DetalleNominas
                .AsNoTracking()
                .Where(d => d.NominaId == id)
                .Include(d => d.Empleado)
                .AsQueryable();

            if (empleadoId.HasValue)
                qDetalles = qDetalles.Where(d => d.EmpleadoId == empleadoId.Value);

            var items = await qDetalles
                .Select(d => new DetalleNominaDto
                {
                    Id = d.Id,
                    NominaId = d.NominaId,
                    EmpleadoId = d.EmpleadoId,
                    SalarioBruto = d.SalarioBruto,
                    Deducciones = d.Deducciones,
                    Bonificaciones = d.Bonificaciones,
                    SalarioNeto = d.SalarioNeto,
                    DesgloseDeducciones = d.DesgloseDeducciones,
                    NombreEmpleado = d.Empleado != null ? d.Empleado.NombreCompleto : string.Empty
                })
                .OrderBy(i => i.NombreEmpleado)
                .ToListAsync(ct);

            var totalBruto = await qDetalles.SumAsync(d => (decimal?)d.SalarioBruto, ct) ?? 0m;
            var totalDeducciones = await qDetalles.SumAsync(d => (decimal?)d.Deducciones, ct) ?? 0m;
            var totalBonificaciones = await qDetalles.SumAsync(d => (decimal?)d.Bonificaciones, ct) ?? 0m;
            var totalNeto = await qDetalles.SumAsync(d => (decimal?)d.SalarioNeto, ct) ?? 0m;

            var dto = new NominaDetalleDto
            {
                NominaId = nomina.Id,
                FechaGeneracion = nomina.FechaGeneracion,
                Descripcion = nomina.Descripcion ?? string.Empty,
                TotalBruto = totalBruto,
                TotalDeducciones = totalDeducciones,
                TotalBonificaciones = totalBonificaciones,
                TotalNeto = totalNeto,
                Items = items
            };

            return Ok(dto);
        }

        // ============================================================
        // POST /api/Nominas  (crear registro vacío)
        // ============================================================
        [HttpPost]
        [ProducesResponseType(typeof(Nomina), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PostNomina([FromBody] CrearNominaDto dto, CancellationToken ct = default)
        {
            var nomina = new Nomina
            {
                Descripcion = dto.Descripcion,
                FechaGeneracion = DateTime.Now
            };

            _context.Nominas.Add(nomina);
            await _context.SaveChangesAsync(ct);

            return CreatedAtRoute("GetNominaById", new { id = nomina.Id }, nomina);
        }

        // ============================================================
        // POST /api/Nominas/generar  (crear + calcular por rango)
        // Body: { fechaInicio, fechaFin, descripcion?, departamentoId? }
        // ============================================================
        [HttpPost("generar")]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerarNominaPorRango([FromBody] GenerarNominaRangoDto dto, CancellationToken ct = default)
        {
            // Validaciones de rango → 422
            if (!dto.FechaInicio.HasValue || !dto.FechaFin.HasValue)
            {
                return UnprocessableEntity(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["fechas"] = new[] { "fechaInicio y fechaFin son requeridos." }
                }));
            }

            var inicio = dto.FechaInicio.Value.Date;
            var fin = dto.FechaFin.Value.Date;

            if (inicio > fin)
            {
                return UnprocessableEntity(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["rangoFechas"] = new[] { "fechaInicio no puede ser mayor que fechaFin." }
                }));
            }

            var nomina = new Nomina
            {
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion)
                    ? $"Nómina {inicio:yyyy-MM-dd} a {fin:yyyy-MM-dd}"
                    : dto.Descripcion!.Trim(),
                FechaGeneracion = DateTime.Now
            };

            _context.Nominas.Add(nomina);
            await _context.SaveChangesAsync(ct);

            // Se mantiene tu lógica actual (retrocompatible)
            await _nominaService.Calcular(nomina);
            await _context.SaveChangesAsync(ct);

            return CreatedAtRoute("GetNominaById", new { id = nomina.Id }, new
            {
                mensaje = "✅ Nómina generada y calculada correctamente.",
                nominaId = nomina.Id
            });
        }

        // ============================================================
        // POST /api/Nominas/generar-v2  (crear + calcular V2 con horas/comisiones/parámetros)
        // Body: GenerarNominaV2Dto
        // ============================================================
        [HttpPost("generar-v2")]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerarNominaV2([FromBody] GenerarNominaV2Dto dto, CancellationToken ct = default)
        {
            // Validaciones de rango
            if (!dto.FechaInicio.HasValue || !dto.FechaFin.HasValue)
            {
                return UnprocessableEntity(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["fechas"] = new[] { "fechaInicio y fechaFin son requeridos." }
                }));
            }

            var inicio = dto.FechaInicio.Value.Date;
            var fin = dto.FechaFin.Value.Date;
            if (inicio > fin)
            {
                return UnprocessableEntity(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["rangoFechas"] = new[] { "fechaInicio no puede ser mayor que fechaFin." }
                }));
            }

            // Validaciones básicas de horas/comisiones
            if (dto.Horas != null)
            {
                foreach (var h in dto.Horas)
                {
                    if (h.HorasRegulares < 0 || h.HorasExtra < 0)
                        return UnprocessableEntity(new ValidationProblemDetails(new Dictionary<string, string[]>
                        {
                            [$"empleado:{h.EmpleadoId}"] = new[] { "Las horas no pueden ser negativas." }
                        }));

                    if (h.TarifaHora <= 0)
                        return UnprocessableEntity(new ValidationProblemDetails(new Dictionary<string, string[]>
                        {
                            [$"empleado:{h.EmpleadoId}"] = new[] { "TarifaHora debe ser > 0." }
                        }));
                }
            }

            if (dto.Comisiones != null)
            {
                foreach (var c in dto.Comisiones)
                {
                    if (c.Monto < 0)
                        return UnprocessableEntity(new ValidationProblemDetails(new Dictionary<string, string[]>
                        {
                            [$"empleado:{c.EmpleadoId}"] = new[] { "La comisión no puede ser negativa." }
                        }));
                }
            }

            var nomina = new Nomina
            {
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion)
                    ? $"Nómina {inicio:yyyy-MM-dd} a {fin:yyyy-MM-dd}"
                    : dto.Descripcion!.Trim(),
                FechaGeneracion = DateTime.Now
            };

            _context.Nominas.Add(nomina);
            await _context.SaveChangesAsync(ct);

            // Mapear horas
            IDictionary<int, NominaService.HorasTarifas>? horasMap = null;
            if (dto.Horas != null && dto.Horas.Any())
            {
                horasMap = dto.Horas
                    .GroupBy(h => h.EmpleadoId)
                    .ToDictionary(
                        g => g.Key,
                        g =>
                        {
                            var last = g.Last();
                            var tarifaExtraEf = last.TarifaExtra > 0 ? last.TarifaExtra : (last.TarifaHora * 1.5m);
                            return new NominaService.HorasTarifas(
                                last.HorasRegulares,
                                last.HorasExtra,
                                last.TarifaHora,
                                tarifaExtraEf
                            );
                        });
            }

            // Mapear comisiones
            IDictionary<int, decimal>? comisionesMap = null;
            if (dto.Comisiones != null && dto.Comisiones.Any())
            {
                comisionesMap = dto.Comisiones
                    .GroupBy(c => c.EmpleadoId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Monto));
            }

            // Resolver de parámetros legales (inline opcional)
            NominaService.ParametrosResolver? resolver = null;
            if (dto.ParametrosLegales != null && dto.ParametrosLegales.Any())
            {
                var dict = dto.ParametrosLegales
                    .GroupBy(p => p.Clave.Trim().ToUpperInvariant())
                    .ToDictionary(g => g.Key, g => g.Last().Valor);

                resolver = (clave, fecha) =>
                {
                    if (string.IsNullOrWhiteSpace(clave)) return null;
                    var k = clave.Trim().ToUpperInvariant();
                    return dict.TryGetValue(k, out var val) ? val : null;
                };
            }

            // Calcular con V2 (si no hay resolver/horas/comisiones, hace fallback y queda como tu versión actual)
            await _nominaService.CalcularV2(
                nomina: nomina,
                periodoInicio: inicio,
                periodoFin: fin,
                horasPorEmpleado: horasMap,
                comisionesPorEmpleado: comisionesMap,
                resolver: resolver
            );

            await _context.SaveChangesAsync(ct);

            return CreatedAtRoute("GetNominaById", new { id = nomina.Id }, new
            {
                mensaje = "✅ Nómina (V2) generada y calculada correctamente.",
                nominaId = nomina.Id
            });
        }

        // ============================================================
        // POST /api/Nominas/procesar/{id}  (recalcular usando el servicio)
        // ============================================================
        [HttpPost("procesar/{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcesarNominaExistente(int id, CancellationToken ct = default)
        {
            var nomina = await _context.Nominas
                .Include(n => n.Detalles)
                .FirstOrDefaultAsync(n => n.Id == id, ct);

            if (nomina == null)
                return NotFound("❌ Nómina no encontrada.");

            // Limpiar detalles anteriores si existen
            _context.DetalleNominas.RemoveRange(nomina.Detalles);
            await _context.SaveChangesAsync(ct);

            await _nominaService.Calcular(nomina);
            await _context.SaveChangesAsync(ct);

            return Ok(new
            {
                mensaje = "✅ Nómina procesada correctamente.",
                nominaId = nomina.Id
            });
        }

        // ============================================================
        // GET /api/Nominas/GenerarPdf/{id}
        // ============================================================
        [HttpGet("GenerarPdf/{id}")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerarPdf(int id, [FromServices] ReporteService reporteService, CancellationToken ct = default)
        {
            try
            {
                var nomina = await _context.Nominas
                    .Include(n => n.Detalles)
                        .ThenInclude(d => d.Empleado)
                    .FirstOrDefaultAsync(n => n.Id == id, ct);

                if (nomina == null)
                    return NotFound("No se encontró la nómina.");

                var pdfBytes = reporteService.GenerarReporteNominaPdf(nomina);
                return File(pdfBytes, "application/pdf", $"Nomina_{id}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"⚠️ Error al generar PDF: {ex.Message}");
            }
        }

        // ============================================================
        // GET /api/Nominas/GenerarExcel/{id}
        // ============================================================
        [HttpGet("GenerarExcel/{id}")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerarExcel(int id, [FromServices] ReporteService reporteService, CancellationToken ct = default)
        {
            var nomina = await _context.Nominas
                .Include(n => n.Detalles)
                    .ThenInclude(d => d.Empleado)
                .FirstOrDefaultAsync(n => n.Id == id, ct);

            if (nomina == null)
                return NotFound("Nómina no encontrada.");

            var excelBytes = reporteService.GenerarReporteNominaExcel(nomina);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Nomina_{id}.xlsx");
        }

        // ============================================================
        // PUT /api/Nominas/{id}
        // ============================================================
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutNomina(int id, [FromBody] Nomina nomina, CancellationToken ct = default)
        {
            if (id != nomina.Id) return BadRequest();

            _context.Entry(nomina).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Nominas.AnyAsync(n => n.Id == id, ct))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // ============================================================
        // DELETE /api/Nominas/{id}
        // ============================================================
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteNomina(int id, CancellationToken ct = default)
        {
            var nomina = await _context.Nominas.FindAsync(new object?[] { id }, ct);
            if (nomina == null) return NotFound();

            _context.Nominas.Remove(nomina);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }

    // =======================
    // DTOs de entrada 
    // =======================
    public class GenerarNominaRangoDto
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? DepartamentoId { get; set; }   // si tu servicio lo usa, puedes leerlo allí
        public string? Descripcion { get; set; }
    }

    // ---------- NUEVOS DTOs para generar-v2 ----------
    public class HorasEmpleadoDto
    {
        public int EmpleadoId { get; set; }
        public decimal HorasRegulares { get; set; }
        public decimal HorasExtra { get; set; }
        public decimal TarifaHora { get; set; }
        public decimal TarifaExtra { get; set; } // si llega 0, se aplica 1.5x en el servicio
    }

    public class ComisionEmpleadoDto
    {
        public int EmpleadoId { get; set; }
        public decimal Monto { get; set; }
        public string? Observacion { get; set; }
    }

    public class ParametroLegalInlineDto
    {
        public string Clave { get; set; } = string.Empty; // "IGSS", "IRTRA", "ISR"
        public decimal Valor { get; set; } // porcentajes como 0.0483, 0.01, etc.
    }

    public class GenerarNominaV2Dto
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? Descripcion { get; set; }

        // Opcionales:
        public List<HorasEmpleadoDto>? Horas { get; set; }
        public List<ComisionEmpleadoDto>? Comisiones { get; set; }

        // Parámetros legales “inline” (opcional). Si no se envían, el servicio usa los defaults (IGSS 4.83%, IRTRA 0, ISR 0)
        public List<ParametroLegalInlineDto>? ParametrosLegales { get; set; }
    }
}
