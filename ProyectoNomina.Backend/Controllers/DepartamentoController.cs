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
    public class DepartamentosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepartamentosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Departamentos
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DepartamentoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<DepartamentoDto>>> GetDepartamentos(
             [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;

    var baseQuery = _context.Departamentos.AsNoTracking();

    var total = await baseQuery.CountAsync();

    var lista = await baseQuery
        .OrderBy(d => d.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(d => new DepartamentoDto
        {
            Id = d.Id,
            Nombre = d.Nombre
        })
        .ToListAsync();

    Response.Headers["X-Total-Count"] = total.ToString();
    return Ok(lista);
}

        // GET: api/Departamentos/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DepartamentoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DepartamentoDto>> GetDepartamento(int id)
        {
            var departamento = await _context.Departamentos.FindAsync(id);

            if (departamento == null)
                return NotFound(new { mensaje = "Departamento no encontrado." });

            return new DepartamentoDto
            {
                Id = departamento.Id,
                Nombre = departamento.Nombre
            };
        }

        // POST: api/Departamentos
        [HttpPost]
        // Cubrimos 200 (tu retorno actual), y documentamos 201/204/400/422/500
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DepartamentoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PostDepartamento([FromBody] DepartamentoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new { mensaje = "El nombre del departamento es obligatorio." });

            var existe = await _context.Departamentos.AnyAsync(d => d.Nombre == dto.Nombre);
            if (existe)
                return BadRequest(new { mensaje = "Ya existe un departamento con ese nombre." });

            var nuevo = new Departamento
            {
                Nombre = dto.Nombre
            };

            _context.Departamentos.Add(nuevo);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Departamentos/5
        [HttpPut("{id}")]
        // Cubrimos 200 (tu retorno actual), y también 204/400/404/422/500
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutDepartamento(int id, [FromBody] DepartamentoDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { mensaje = "ID de departamento no válido." });

            var departamento = await _context.Departamentos.FindAsync(id);
            if (departamento == null)
                return NotFound(new { mensaje = "Departamento no encontrado." });

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new { mensaje = "El nombre del departamento es obligatorio." });

            departamento.Nombre = dto.Nombre;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/Departamentos/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteDepartamento(int id)
        {
            var departamento = await _context.Departamentos
                                             .Include(d => d.Empleados)
                                             .FirstOrDefaultAsync(d => d.Id == id);

            if (departamento == null)
                return NotFound(new { mensaje = "Departamento no encontrado." });

            if (departamento.Empleados.Any())
                return BadRequest(new { mensaje = "El departamento no se puede eliminar porque tiene empleados asociados." });

            _context.Departamentos.Remove(departamento);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
