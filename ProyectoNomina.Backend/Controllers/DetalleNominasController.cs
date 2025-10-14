using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;

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

        public DetalleNominasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DetalleNominas
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DetalleNomina>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<DetalleNomina>>> GetDetalles()
        {
            return await _context.DetalleNominas
                .Include(d => d.Empleado)
                .Include(d => d.Nomina)
                .ToListAsync();
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

            if (!_context.DetalleNominas.Any(d => d.Id == id))
                return NotFound();

            detalle.SalarioNeto = detalle.SalarioBruto + detalle.Bonificaciones - detalle.Deducciones;

            _context.Entry(detalle).State = EntityState.Modified;

            try
            {
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
    }
}
