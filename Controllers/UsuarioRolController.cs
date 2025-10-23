using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class UsuarioRolController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuarioRolController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UsuarioRol
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<UsuarioRol>>> GetAsignaciones( [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
{
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;

    var baseQuery = _context.Usuarios.AsNoTracking();

    var total = await baseQuery.CountAsync();

    var usuarios = await baseQuery
        .OrderBy(u => u.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    Response.Headers["X-Total-Count"] = total.ToString();
    return Ok(usuarios);
}

        // GET: api/UsuarioRol/{usuarioId}/{rolId}
        [HttpGet("{usuarioId}/{rolId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UsuarioRol>> GetAsignacion(int usuarioId, int rolId)
        {
            var asignacion = await _context.UsuarioRoles
                .Include(ur => ur.Usuario)
                .Include(ur => ur.Rol)
                .FirstOrDefaultAsync(ur => ur.UsuarioId == usuarioId && ur.RolId == rolId);

            if (asignacion == null)
                return NotFound();

            return Ok(asignacion);
        }

        // POST: api/UsuarioRol
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<UsuarioRol>> AsignarRol([FromBody] UsuarioRol usuarioRol)
        {
            var existe = await _context.UsuarioRoles
                .AnyAsync(ur => ur.UsuarioId == usuarioRol.UsuarioId && ur.RolId == usuarioRol.RolId);

            if (existe)
                return BadRequest("El usuario ya tiene asignado este rol.");

            _context.UsuarioRoles.Add(usuarioRol);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAsignacion),
                new { usuarioId = usuarioRol.UsuarioId, rolId = usuarioRol.RolId },
                usuarioRol);
        }

        // DELETE: api/UsuarioRol/{usuarioId}/{rolId}
        [HttpDelete("{usuarioId}/{rolId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminarAsignacion(int usuarioId, int rolId)
        {
            var asignacion = await _context.UsuarioRoles
                .FirstOrDefaultAsync(ur => ur.UsuarioId == usuarioId && ur.RolId == rolId);

            if (asignacion == null)
                return NotFound();

            _context.UsuarioRoles.Remove(asignacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
