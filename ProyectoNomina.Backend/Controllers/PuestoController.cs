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
    [Produces("application/json")]
    public class PuestosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PuestosController(AppDbContext context) => _context = context;

        // GET: api/Puestos
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PuestoDto>>> GetPuestos(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? activo = null,
            [FromQuery] int? departamentoId = null)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

            var q = _context.Puestos.AsNoTracking();
            if (activo.HasValue) q = q.Where(p => p.Activo == activo.Value);
            if (departamentoId.HasValue) q = q.Where(p => p.DepartamentoId == departamentoId.Value);

            var total = await q.CountAsync();

            var data = await q
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PuestoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    SalarioBase = p.SalarioBase,
                    Activo = p.Activo,
                    DepartamentoId = p.DepartamentoId
                })
                .ToListAsync();

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(data);
        }

        // GET: api/Puestos/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PuestoDto>> GetPuesto(int id)
        {
            var p = await _context.Puestos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();

            return Ok(new PuestoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                SalarioBase = p.SalarioBase,
                Activo = p.Activo,
                DepartamentoId = p.DepartamentoId
            });
        }

        // POST: api/Puestos
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PuestoDto>> PostPuesto([FromBody] PuestoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new { mensaje = "El nombre del puesto es obligatorio." });

            if (dto.DepartamentoId.HasValue)
            {
                var dep = await _context.Departamentos.FindAsync(dto.DepartamentoId.Value);
                if (dep == null) return BadRequest(new { mensaje = "Departamento no válido." });
                if (!dep.Activo) return BadRequest(new { mensaje = "El departamento está INACTIVO." });
            }

            var p = new Puesto
            {
                Nombre = dto.Nombre,
                SalarioBase = dto.SalarioBase,
                Activo = dto.Activo,
                DepartamentoId = dto.DepartamentoId
            };

            _context.Puestos.Add(p);
            await _context.SaveChangesAsync();

            dto.Id = p.Id;
            dto.Activo = p.Activo;
            dto.DepartamentoId = p.DepartamentoId;

            return CreatedAtAction(nameof(GetPuesto), new { id = dto.Id }, dto);
        }

        // PUT: api/Puestos/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PutPuesto(int id, [FromBody] PuestoDto dto)
        {
            if (id != dto.Id) return BadRequest(new { mensaje = "ID de puesto no válido." });

            var p = await _context.Puestos.FindAsync(id);
            if (p == null) return NotFound();

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new { mensaje = "El nombre del puesto es obligatorio." });

            if (dto.DepartamentoId.HasValue)
            {
                var dep = await _context.Departamentos.FindAsync(dto.DepartamentoId.Value);
                if (dep == null) return BadRequest(new { mensaje = "Departamento no válido." });
                if (!dep.Activo) return BadRequest(new { mensaje = "El departamento está INACTIVO." });
            }

            // Si se intenta DESACTIVAR, verifica que no haya empleados ACTIVOS ligados
            if (p.Activo && dto.Activo == false)
            {
                var tieneActivos = await _context.Empleados
                    .AnyAsync(e => e.PuestoId == id && e.EstadoLaboral == "ACTIVO");
                if (tieneActivos)
                    return Conflict(new { mensaje = "No se puede desactivar el puesto: hay empleados ACTIVOS asociados." });
            }

            p.Nombre = dto.Nombre;
            p.SalarioBase = dto.SalarioBase;
            p.Activo = dto.Activo;
            p.DepartamentoId = dto.DepartamentoId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Puestos/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeletePuesto(int id)
        {
            var p = await _context.Puestos
                .Include(x => x.Empleados)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) return NotFound();

            if (p.Empleados.Any())
                return Conflict(new { mensaje = "El puesto no se puede eliminar porque tiene empleados asociados." });

            _context.Puestos.Remove(p);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
