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

        /// <summary>
        /// Obtiene puestos con filtros: departamentoId?, soloActivos?=true|false
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PuestoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PuestoDto>>> GetPuestos(
            [FromQuery] int? departamentoId = null,
            [FromQuery] bool soloActivos = true) // Por defecto solo activos según especificación
        {
            var q = _context.Puestos.AsNoTracking().AsQueryable();

            // Filtros según especificación
            if (departamentoId.HasValue) 
                q = q.Where(p => p.DepartamentoId == departamentoId.Value);
            
            if (soloActivos) 
                q = q.Where(p => p.Activo);

            var data = await q
                .OrderBy(p => p.Nombre) // Ordenar por nombre
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
        [ProducesResponseType(typeof(PuestoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PuestoDto>> PostPuesto([FromBody] PuestoDto dto)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new ProblemDetails { Title = "Nombre requerido", Detail = "El nombre del puesto es obligatorio." });

            if (dto.SalarioBase < 0)
                return UnprocessableEntity(new ProblemDetails { Title = "Salario inválido", Detail = "El salario base debe ser mayor o igual a 0." });

            // Validar que no exista otro puesto con el mismo nombre (case-insensitive)
            var nombreExiste = await _context.Puestos
                .AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower());
            
            if (nombreExiste)
                return Conflict(new ProblemDetails 
                { 
                    Title = "Puesto duplicado", 
                    Detail = $"Ya existe un puesto con el nombre '{dto.Nombre}'." 
                });

            // Validar departamento si se proporciona
            if (dto.DepartamentoId.HasValue)
            {
                var dep = await _context.Departamentos.FindAsync(dto.DepartamentoId.Value);
                if (dep == null) 
                    return BadRequest(new ProblemDetails { Title = "Departamento inválido", Detail = "El departamento seleccionado no existe." });
                if (!dep.Activo) 
                    return UnprocessableEntity(new ProblemDetails { Title = "Departamento inactivo", Detail = "El departamento seleccionado está inactivo." });
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

            var result = new PuestoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                SalarioBase = p.SalarioBase,
                Activo = p.Activo,
                DepartamentoId = p.DepartamentoId
            };

            return CreatedAtAction(nameof(GetPuesto), new { id = result.Id }, result);
        }

        // PUT: api/Puestos/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutPuesto(int id, [FromBody] PuestoDto dto)
        {
            if (id != dto.Id) 
                return BadRequest(new ProblemDetails { Title = "ID inválido", Detail = "El ID en la URL no coincide con el del cuerpo." });

            var p = await _context.Puestos.FindAsync(id);
            if (p == null) 
                return NotFound(new ProblemDetails { Title = "Puesto no encontrado", Detail = $"No existe un puesto con ID {id}." });

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new ProblemDetails { Title = "Nombre requerido", Detail = "El nombre del puesto es obligatorio." });

            if (dto.SalarioBase < 0)
                return UnprocessableEntity(new ProblemDetails { Title = "Salario inválido", Detail = "El salario base debe ser mayor o igual a 0." });

            // Validar que no exista otro puesto con el mismo nombre (excluyendo el actual)
            var nombreExiste = await _context.Puestos
                .AnyAsync(pu => pu.Nombre.ToLower() == dto.Nombre.ToLower() && pu.Id != id);
            
            if (nombreExiste)
                return Conflict(new ProblemDetails 
                { 
                    Title = "Puesto duplicado", 
                    Detail = $"Ya existe otro puesto con el nombre '{dto.Nombre}'." 
                });

            // Validar departamento si se proporciona
            if (dto.DepartamentoId.HasValue)
            {
                var dep = await _context.Departamentos.FindAsync(dto.DepartamentoId.Value);
                if (dep == null) 
                    return BadRequest(new ProblemDetails { Title = "Departamento inválido", Detail = "El departamento seleccionado no existe." });
                if (!dep.Activo) 
                    return UnprocessableEntity(new ProblemDetails { Title = "Departamento inactivo", Detail = "El departamento seleccionado está inactivo." });
            }

            // Si se intenta desactivar, verificar que no haya empleados activos asociados
            if (p.Activo && dto.Activo == false)
            {
                var tieneActivos = await _context.Empleados
                    .AnyAsync(e => e.PuestoId == id && e.EstadoLaboral == "ACTIVO");
                if (tieneActivos)
                    return Conflict(new ProblemDetails 
                    { 
                        Title = "No se puede desactivar", 
                        Detail = "El puesto tiene empleados activos asociados." 
                    });
            }

            p.Nombre = dto.Nombre;
            p.SalarioBase = dto.SalarioBase;
            p.Activo = dto.Activo;
            p.DepartamentoId = dto.DepartamentoId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Activar un puesto
        /// </summary>
        /// <param name="id">ID del puesto</param>
        /// <returns>204 NoContent si la operación fue exitosa</returns>
        [HttpPut("{id}/activar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActivarPuesto(int id)
        {
            var puesto = await _context.Puestos.FindAsync(id);
            if (puesto == null)
                return NotFound(new ProblemDetails { Title = "Puesto no encontrado", Detail = $"No existe un puesto con ID {id}." });

            puesto.Activo = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Desactivar un puesto
        /// </summary>
        /// <param name="id">ID del puesto</param>
        /// <returns>204 NoContent si exitoso, 409 Conflict si tiene empleados activos</returns>
        /// <remarks>
        /// No se puede desactivar si tiene empleados activos asociados.
        /// </remarks>
        [HttpPut("{id}/desactivar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DesactivarPuesto(int id)
        {
            var puesto = await _context.Puestos.FindAsync(id);
            if (puesto == null)
                return NotFound(new ProblemDetails { Title = "Puesto no encontrado", Detail = $"No existe un puesto con ID {id}." });

            // Verificar que no tenga empleados ACTIVOS asociados
            var tieneEmpleadosActivos = await _context.Empleados
                .AnyAsync(e => e.PuestoId == id && e.EstadoLaboral == "ACTIVO");

            if (tieneEmpleadosActivos)
                return Conflict(new ProblemDetails 
                { 
                    Title = "No se puede desactivar", 
                    Detail = "El puesto tiene empleados activos asociados." 
                });

            puesto.Activo = false;
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
