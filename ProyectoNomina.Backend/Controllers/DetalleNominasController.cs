using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Backend.Services; 
using System.Linq;

namespace ProyectoNomina.Backend.Controllers
{
    [Authorize(Roles = "Admin,RRHH")]
    [ApiController]
    [Route("api/[controller]")]
    // Por tener [Authorize], documentamos 401 y 403 a nivel de clase
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class DetalleNominasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDetalleNominaAuditService _audit; // <-- NUEVO

        public DetalleNominasController(AppDbContext context, IDetalleNominaAuditService audit) // <-- NUEVO
        {
            _context = context;
            _audit = audit; // <-- NUEVO
        }

        // GET: api/DetalleNominas
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DetalleNomina>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<DetalleNomina>>> GetDetalles(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var baseQuery = _context.DetalleNominas
                .AsNoTracking()
                .Include(d => d.Empleado)
                .Include(d => d.Nomina);

            var total = await baseQuery.CountAsync();

            var lista = await baseQuery
                .OrderBy(d => d.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(lista);
        }

        // GET: api/DetalleNominas/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DetalleNomina), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DetalleNomina>> GetDetalle(int id)
        {
            var detalle = await _context.DetalleNominas
                .Include(d => d.Empleado)
                .Include(d => d.Nomina)
                .FirstOrDefaultAsync(d => d.Id == id);

            return detalle == null ? NotFound() : detalle;
        }

        // POST: api/DetalleNominas
        [HttpPost]
        [ProducesResponseType(typeof(DetalleNomina), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DetalleNomina>> PostDetalle([FromBody] DetalleNomina detalle)
        {
            if (!_context.Empleados.Any(e => e.Id == detalle.EmpleadoId) ||
                !_context.Nominas.Any(n => n.Id == detalle.NominaId))
            {
                return BadRequest("El Empleado o la Nómina no existen.");
            }

            // Tu lógica original de cálculo
            detalle.SalarioNeto = detalle.SalarioBruto + detalle.Bonificaciones - detalle.Deducciones;

            _context.DetalleNominas.Add(detalle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDetalle), new { id = detalle.Id }, detalle);
        }

        // PUT: api/DetalleNominas/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutDetalle(int id, [FromBody] DetalleNomina detalle)
        {
            if (id != detalle.Id)
                return BadRequest();

            // Verificación de existencia (misma que ya tenías)
            if (!_context.DetalleNominas.Any(d => d.Id == id))
                return NotFound();

            // ⬇️ NUEVO: obtenemos snapshot ORIGINAL (sin tracking) para auditar difs
            var original = await _context.DetalleNominas
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
            if (original == null)
                return NotFound();

            // Tu lógica original de cálculo
            detalle.SalarioNeto = detalle.SalarioBruto + detalle.Bonificaciones - detalle.Deducciones;

            // Marcamos como modificado, igual que antes
            _context.Entry(detalle).State = EntityState.Modified;

            try
            {
                // ⬇️ NUEVO: registramos difs por campo ANTES de guardar
                await _audit.AuditarAsync(original, detalle);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Error al actualizar el detalle de nómina.");
            }

            return NoContent();
        }

        // DELETE: api/DetalleNominas/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteDetalle(int id)
        {
            var detalle = await _context.DetalleNominas.FindAsync(id);
            if (detalle == null)
                return NotFound();

            _context.DetalleNominas.Remove(detalle);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================
        // GET: /api/DetalleNominas/{id}/historial   <-- NUEVO ENDPOINT
        // ============================
        [HttpGet("{id:int}/historial")]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHistorial(int id, CancellationToken ct = default)
        {
            var existe = await _context.DetalleNominas
                .AsNoTracking()
                .AnyAsync(d => d.Id == id, ct);

            if (!existe) return NotFound($"No existe DetalleNomina con id {id}.");

            var data = await _context.Set<DetalleNominaHistorial>()
                .AsNoTracking()
                .Where(h => h.DetalleNominaId == id)
                .OrderByDescending(h => h.Fecha)
                .Select(h => new
                {
                    h.Id,
                    h.DetalleNominaId,
                    h.Campo,
                    h.ValorAnterior,
                    h.ValorNuevo,
                    h.UsuarioId,
                    h.Fecha
                })
                .ToListAsync(ct);

            return Ok(data);
        }
    }
}
