using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Backend.Services;
using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Backend.Controllers
{
    [Authorize(Roles = "Admin,RRHH")]
    [ApiController]
    [Route("api/[controller]")]
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

        // Obtener todas las nóminas con sus detalles (paginado)
       [HttpGet]
[ProducesResponseType(typeof(IEnumerable<Nomina>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // ← NUEVO
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<IEnumerable<Nomina>>> GetNominas(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] int? departamentoId = null,      // ← NUEVO
    [FromQuery] DateTime? fechaInicio = null,    // ← NUEVO
    [FromQuery] DateTime? fechaFin = null)       // ← NUEVO
{
    // saneo de paginación (igual)
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;

    // validación rango fechas → 422 (punto 9)
    if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
    {
        return UnprocessableEntity(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            ["rangoFechas"] = new[] { "fechaInicio no puede ser mayor que fechaFin." }
        }));
    }

    // base query
    var query = _context.Nominas
        .AsNoTracking()
        .Include(n => n.Detalles)
            .ThenInclude(d => d.Empleado)
        .AsQueryable();

    // filtros del punto 9
    if (fechaInicio.HasValue)
        query = query.Where(n => n.FechaGeneracion >= fechaInicio.Value.Date);

    if (fechaFin.HasValue)
        query = query.Where(n => n.FechaGeneracion <= fechaFin.Value.Date);

    if (departamentoId.HasValue)
        query = query.Where(n => n.Detalles.Any(d => d.Empleado.DepartamentoId == departamentoId.Value));

    // total después de aplicar filtros
    var total = await query.CountAsync();

    // orden + paginación (igual que tenías)
    var nominas = await query
        .OrderByDescending(n => n.FechaGeneracion)
        .ThenBy(n => n.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    Response.Headers["X-Total-Count"] = total.ToString();
    return Ok(nominas);
}

        // NUEVO: Obtener una nómina por Id (necesario para CreatedAtRoute)
        [HttpGet("{id}", Name = "GetNominaById")]
        [ProducesResponseType(typeof(Nomina), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Nomina>> GetNomina(int id)
        {
            var nomina = await _context.Nominas
                .AsNoTracking()
                .Include(n => n.Detalles)
                .ThenInclude(d => d.Empleado)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nomina == null) return NotFound();

            return Ok(nomina);
        }

        // Obtener DTO con detalles incluidos (paginado)
        [HttpGet("completa")]
        [ProducesResponseType(typeof(IEnumerable<NominaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<NominaDto>>> ObtenerNominasCompletas(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var baseQuery = _context.Nominas
                .AsNoTracking()
                .Include(n => n.Detalles)
                .ThenInclude(d => d.Empleado);

            var total = await baseQuery.CountAsync();

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
                .ToListAsync();

            Response.Headers["X-Total-Count"] = total.ToString();

            return Ok(resultado);
        }

        // Listado simple (paginado)
        [HttpGet("listado")]
        [ProducesResponseType(typeof(IEnumerable<NominaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerNominas(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var baseQuery = _context.Nominas.AsNoTracking();

            var total = await baseQuery.CountAsync();

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
                .ToListAsync();

            Response.Headers["X-Total-Count"] = total.ToString();

            return Ok(nominas);
        }

        // GET: /api/nominas/{id}/detalle?empleadoId=123   (empleadoId es opcional)
[HttpGet("{id:int}/detalle")]
[ProducesResponseType(typeof(NominaDetalleDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public async Task<ActionResult<NominaDetalleDto>> GetDetalle(int id, [FromQuery] int? empleadoId = null)
{
    // 404 si no existe la nómina
    var nomina = await _context.Nominas
        .AsNoTracking()
        .FirstOrDefaultAsync(n => n.Id == id);

    if (nomina == null)
        return NotFound();

    // Query base de detalles (con Empleado para el nombre)
    var qDetalles = _context.DetalleNominas
        .AsNoTracking()
        .Where(d => d.NominaId == id)
        .Include(d => d.Empleado)
        .AsQueryable();

    // Filtro opcional por empleado
    if (empleadoId.HasValue)
        qDetalles = qDetalles.Where(d => d.EmpleadoId == empleadoId.Value);

    // Proyección a tu DTO existente (DetalleNominaDto)
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
        .ToListAsync();

    // Totales (del conjunto actual: toda la nómina o filtrado por empleado)
    var totalBruto = await qDetalles.SumAsync(d => (decimal?)d.SalarioBruto) ?? 0m;
    var totalDeducciones = await qDetalles.SumAsync(d => (decimal?)d.Deducciones) ?? 0m;
    var totalBonificaciones = await qDetalles.SumAsync(d => (decimal?)d.Bonificaciones) ?? 0m;
    var totalNeto = await qDetalles.SumAsync(d => (decimal?)d.SalarioNeto) ?? 0m;

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

        // Crear una nueva nómina (vacía, sin detalles)
        [HttpPost]
        [ProducesResponseType(typeof(Nomina), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PostNomina([FromBody] CrearNominaDto dto)
        {
            var nomina = new Nomina
            {
                Descripcion = dto.Descripcion,
                FechaGeneracion = DateTime.Now
            };

            _context.Nominas.Add(nomina);
            await _context.SaveChangesAsync();

            // CAMBIO: usar la ruta nombrada del GET-by-id
            return CreatedAtRoute("GetNominaById", new { id = nomina.Id }, nomina);
        }

        // Procesar y calcular automáticamente una nómina
        [HttpPost("procesar/{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcesarNominaExistente(int id)
        {
            var nomina = await _context.Nominas
                .Include(n => n.Detalles)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nomina == null)
                return NotFound("❌ Nómina no encontrada.");

            // Limpiar detalles anteriores si existen
            _context.DetalleNominas.RemoveRange(nomina.Detalles);

            await _nominaService.Calcular(nomina);

            return Ok(new
            {
                mensaje = "✅ Nómina procesada correctamente.",
                nominaId = nomina.Id
            });
        }

        [HttpGet("GenerarPdf/{id}")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerarPdf(int id, [FromServices] ReporteService reporteService)
        {
            try
            {
                var nomina = await _context.Nominas
                    .Include(n => n.Detalles)
                    .ThenInclude(d => d.Empleado)
                    .FirstOrDefaultAsync(n => n.Id == id);

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

        [HttpGet("GenerarExcel/{id}")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerarExcel(int id, [FromServices] ReporteService reporteService)
        {
            var nomina = await _context.Nominas
                .Include(n => n.Detalles)
                .ThenInclude(d => d.Empleado)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nomina == null)
                return NotFound("Nómina no encontrada.");

            var excelBytes = reporteService.GenerarReporteNominaExcel(nomina);

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Nomina_{id}.xlsx");
        }

        // Editar una nómina existente
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutNomina(int id, [FromBody] Nomina nomina)
        {
            if (id != nomina.Id) return BadRequest();

            _context.Entry(nomina).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Nominas.Any(n => n.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // Eliminar una nómina por ID
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteNomina(int id)
        {
            var nomina = await _context.Nominas.FindAsync(id);
            if (nomina == null) return NotFound();

            _context.Nominas.Remove(nomina);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
