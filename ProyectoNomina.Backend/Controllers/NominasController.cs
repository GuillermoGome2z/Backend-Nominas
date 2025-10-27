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
        // GET /api/Nominas  (paginado + filtros + 422 por rango inv�lido) ? ahora responde DTO
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
            // saneo de paginaci�n
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            // validaci�n rango fechas ? 422
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

            // Proyección a DTO completo con todos los campos calculados
            var nominas = await query
                .Include(n => n.DetallesNomina)
                .Include(n => n.AportesPatronales)
                .OrderByDescending(n => n.FechaGeneracion)
                .ThenBy(n => n.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var nominasDTO = nominas.Select(n => new
            {
                n.Id,
                n.Descripcion,
                n.FechaGeneracion,
                Periodo = n.Periodo ?? $"{n.Anio}-{n.Mes:D2}",
                n.Anio,
                n.Mes,
                n.Quincena,
                n.TipoPeriodo,
                n.TipoNomina,
                n.Estado,
                n.FechaInicio,
                n.FechaFin,
                n.FechaCorte,
                n.FechaAprobacion,
                n.FechaPago,
                
                // Totales calculados
                CantidadEmpleados = n.DetallesNomina?.Count ?? n.CantidadEmpleados,
                n.TotalBruto,
                n.TotalDeducciones,
                n.TotalBonificaciones,
                n.TotalNeto,
                n.TotalIgssEmpleado,
                n.TotalIsr,
                n.MontoTotal,
                
                // Aportes patronales
                TotalIgssPatronal = n.AportesPatronales?.TotalIgssPatronal ?? 0,
                TotalIrtra = n.AportesPatronales?.TotalIrtra ?? 0,
                TotalIntecap = n.AportesPatronales?.TotalIntecap ?? 0,
                TotalAportesPatronales = n.AportesPatronales?.TotalAportesPatronales ?? 0,
                
                // Control
                n.CreadoPor,
                n.AprobadoPor,
                n.Observaciones
            }).ToList();

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(nominasDTO);
        }

        // ============================================================
        // GET /api/Nominas/{id}
        // ============================================================
        [HttpGet("{id}", Name = "GetNominaById")]
        [ProducesResponseType(typeof(Nomina), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Nomina>> GetNomina(int id, CancellationToken ct = default)
        {
            // Obtener la nómina con aportes patronales
            var nomina = await _context.Nominas
                .Include(n => n.AportesPatronales)
                .AsNoTracking()
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
                .Include(n => n.DetallesNomina)
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
                    Detalles = n.DetallesNomina.Select(d => new DetalleNominaDto
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
        // GET /api/Nominas/listado  (DTO simple + paginado) � se mantiene por compatibilidad
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
        // GET /api/Nominas/{id}/detalles?empleadoId=123 (alias plural)
        // ============================================================
        [HttpGet("{id:int}/detalle")]
        [HttpGet("{id:int}/detalles")] // Alias plural para compatibilidad con frontend
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
        // POST /api/Nominas  (crear n�mina)
        // ============================================================
        [HttpPost]
        [ProducesResponseType(typeof(Nomina), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PostNomina([FromBody] CrearNominaDto dto, CancellationToken ct = default)
        {
            // Validar que no exista una nómina con el mismo período y tipo
            var existeNomina = await _context.Nominas
                .AnyAsync(n => n.Periodo == dto.Periodo && n.TipoNomina == dto.TipoNomina, ct);

            if (existeNomina)
            {
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Nómina duplicada",
                    Detail = $"Ya existe una nómina {dto.TipoNomina} para el período {dto.Periodo}"
                });
            }

            // Construir query base de empleados activos
            var query = _context.Empleados
                .Where(e => e.EstadoLaboral == "ACTIVO")
                .AsQueryable();

            // Aplicar filtros seg�n lo que venga
            if (dto.DepartamentoIds?.Any() == true)
            {
                query = query.Where(e => dto.DepartamentoIds.Contains(e.DepartamentoId ?? 0));
            }

            if (dto.EmpleadoIds?.Any() == true)
            {
                query = query.Where(e => dto.EmpleadoIds.Contains(e.Id));
            }

            var empleados = await query.ToListAsync(ct);

            if (!empleados.Any())
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Sin empleados",
                    Detail = "No se encontraron empleados activos con los filtros especificados"
                });
            }

            // Crear la nómina con todos los campos
            var nomina = new Nomina
            {
                Periodo = dto.Periodo,
                TipoNomina = dto.TipoNomina ?? "ORDINARIA",
                Descripcion = dto.Observaciones ?? $"Nómina {dto.TipoNomina ?? "ORDINARIA"} - {dto.Periodo}",
                Observaciones = dto.Observaciones,
                FechaGeneracion = DateTime.Now,
                FechaCorte = dto.FechaCorte ?? DateTime.Now,
                Estado = "BORRADOR",
                CantidadEmpleados = empleados.Count,
                CreadoPor = User.FindFirst("sub")?.Value ?? User.Identity?.Name,
                
                // Extraer Año y Mes del Periodo (formato "2025-10")
                Anio = !string.IsNullOrEmpty(dto.Periodo) && dto.Periodo.Length >= 4 
                    ? int.Parse(dto.Periodo.Substring(0, 4)) 
                    : (int?)null,
                Mes = !string.IsNullOrEmpty(dto.Periodo) && dto.Periodo.Length >= 7 
                    ? int.Parse(dto.Periodo.Substring(5, 2)) 
                    : (int?)null
            };

            _context.Nominas.Add(nomina);
            await _context.SaveChangesAsync(ct);

            // ============================================================
            // CALCULAR DETALLES POR CADA EMPLEADO
            // ============================================================
            foreach (var empleado in empleados)
            {
                var detalle = new DetalleNomina
                {
                    NominaId = nomina.Id,
                    EmpleadoId = empleado.Id,
                    
                    // Salario base
                    SalarioBruto = empleado.SalarioMensual,
                    
                    // Cálculo IGSS Empleado (4.83%)
                    IgssEmpleado = Math.Round(empleado.SalarioMensual * 0.0483m, 2),
                    
                    // Cálculo ISR básico (simplificado - 5% sobre salario > 5000)
                    Isr = empleado.SalarioMensual > 5000 
                        ? Math.Round((empleado.SalarioMensual - 5000) * 0.05m, 2) 
                        : 0,
                    
                    // Totales calculados
                    TotalDevengado = empleado.SalarioMensual,
                    Deducciones = 0, // Se calculará después
                    Bonificaciones = 0,
                    SalarioNeto = 0 // Se calculará después
                };
                
                // Calcular total deducciones
                detalle.TotalDeducciones = detalle.IgssEmpleado + detalle.Isr;
                detalle.Deducciones = detalle.TotalDeducciones;
                
                // Calcular salario neto
                detalle.LiquidoAPagar = detalle.TotalDevengado - detalle.TotalDeducciones;
                detalle.SalarioNeto = detalle.LiquidoAPagar;
                
                _context.DetalleNominas.Add(detalle);
            }
            
            await _context.SaveChangesAsync(ct);

            // ============================================================
            // RECALCULAR TOTALES DE LA NÓMINA
            // ============================================================
            nomina.TotalBruto = await _context.DetalleNominas
                .Where(d => d.NominaId == nomina.Id)
                .SumAsync(d => d.TotalDevengado, ct);
                
            nomina.TotalIgssEmpleado = await _context.DetalleNominas
                .Where(d => d.NominaId == nomina.Id)
                .SumAsync(d => d.IgssEmpleado, ct);
                
            nomina.TotalIsr = await _context.DetalleNominas
                .Where(d => d.NominaId == nomina.Id)
                .SumAsync(d => d.Isr, ct);
                
            nomina.TotalDeducciones = await _context.DetalleNominas
                .Where(d => d.NominaId == nomina.Id)
                .SumAsync(d => d.TotalDeducciones, ct);
                
            nomina.TotalNeto = nomina.TotalBruto - nomina.TotalDeducciones;
            nomina.MontoTotal = nomina.TotalNeto;
            
            // Calcular aportes patronales
            var aportesPatronales = new NominaAportesPatronales
            {
                NominaId = nomina.Id,
                TotalIgssPatronal = Math.Round(nomina.TotalBruto * 0.1067m, 2), // 10.67%
                TotalIrtra = Math.Round(nomina.TotalBruto * 0.01m, 2), // 1%
                TotalIntecap = Math.Round(nomina.TotalBruto * 0.01m, 2), // 1%
                CalculadoEn = DateTime.UtcNow,
                CalculadoPor = User.FindFirst("sub")?.Value ?? User.Identity?.Name
            };
            
            aportesPatronales.TotalAportesPatronales = 
                aportesPatronales.TotalIgssPatronal +
                aportesPatronales.TotalIrtra +
                aportesPatronales.TotalIntecap;
            
            _context.NominaAportesPatronales.Add(aportesPatronales);
            
            await _context.SaveChangesAsync(ct);

            // Cargar la nómina completa con sus relaciones
            var nominaCompleta = await _context.Nominas
                .Include(n => n.AportesPatronales)
                .FirstOrDefaultAsync(n => n.Id == nomina.Id, ct);

            return CreatedAtRoute("GetNominaById", new { id = nomina.Id }, nominaCompleta);
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
            // Validaciones de rango ? 422
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
                    ? $"N�mina {inicio:yyyy-MM-dd} a {fin:yyyy-MM-dd}"
                    : dto.Descripcion!.Trim(),
                FechaGeneracion = DateTime.Now
            };

            _context.Nominas.Add(nomina);
            await _context.SaveChangesAsync(ct);

            // Se mantiene tu l�gica actual (retrocompatible)
            await _nominaService.Calcular(nomina);
            await _context.SaveChangesAsync(ct);

            return CreatedAtRoute("GetNominaById", new { id = nomina.Id }, new
            {
                mensaje = "? N�mina generada y calculada correctamente.",
                nominaId = nomina.Id
            });
        }

        // ============================================================
        // POST /api/Nominas/generar-v2  (crear + calcular V2 con horas/comisiones/par�metros)
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

            // Validaciones b�sicas de horas/comisiones
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
                            [$"empleado:{c.EmpleadoId}"] = new[] { "La comisi�n no puede ser negativa." }
                        }));
                }
            }

            var nomina = new Nomina
            {
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion)
                    ? $"N�mina {inicio:yyyy-MM-dd} a {fin:yyyy-MM-dd}"
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

            // Resolver de par�metros legales (inline opcional)
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

            // Calcular con V2 (si no hay resolver/horas/comisiones, hace fallback y queda como tu versi�n actual)
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
                mensaje = "? N�mina (V2) generada y calculada correctamente.",
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
                .Include(n => n.DetallesNomina)
                .FirstOrDefaultAsync(n => n.Id == id, ct);

            if (nomina == null)
                return NotFound("? N�mina no encontrada.");

            // Limpiar detalles anteriores si existen
            _context.DetalleNominas.RemoveRange(nomina.DetallesNomina);
            await _context.SaveChangesAsync(ct);

            await _nominaService.Calcular(nomina);
            await _context.SaveChangesAsync(ct);

            return Ok(new
            {
                mensaje = "? N�mina procesada correctamente.",
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
                    .Include(n => n.DetallesNomina)
                        .ThenInclude(d => d.Empleado)
                    .FirstOrDefaultAsync(n => n.Id == id, ct);

                if (nomina == null)
                    return NotFound("No se encontr� la n�mina.");

                var pdfBytes = reporteService.GenerarReporteNominaPdf(nomina);
                return File(pdfBytes, "application/pdf", $"Nomina_{id}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"?? Error al generar PDF: {ex.Message}");
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
                .Include(n => n.DetallesNomina)
                    .ThenInclude(d => d.Empleado)
                .FirstOrDefaultAsync(n => n.Id == id, ct);

            if (nomina == null)
                return NotFound("N�mina no encontrada.");

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

        // ============================================================
        // POST /api/nominas/calcular - Calcular preview de n�mina para empleados seg�n filtros
        // ============================================================
        [HttpPost("calcular")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CalcularNominas([FromBody] NominaCalculoRequest request, CancellationToken ct = default)
        {
            try
            {
                // 1. Validar que el per�odo sea v�lido
                if (string.IsNullOrWhiteSpace(request.Periodo))
                    return BadRequest(new { message = "El per�odo es requerido" });

                // 2. Construir query base de empleados activos
                var query = _context.Empleados
                    .Where(e => e.EstadoLaboral == "ACTIVO")
                    .AsQueryable();

                // 3. Aplicar filtros seg�n lo que venga
                if (request.DepartamentoIds?.Any() == true)
                {
                    query = query.Where(e => request.DepartamentoIds.Contains(e.DepartamentoId ?? 0));
                }

                if (request.EmpleadoIds?.Any() == true)
                {
                    query = query.Where(e => request.EmpleadoIds.Contains(e.Id));
                }

                // 4. Obtener empleados con sus relaciones
                var empleados = await query
                    .Include(e => e.Departamento)
                    .Include(e => e.Puesto)
                    .ToListAsync(ct);

                // 5. Validar que haya empleados
                if (!empleados.Any())
                    return BadRequest(new { message = "No se encontraron empleados activos con los filtros especificados" });

                // 6. Calcular deducciones y totales
                decimal totalBruto = 0;
                decimal totalDeducciones = 0;
                decimal totalNeto = 0;

                var detallesPorDepartamento = empleados
                    .GroupBy(e => e.Departamento?.Nombre ?? "Sin Departamento")
                    .Select(g => new
                    {
                        Departamento = g.Key,
                        Empleados = g.Count(),
                        TotalBruto = g.Sum(e => e.SalarioMensual),
                        TotalDeducciones = Math.Round(g.Sum(e => e.SalarioMensual) * 0.1283m, 2), // IGSS 4.83% + ISR promedio 8%
                        TotalNeto = Math.Round(g.Sum(e => e.SalarioMensual) * 0.8717m, 2) // 87.17% neto
                    })
                    .OrderByDescending(d => d.TotalBruto)
                    .ToList();

                totalBruto = empleados.Sum(e => e.SalarioMensual);
                totalDeducciones = Math.Round(totalBruto * 0.1283m, 2); // 12.83% promedio (IGSS + ISR b�sico)
                totalNeto = totalBruto - totalDeducciones;

                // 7. Devolver respuesta
                return Ok(new
                {
                    Periodo = request.Periodo,
                    TipoNomina = request.TipoNomina ?? "ORDINARIA",
                    TotalEmpleados = empleados.Count,
                    TotalBruto = totalBruto,
                    TotalDeducciones = totalDeducciones,
                    TotalNeto = totalNeto,
                    DetallesPorDepartamento = detallesPorDepartamento,
                    FechaCalculo = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al calcular n�mina", error = ex.Message });
            }
        }

        // ============================================================
        // GET /api/nominas/calcular - Calcular n�minas (versi�n GET para frontend)
        // ============================================================
        [HttpGet("calcular")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CalcularNominasGet([FromQuery] string? periodo = null, [FromQuery] string? estado = null, CancellationToken ct = default)
        {
            try
            {
                var query = _context.Nominas.AsQueryable();
                
                if (!string.IsNullOrEmpty(periodo))
                {
                    query = query.Where(n => n.Periodo == periodo);
                }
                
                if (!string.IsNullOrEmpty(estado))
                {
                    query = query.Where(n => n.Estado == estado);
                }
                else
                {
                    // Por defecto, calcular n�minas en BORRADOR
                    query = query.Where(n => n.Estado == "BORRADOR");
                }

                var nominas = await query.ToListAsync(ct);
                
                if (!nominas.Any())
                {
                    return Ok(new { 
                        message = "No se encontraron n�minas para calcular",
                        resultados = new object[0],
                        totalCalculadas = 0
                    });
                }

                var resultados = new List<object>();
                foreach (var nomina in nominas)
                {
                    await _nominaService.Calcular(nomina);
                    resultados.Add(new { 
                        NominaId = nomina.Id,
                        Periodo = nomina.Periodo,
                        TotalEmpleados = nomina.DetallesNomina?.Count ?? 0,
                        MontoTotal = nomina.MontoTotal,
                        Estado = "Calculado"
                    });
                }

                await _context.SaveChangesAsync(ct);
                
                return Ok(new { 
                    message = "N�minas calculadas exitosamente",
                    resultados = resultados,
                    totalCalculadas = resultados.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al calcular n�minas", error = ex.Message });
            }
        }

        // ============================================================
        // PUT /api/nominas/{id}/aprobar - Aprobar nómina
        // ============================================================
        [HttpPut("{id}/aprobar")]
        [ProducesResponseType(typeof(Nomina), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Nomina>> AprobarNomina(
            int id, 
            [FromBody] AprobarNominaDto dto,
            CancellationToken ct = default)
        {
            var nomina = await _context.Nominas.FindAsync(new object?[] { id }, ct);
            if (nomina == null)
                return NotFound(new { message = "Nómina no encontrada" });

            // Validar que esté en estado BORRADOR
            if (nomina.Estado != "BORRADOR")
                return BadRequest(new { message = "Solo se pueden aprobar nóminas en estado BORRADOR" });

            // Actualizar estado
            nomina.Estado = "APROBADA";
            nomina.FechaAprobacion = DateTime.Now;
            
            // Actualizar observaciones si se proporcionan
            if (!string.IsNullOrWhiteSpace(dto.Observaciones))
            {
                nomina.Observaciones = dto.Observaciones;
            }
            
            // Guardar usuario que aprobó
            var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
            nomina.AprobadoPor = userId;
            
            await _context.SaveChangesAsync(ct);

            // Devolver la nómina completa actualizada
            return Ok(nomina);
        }

        // ============================================================
        // PUT /api/nominas/{id}/pagar - Marcar n�mina como pagada
        // ============================================================
        [HttpPut("{id}/pagar")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PagarNomina(int id, CancellationToken ct = default)
        {
            var nomina = await _context.Nominas.FindAsync(new object?[] { id }, ct);
            if (nomina == null)
                return NotFound(new { message = "N�mina no encontrada" });

            if (nomina.Estado != "APROBADA")
                return BadRequest(new { message = "Solo se pueden pagar n�minas en estado APROBADA" });

            nomina.Estado = "PAGADA";
            nomina.FechaPago = DateTime.UtcNow;
            
            await _context.SaveChangesAsync(ct);

            return Ok(new { 
                message = "N�mina marcada como pagada exitosamente",
                nominaId = id,
                estado = nomina.Estado,
                fechaPago = nomina.FechaPago
            });
        }

        // ============================================================
        // PUT /api/nominas/{id}/anular - Anular n�mina
        // ============================================================
        [HttpPut("{id}/anular")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AnularNomina(int id, [FromBody] AnularNominaDto dto, CancellationToken ct = default)
        {
            var nomina = await _context.Nominas.FindAsync(new object?[] { id }, ct);
            if (nomina == null)
                return NotFound(new { message = "N�mina no encontrada" });

            if (nomina.Estado == "ANULADA")
                return BadRequest(new { message = "La n�mina ya est� anulada" });

            if (nomina.Estado == "PAGADA")
                return BadRequest(new { message = "No se puede anular una n�mina que ya ha sido pagada" });

            nomina.Estado = "ANULADA";
            nomina.FechaAnulacion = DateTime.UtcNow;
            nomina.MotivoAnulacion = dto.Motivo ?? "Sin motivo especificado";
            
            await _context.SaveChangesAsync(ct);

            return Ok(new { 
                message = "N�mina anulada exitosamente",
                nominaId = id,
                estado = nomina.Estado,
                fechaAnulacion = nomina.FechaAnulacion,
                motivo = nomina.MotivoAnulacion
            });
        }

        // ============================================================
        // GET /api/nominas/stats - Estad�sticas de n�minas
        // ============================================================
        [HttpGet("stats")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEstadisticasNominas([FromQuery] string? periodo = null, CancellationToken ct = default)
        {
            try
            {
                var query = _context.Nominas.AsQueryable();
                
                if (!string.IsNullOrEmpty(periodo))
                {
                    query = query.Where(n => n.Periodo == periodo);
                }

                var totalNominas = await query.CountAsync(ct);
                
                if (totalNominas == 0)
                {
                    return Ok(new {
                        periodo = periodo ?? "Todos los per�odos",
                        resumen = new {
                            totalNominas = 0,
                            montoGlobalTotal = 0m,
                            empleadosConNomina = 0
                        },
                        estadisticasPorEstado = new object[0],
                        fechaConsulta = DateTime.UtcNow
                    });
                }

                var estadisticas = await query
                    .GroupBy(n => n.Estado ?? "SIN_ESTADO")
                    .Select(g => new {
                        Estado = g.Key,
                        Cantidad = g.Count(),
                        MontoTotal = g.Sum(n => n.MontoTotal)
                    })
                    .ToListAsync(ct);

                var montoGlobalTotal = estadisticas.Sum(e => e.MontoTotal);
                
                var empleadosConNomina = 0;
                try 
                {
                    empleadosConNomina = await _context.DetalleNominas
                        .Where(d => query.Any(n => n.Id == d.NominaId))
                        .Select(d => d.EmpleadoId)
                        .Distinct()
                        .CountAsync(ct);
                }
                catch
                {
                    // Si falla la consulta de empleados, continuar con 0
                }

                return Ok(new {
                    periodo = periodo ?? "Todos los per�odos",
                    resumen = new {
                        totalNominas = totalNominas,
                        montoGlobalTotal = montoGlobalTotal,
                        empleadosConNomina = empleadosConNomina
                    },
                    estadisticasPorEstado = estadisticas,
                    fechaConsulta = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estad�sticas", error = ex.Message });
            }
        }

        // ============================================================
        // GET /api/nominas/{id}/export/{formato} - Exportar n�mina
        // ============================================================
        [HttpGet("{id}/export/{formato}")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportarNomina(int id, string formato, CancellationToken ct = default)
        {
            formato = formato.ToLowerInvariant();
            
            if (formato != "pdf" && formato != "excel")
                return BadRequest(new { message = "Formato no soportado. Use 'pdf' o 'excel'" });

            var nomina = await _context.Nominas
                .Include(n => n.DetallesNomina)
                    .ThenInclude(d => d.Empleado)
                .FirstOrDefaultAsync(n => n.Id == id, ct);

            if (nomina == null)
                return NotFound(new { message = "N�mina no encontrada" });

            try
            {
                if (formato == "pdf")
                {
                    var pdfBytes = await _nominaService.GenerarPdfAsync(nomina);
                    var fileName = $"nomina_{nomina.Periodo}_{nomina.Id}.pdf";
                    return File(pdfBytes, "application/pdf", fileName);
                }
                else // excel
                {
                    var excelBytes = await _nominaService.GenerarExcelAsync(nomina);
                    var fileName = $"nomina_{nomina.Periodo}_{nomina.Id}.xlsx";
                    return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al generar {formato.ToUpper()}", error = ex.Message });
            }
        }

        // ============================================================
        // POST /api/nominas/{id}/enviar-email - Enviar n�mina por email
        // ============================================================
        [HttpPost("{id}/enviar-email")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EnviarNominaPorEmail(int id, [FromBody] EnviarEmailDto dto, CancellationToken ct = default)
        {
            var nomina = await _context.Nominas
                .Include(n => n.DetallesNomina)
                    .ThenInclude(d => d.Empleado)
                .FirstOrDefaultAsync(n => n.Id == id, ct);

            if (nomina == null)
                return NotFound(new { message = "N�mina no encontrada" });

            if (nomina.Estado != "APROBADA" && nomina.Estado != "PAGADA")
                return BadRequest(new { message = "Solo se pueden enviar n�minas aprobadas o pagadas" });

            try
            {
                var resultados = new List<object>();

                if (dto.EnviarATodos)
                {
                    // Enviar a todos los empleados de la n�mina
                    foreach (var detalle in nomina.DetallesNomina)
                    {
                        if (!string.IsNullOrEmpty(detalle.Empleado.Correo))
                        {
                            var enviado = await _nominaService.EnviarNominaPorEmailAsync(
                                nomina, 
                                detalle.Empleado.Correo, 
                                dto.Formato ?? "pdf",
                                dto.Mensaje
                            );

                            resultados.Add(new {
                                EmpleadoId = detalle.EmpleadoId,
                                Nombre = detalle.Empleado.NombreCompleto,
                                Email = detalle.Empleado.Correo,
                                Enviado = enviado
                            });
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(dto.Email))
                {
                    // Enviar a email espec�fico
                    var enviado = await _nominaService.EnviarNominaPorEmailAsync(
                        nomina, 
                        dto.Email, 
                        dto.Formato ?? "pdf",
                        dto.Mensaje
                    );

                    resultados.Add(new {
                        Email = dto.Email,
                        Enviado = enviado
                    });
                }
                else
                {
                    return BadRequest(new { message = "Debe especificar un email o marcar enviarATodos como true" });
                }

                return Ok(new {
                    message = "Proceso de env�o completado",
                    nominaId = id,
                    resultados = resultados,
                    totalEnviados = resultados.Count(r => (bool)r.GetType().GetProperty("Enviado")?.GetValue(r)!)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al enviar emails", error = ex.Message });
            }
        }
    }

    // =======================
    // DTOs de entrada 
    // =======================
    public class GenerarNominaRangoDto
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? DepartamentoId { get; set; }   // si tu servicio lo usa, puedes leerlo all�
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

        // Par�metros legales "inline" (opcional). Si no se env�an, el servicio usa los defaults (IGSS 4.83%, IRTRA 0, ISR 0)
        public List<ParametroLegalInlineDto>? ParametrosLegales { get; set; }
    }

    public class NominaCalculoRequest
    {
        public string Periodo { get; set; } = string.Empty;
        public string? TipoNomina { get; set; }
        public List<int> DepartamentoIds { get; set; } = new();
        public List<int> EmpleadoIds { get; set; } = new();
    }

    public class CalcularNominasDto
    {
        public List<int>? NominaIds { get; set; }
        public string? Periodo { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }

    public class AnularNominaDto
    {
        public string? Motivo { get; set; }
    }

    public class EnviarEmailDto
    {
        public string? Email { get; set; }
        public bool EnviarATodos { get; set; } = false;
        public string? Formato { get; set; } = "pdf"; // "pdf" o "excel"
        public string? Mensaje { get; set; }
    }
}
