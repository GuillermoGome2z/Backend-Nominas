using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Roles (Permitir acceso anónimo para registro inicial)
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<RolDto>>> GetRoles( [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
{
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;

    var baseQuery = _context.Roles.AsNoTracking();

    var total = await baseQuery.CountAsync();

    var roles = await baseQuery
        .OrderBy(r => r.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(r => new RolDto { Id = r.Id, Nombre = r.Nombre })
        .ToListAsync();

    Response.Headers["X-Total-Count"] = total.ToString();
    return Ok(roles);
}

        // GET: api/Roles/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RolDto>> GetRol(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
                return NotFound();

            return new RolDto
            {
                Id = rol.Id,
                Nombre = rol.Nombre
            };
        }

        // POST: api/Roles
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> PostRol([FromBody] RolDto dto)
        {
            var nuevo = new Rol { Nombre = dto.Nombre };
            _context.Roles.Add(nuevo);
            await _context.SaveChangesAsync();

            // Retornamos CreatedAtAction para documentar correctamente 201
            return CreatedAtAction(nameof(GetRol), new { id = nuevo.Id }, new RolDto { Id = nuevo.Id, Nombre = nuevo.Nombre });
        }

        // PUT: api/Roles/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> PutRol(int id, [FromBody] RolDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
                return NotFound();

            rol.Nombre = dto.Nombre;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Roles/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRol(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
                return NotFound();

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
