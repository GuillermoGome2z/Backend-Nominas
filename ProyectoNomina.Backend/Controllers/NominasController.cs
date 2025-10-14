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

        // ✅ Obtener todas las nóminas con sus detalles
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Nomina>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Nomina>>> GetNominas()
        {
            return await _context.Nominas
                .Include(n => n.Detalles)
                .ThenInclude(d => d.Empleado)
                .ToListAsync();
        }

        // ✅ Obtener una nómina específica por ID
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Nomina), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Nomina>> GetNomina(int id)
        {
            var nomina = await _context.Nominas
                .Include(n => n.Detalles)
                .ThenInclude(d => d.Empleado)
                .FirstOrDefaultAsync(n => n.Id == id);

            return nomina == null ? NotFound() : nomina;
        }

        // ✅ Obtener DTO con detalles incluidos para visualización completa
        [HttpGet("completa")]
        [ProducesResponseType(typeof(IEnumerable<NominaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<NominaDto>>> ObtenerNominasCompletas()
        {
            var nominas = await _context.Nominas
                .Include(n => n.Detalles)
                .ThenInclude(d => d.Empleado)
                .OrderByDescending(n => n.FechaGeneracion)
                .ToListAsync();

            var resultado = nominas.Select(n => new NominaDto
            {
                Id = n.Id,
                Descripcion = n.Descripcion,
                FechaGeneracion = n.FechaGeneracion,
                Detalles = n.Detalles.Select(d => new DetalleNominaDto
                {
                    EmpleadoId = d.EmpleadoId,
                    NombreEmpleado = d.Empleado?.NombreCompleto ?? "",
                    SalarioBruto = d.SalarioBruto,
                    Deducciones = d.Deducciones,
                    Bonificaciones = d.Bonificaciones,
                    SalarioNeto = d.SalarioNeto,
                    DesgloseDeducciones = d.DesgloseDeducciones
                }).ToList()
            });

            return Ok(resultado);
        }

        [HttpGet("listado")]
        [ProducesResponseType(typeof(IEnumerable<NominaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerNominas()
        {
            var nominas = await _context.Nominas
                .OrderByDescending(n => n.FechaGeneracion)
                .Select(n => new NominaDto
                {
                    Id = n.Id,
                    Descripcion = n.Descripcion,
                    FechaGeneracion = n.FechaGeneracion
                })
                .ToListAsync();

            return Ok(nominas);
        }

        // ✅ Crear una nueva nómina (vacía, sin detalles)
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

            return CreatedAtAction(nameof(GetNomina), new { id = nomina.Id }, nomina);
        }

        // ✅ Procesar y calcular automáticamente una nómina
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

        // ✅ Editar una nómina existente
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

        // ✅ Eliminar una nómina por ID
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
