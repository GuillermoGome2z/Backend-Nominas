using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Backend.Controllers
{
    [Authorize(Roles = "Admin,RRHH")]
    [ApiController]
    [Route("api/[controller]")]
    // Por [Authorize], documentamos 401 y 403 a nivel de clase
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class DeduccionesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DeduccionesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Deducciones
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Deduccion>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Deduccion>>> GetDeducciones()
        {
            return await _context.Deducciones.ToListAsync();
        }

        // GET: api/Deducciones/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Deduccion), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Deduccion>> GetDeduccion(int id)
        {
            var deduccion = await _context.Deducciones.FindAsync(id);
            if (deduccion == null) return NotFound();
            return deduccion;
        }

        // POST: api/Deducciones
        [HttpPost]
        [ProducesResponseType(typeof(Deduccion), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Deduccion>> PostDeduccion(Deduccion deduccion)
        {
            _context.Deducciones.Add(deduccion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDeduccion), new { id = deduccion.Id }, deduccion);
        }

        // PUT: api/Deducciones/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutDeduccion(int id, Deduccion deduccion)
        {
            if (id != deduccion.Id) return BadRequest();

            _context.Entry(deduccion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Deducciones.Any(d => d.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Deducciones/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteDeduccion(int id)
        {
            var deduccion = await _context.Deducciones.FindAsync(id);
            if (deduccion == null) return NotFound();

            _context.Deducciones.Remove(deduccion);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
