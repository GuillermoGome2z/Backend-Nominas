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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class DepartamentosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepartamentosController(AppDbContext context) => _context = context;

        // GET: api/Departamentos
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DepartamentoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DepartamentoDto>>> GetDepartamentos(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? activo = null)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

            var q = _context.Departamentos.AsNoTracking();
            if (activo.HasValue) q = q.Where(d => d.Activo == activo.Value);

            var total = await q.CountAsync();

            var lista = await q
                .OrderBy(d => d.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DepartamentoDto { Id = d.Id, Nombre = d.Nombre, Activo = d.Activo })
                .ToListAsync();

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(lista);
        }

        // GET: api/Departamentos/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DepartamentoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DepartamentoDto>> GetDepartamento(int id)
        {
            var d = await _context.Departamentos.FindAsync(id);
            if (d == null) return NotFound(new { mensaje = "Departamento no encontrado." });

            return new DepartamentoDto { Id = d.Id, Nombre = d.Nombre, Activo = d.Activo };
        }

        // NUEVO: GET: api/Departamentos/5/Puestos
        [HttpGet("{id}/Puestos")]
        [ProducesResponseType(typeof(IEnumerable<PuestoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PuestoDto>>> GetPuestosPorDepartamento(
            int id,
            [FromQuery] bool? activo = null)
        {
            var dep = await _context.Departamentos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
            if (dep == null) return NotFound(new { mensaje = "Departamento no encontrado." });

            var q = _context.Puestos.AsNoTracking().Where(p => p.DepartamentoId == id);
            if (activo.HasValue) q = q.Where(p => p.Activo == activo.Value);

            var data = await q
                .OrderBy(p => p.Id)
                .Select(p => new PuestoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    SalarioBase = p.SalarioBase,
                    Activo = p.Activo,
                    DepartamentoId = p.DepartamentoId
                })
                .ToListAsync();

            return Ok(data);
        }

        // POST: api/Departamentos
        [HttpPost]
        [ProducesResponseType(typeof(DepartamentoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> PostDepartamento([FromBody] DepartamentoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new { mensaje = "El nombre del departamento es obligatorio." });

            var existe = await _context.Departamentos.AnyAsync(d => d.Nombre == dto.Nombre);
            if (existe)
                return BadRequest(new { mensaje = "Ya existe un departamento con ese nombre." });

            var nuevo = new Departamento { Nombre = dto.Nombre, Activo = dto.Activo };
            _context.Departamentos.Add(nuevo);
            await _context.SaveChangesAsync();

            var result = new DepartamentoDto { Id = nuevo.Id, Nombre = nuevo.Nombre, Activo = nuevo.Activo };
            return CreatedAtAction(nameof(GetDepartamento), new { id = result.Id }, result);
        }

        // PUT: api/Departamentos/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PutDepartamento(int id, [FromBody] DepartamentoDto dto)
        {
            if (id != dto.Id) return BadRequest(new { mensaje = "ID de departamento no válido." });

            var d = await _context.Departamentos.FindAsync(id);
            if (d == null) return NotFound(new { mensaje = "Departamento no encontrado." });

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new { mensaje = "El nombre del departamento es obligatorio." });

            // Desactivar: no debe tener empleados ACTIVOS ni puestos ACTIVOS
            if (d.Activo && dto.Activo == false)
            {
                var tieneEmpleadosActivos = await _context.Empleados
                    .AnyAsync(e => e.DepartamentoId == id && e.EstadoLaboral == "ACTIVO");

                var tienePuestosActivos = await _context.Puestos
                    .AnyAsync(p => p.DepartamentoId == id && p.Activo);

                if (tieneEmpleadosActivos || tienePuestosActivos)
                    return Conflict(new { mensaje = "No se puede desactivar: hay empleados o puestos ACTIVOS asociados." });
            }

            d.Nombre = dto.Nombre;
            d.Activo = dto.Activo;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Departamentos/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteDepartamento(int id)
        {
            var d = await _context.Departamentos
                .Include(x => x.Empleados)
                .Include(x => x.Puestos)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (d == null) return NotFound(new { mensaje = "Departamento no encontrado." });

            if (d.Empleados.Any() || d.Puestos.Any())
                return Conflict(new { mensaje = "No se puede eliminar: tiene empleados o puestos asociados." });

            _context.Departamentos.Remove(d);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
