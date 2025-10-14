using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioRolController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuarioRolController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UsuarioRol
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioRol>>> GetAsignaciones()
        {
            return await _context.UsuarioRoles
                .Include(ur => ur.Usuario)
                .Include(ur => ur.Rol)
                .ToListAsync();
        }

        // GET: api/UsuarioRol/5
        [HttpGet("{usuarioId}/{rolId}")]
        public async Task<ActionResult<UsuarioRol>> GetAsignacion(int usuarioId, int rolId)
        {
            var asignacion = await _context.UsuarioRoles
                .Include(ur => ur.Usuario)
                .Include(ur => ur.Rol)
                .FirstOrDefaultAsync(ur => ur.UsuarioId == usuarioId && ur.RolId == rolId);

            return asignacion == null ? NotFound() : asignacion;
        }

        // POST: api/UsuarioRol
        [HttpPost]
        public async Task<ActionResult<UsuarioRol>> AsignarRol(UsuarioRol usuarioRol)
        {
            var existe = await _context.UsuarioRoles
                .AnyAsync(ur => ur.UsuarioId == usuarioRol.UsuarioId && ur.RolId == usuarioRol.RolId);

            if (existe)
                return BadRequest("El usuario ya tiene asignado este rol.");

            _context.UsuarioRoles.Add(usuarioRol);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAsignacion), new { usuarioId = usuarioRol.UsuarioId, rolId = usuarioRol.RolId }, usuarioRol);
        }

        // DELETE: api/UsuarioRol/5/3
        [HttpDelete("{usuarioId}/{rolId}")]
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
