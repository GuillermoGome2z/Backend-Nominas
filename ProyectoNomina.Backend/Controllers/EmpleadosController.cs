using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Shared.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ProyectoNomina.Backend.Controllers
{
    [Authorize(Roles = "Admin,RRHH,Usuario")]
    [ApiController]
    [Route("api/[controller]")]
    // Por [Authorize] en la clase, documentamos 401 y 403 globales
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class EmpleadosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmpleadosController(AppDbContext context)
        {
            _context = context;
        }

      [HttpGet]
[ProducesResponseType(typeof(IEnumerable<EmpleadoDto>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] 
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<IEnumerable<EmpleadoDto>>> GetEmpleados(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? q = null,                    
    [FromQuery] int? departamentoId = null,          
    [FromQuery] DateTime? fechaInicio = null,        
    [FromQuery] DateTime? fechaFin = null)           
{
    // --- saneo de parámetros (igual que tenías) ---
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;

    // --- validación de rango de fechas → 422 (punto 9) ---
    if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
    {
        return UnprocessableEntity(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            ["rangoFechas"] = new[] { "fechaInicio no puede ser mayor que fechaFin." }
        }));
    }

    // --- base query ---
    var query = _context.Empleados
        .AsNoTracking()
        .Include(e => e.Departamento)
        .Include(e => e.Puesto)
        .AsQueryable();

    // --- filtros del punto 9 ---
    // q: búsqueda básica por nombre, DPI, NIT, correo
    if (!string.IsNullOrWhiteSpace(q))
    {
        var qNorm = q.Trim().ToLower();
        query = query.Where(e =>
            (e.NombreCompleto ?? "").ToLower().Contains(qNorm) ||
            (e.DPI ?? "").ToLower().Contains(qNorm) ||
            (e.NIT ?? "").ToLower().Contains(qNorm) ||
            (e.Correo ?? "").ToLower().Contains(qNorm));
    }

    // departamentoId
    if (departamentoId.HasValue)
    {
        query = query.Where(e => e.DepartamentoId == departamentoId.Value);
    }

    // fechaInicio / fechaFin (usando FechaContratacion; ajusta si deseas otra fecha)
    if (fechaInicio.HasValue)
        query = query.Where(e => e.FechaContratacion >= fechaInicio.Value.Date);

    if (fechaFin.HasValue)
        query = query.Where(e => e.FechaContratacion <= fechaFin.Value.Date);

    // --- total después de aplicar filtros ---
    var total = await query.CountAsync();

    // --- orden determinista + paginación + proyección a DTO (igual que tenías) ---
    var empleados = await query
        .OrderBy(e => e.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(e => new EmpleadoDto
        {
            Id = e.Id,
            NombreCompleto = e.NombreCompleto,
            DPI = e.DPI,
            NIT = e.NIT,
            Correo = e.Correo,
            Direccion = e.Direccion,
            Telefono = e.Telefono,
            FechaContratacion = e.FechaContratacion,
            FechaNacimiento = e.FechaNacimiento,
            EstadoLaboral = e.EstadoLaboral,
            SalarioMensual = e.SalarioMensual,
            DepartamentoId = e.DepartamentoId ?? 0,
            PuestoId = e.PuestoId,
            NombreDepartamento = e.Departamento != null ? e.Departamento.Nombre : "No disponible",
            NombrePuesto = e.Puesto != null ? e.Puesto.Nombre : "No disponible"
        })
        .ToListAsync();

    // --- header X-Total-Count (igual que tenías) ---
    Response.Headers["X-Total-Count"] = total.ToString();

    return Ok(empleados);
}

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EmpleadoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EmpleadoDto>> GetEmpleado(int id)
        {

            var e = await _context.Empleados
                .Include(e => e.Departamento)
                .Include(e => e.Puesto)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (e == null) return NotFound();

            var dto = new EmpleadoDto
            {
                Id = e.Id,
                NombreCompleto = e.NombreCompleto,
                DPI = e.DPI,
                NIT = e.NIT,
                Correo = e.Correo,
                Direccion = e.Direccion,
                Telefono = e.Telefono,
                FechaContratacion = e.FechaContratacion,
                FechaNacimiento = e.FechaNacimiento,
                SalarioMensual = e.SalarioMensual,
                DepartamentoId = e.DepartamentoId ?? 0,
                PuestoId = e.PuestoId,
                NombreDepartamento = e.Departamento?.Nombre ?? "No disponible",
                NombrePuesto = e.Puesto?.Nombre ?? "No disponible",
                EstadoLaboral = e.EstadoLaboral
            };

            return Ok(dto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostEmpleado([FromBody] EmpleadoCreacionDto dto)
        {
            if (dto.FechaNacimiento == DateTime.MinValue)
                return BadRequest("La fecha de nacimiento es inválida.");

            if (!await _context.Departamentos.AnyAsync(d => d.Id == dto.DepartamentoId))
                return BadRequest("El departamento seleccionado no existe.");

            if (!await _context.Puestos.AnyAsync(p => p.Id == dto.PuestoId))
                return BadRequest("El puesto seleccionado no existe.");

            var dpiExiste = await _context.Empleados.AnyAsync(e => e.DPI == dto.DPI);
            var nitExiste = await _context.Empleados.AnyAsync(e => e.NIT == dto.NIT);
            var correoExiste = await _context.Empleados.AnyAsync(e => e.Correo == dto.Correo);

            if (dpiExiste) return BadRequest("Ya existe un empleado con ese DPI.");
            if (nitExiste) return BadRequest("Ya existe un empleado con ese NIT.");
            if (correoExiste) return BadRequest("Ya existe un empleado con ese correo.");

            var nuevo = new Empleado
            {
                NombreCompleto = dto.NombreCompleto,
                Correo = dto.Correo,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                SalarioMensual = dto.SalarioBase,
                DepartamentoId = dto.DepartamentoId != 0 ? dto.DepartamentoId : null,
                PuestoId = dto.PuestoId,
                FechaContratacion = dto.FechaContratacion,
                FechaNacimiento = dto.FechaNacimiento,
                EstadoLaboral = dto.EstadoLaboral,
                DPI = dto.DPI,
                NIT = dto.NIT
            };

            _context.Empleados.Add(nuevo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmpleado), new { id = nuevo.Id }, new
            {
                mensaje = "Empleado creado correctamente",
                empleadoId = nuevo.Id
            });
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizarEmpleado(int id, EmpleadoDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID en la URL no coincide con el del cuerpo.");

            if (dto.FechaNacimiento == DateTime.MinValue)
                return BadRequest("La fecha de nacimiento es inválida.");

            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null)
                return NotFound();

            var otroConMismoDPI = await _context.Empleados.AnyAsync(e => e.Id != id && e.DPI == dto.DPI);
            var otroConMismoNIT = await _context.Empleados.AnyAsync(e => e.Id != id && e.NIT == dto.NIT);
            var otroConMismoCorreo = await _context.Empleados.AnyAsync(e => e.Id != id && e.Correo == dto.Correo);

            if (otroConMismoDPI) return BadRequest("Ya existe otro empleado con el mismo DPI.");
            if (otroConMismoNIT) return BadRequest("Ya existe otro empleado con el mismo NIT.");
            if (otroConMismoCorreo) return BadRequest("Ya existe otro empleado con el mismo correo.");

            empleado.NombreCompleto = dto.NombreCompleto;
            empleado.Correo = dto.Correo;
            empleado.Telefono = dto.Telefono;
            empleado.Direccion = dto.Direccion;
            empleado.FechaNacimiento = dto.FechaNacimiento;
            empleado.FechaContratacion = dto.FechaContratacion;
            empleado.EstadoLaboral = dto.EstadoLaboral;
            empleado.DPI = dto.DPI;
            empleado.NIT = dto.NIT;
            empleado.SalarioMensual = dto.SalarioMensual;
            empleado.DepartamentoId = dto.DepartamentoId != 0 ? dto.DepartamentoId : null;
            empleado.PuestoId = dto.PuestoId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("validar-duplicados")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidarDuplicados([FromBody] EmpleadoCreacionDto dto)
        {
            var dpiExiste = await _context.Empleados.AnyAsync(e => e.DPI == dto.DPI);
            var nitExiste = await _context.Empleados.AnyAsync(e => e.NIT == dto.NIT);
            var correoExiste = await _context.Empleados.AnyAsync(e => e.Correo == dto.Correo);

            return Ok(new
            {
                DpiDuplicado = dpiExiste,
                NitDuplicado = nitExiste,
                CorreoDuplicado = correoExiste
            });
        }

        [HttpGet("sin-usuario")]
        [Authorize(Roles = "Admin,RRHH")]
        [ProducesResponseType(typeof(IEnumerable<EmpleadoAsignacionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerEmpleadosSinUsuario()
        {
            var empleados = await _context.Empleados
                .Where(e => e.Usuario == null)
                .Select(e => new EmpleadoAsignacionDto
                {
                    Id = e.Id,
                    NombreCompleto = e.NombreCompleto
                })
                .ToListAsync();

            return Ok(empleados);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEmpleado(int id)
        {
            var empleado = await _context.Empleados
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (empleado == null) return NotFound();

            if (empleado.Usuario != null)
                return BadRequest("❌ No se puede eliminar el empleado porque está asignado a un usuario.");

            _context.Empleados.Remove(empleado);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("mi-informacion")]
        [Authorize(Roles = "Usuario")]
        [ProducesResponseType(typeof(EmpleadoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EmpleadoDto>> ObtenerMiInformacion()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("No se pudo identificar el usuario.");

            var usuario = await _context.Usuarios
                .Include(u => u.Empleado)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (usuario?.Empleado == null)
                return NotFound("No se ha asignado un empleado a este usuario.");

            var empleado = usuario.Empleado;

            return Ok(new EmpleadoDto
            {
                Id = empleado.Id,
                NombreCompleto = empleado.NombreCompleto,
                FechaNacimiento = empleado.FechaNacimiento,
                NIT = empleado.NIT,
                DPI = empleado.DPI,
                Correo = empleado.Correo,
                DepartamentoId = empleado.DepartamentoId.GetValueOrDefault(),
                PuestoId = empleado.PuestoId,
                EstadoLaboral = empleado.EstadoLaboral
            });
        }

        [HttpGet("empleado-actual")]
        [Authorize]
        [ProducesResponseType(typeof(int?), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerEmpleadoActual()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("No se pudo identificar al usuario.");

            int usuarioId = int.Parse(userIdClaim.Value);

            var empleadoId = await _context.Usuarios
                .Where(u => u.Id == usuarioId)
                .Select(u => u.EmpleadoId)
                .FirstOrDefaultAsync();

            if (empleadoId == null)
                return NotFound("El usuario no tiene un empleado asignado.");

            return Ok(empleadoId);
        }
    }
}
