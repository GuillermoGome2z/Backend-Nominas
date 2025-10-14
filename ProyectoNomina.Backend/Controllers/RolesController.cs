using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/Roles (Permitir acceso anónimo para registro inicial)
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RolDto>>> GetRoles()
        {
            var roles = await _context.Roles
                .Select(r => new RolDto
                {
                    Id = r.Id,
                    Nombre = r.Nombre
                })
                .ToListAsync();

            return Ok(roles);
        }

        // GET: api/Roles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RolDto>> GetRol(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null) return NotFound();

            return new RolDto
            {
                Id = rol.Id,
                Nombre = rol.Nombre
            };
        }

        // POST: api/Roles
        [HttpPost]
        public async Task<IActionResult> PostRol([FromBody] RolDto dto)
        {
            var nuevo = new Rol { Nombre = dto.Nombre };
            _context.Roles.Add(nuevo);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Roles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRol(int id, [FromBody] RolDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var rol = await _context.Roles.FindAsync(id);
            if (rol == null) return NotFound();

            rol.Nombre = dto.Nombre;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Roles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRol(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null) return NotFound();

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
