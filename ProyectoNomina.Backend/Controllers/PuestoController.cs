using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class PuestosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PuestosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Puestos
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<PuestoDto>>> GetPuestos(
            [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10
        )
       {
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;

    var baseQuery = _context.Puestos.AsNoTracking();

    var total = await baseQuery.CountAsync();

    var puestos = await baseQuery
        .OrderBy(p => p.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new PuestoDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            SalarioBase = p.SalarioBase
        })
        .ToListAsync();

    Response.Headers["X-Total-Count"] = total.ToString();
    return Ok(puestos);
}

        // GET: api/Puestos/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PuestoDto>> GetPuesto(int id)
        {
            var puesto = await _context.Puestos.FindAsync(id);

            if (puesto == null)
                return NotFound();

            var dto = new PuestoDto
            {
                Id = puesto.Id,
                Nombre = puesto.Nombre,
                SalarioBase = puesto.SalarioBase
            };

            return Ok(dto);
        }

        // POST: api/Puestos
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<PuestoDto>> PostPuesto([FromBody] PuestoDto dto)
        {
            var puesto = new Puesto
            {
                Nombre = dto.Nombre,
                SalarioBase = dto.SalarioBase
            };

            _context.Puestos.Add(puesto);
            await _context.SaveChangesAsync();

            dto.Id = puesto.Id;

            return CreatedAtAction(nameof(GetPuesto), new { id = dto.Id }, dto);
        }

        // PUT: api/Puestos/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)] // si en algún momento devuelves contenido
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> PutPuesto(int id, [FromBody] PuestoDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var puesto = await _context.Puestos.FindAsync(id);
            if (puesto == null)
                return NotFound();

            puesto.Nombre = dto.Nombre;
            puesto.SalarioBase = dto.SalarioBase;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Puestos/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePuesto(int id)
        {
            var puesto = await _context.Puestos.FindAsync(id);
            if (puesto == null)
                return NotFound();

            _context.Puestos.Remove(puesto);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
