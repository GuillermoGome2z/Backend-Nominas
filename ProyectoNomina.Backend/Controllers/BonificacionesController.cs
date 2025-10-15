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
    // Por tener [Authorize], documentamos 401 y 403 a nivel de clase
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class BonificacionesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BonificacionesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Bonificaciones
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Bonificacion>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Bonificacion>>> GetBonificaciones(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
{
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;

    var baseQuery = _context.Bonificaciones.AsNoTracking();

    var total = await baseQuery.CountAsync();

    var bonos = await baseQuery
        .OrderBy(b => b.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    Response.Headers["X-Total-Count"] = total.ToString();
    return Ok(bonos);
}

        // GET: api/Bonificaciones/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Bonificacion), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Bonificacion>> GetBonificacion(int id)
        {
            var bono = await _context.Bonificaciones.FindAsync(id);
            if (bono == null) return NotFound();
            return bono;
        }

        // POST: api/Bonificaciones
        [HttpPost]
        [ProducesResponseType(typeof(Bonificacion), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Bonificacion>> PostBonificacion(Bonificacion bono)
        {
            _context.Bonificaciones.Add(bono);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBonificacion), new { id = bono.Id }, bono);
        }

        // PUT: api/Bonificaciones/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutBonificacion(int id, Bonificacion bono)
        {
            if (id != bono.Id) return BadRequest();

            _context.Entry(bono).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Bonificaciones.Any(b => b.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Bonificaciones/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteBonificacion(int id)
        {
            var bono = await _context.Bonificaciones.FindAsync(id);
            if (bono == null) return NotFound();

            _context.Bonificaciones.Remove(bono);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
