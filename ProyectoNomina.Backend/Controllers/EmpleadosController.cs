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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class EmpleadosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmpleadosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Empleados
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
            [FromQuery] int? puestoId = null,                 // NUEVO
            [FromQuery] string? estadoLaboral = null,         // NUEVO (ACTIVO/SUSPENDIDO/RETIRADO)
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
            {
                return UnprocessableEntity(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["rangoFechas"] = new[] { "fechaInicio no puede ser mayor que fechaFin." }
                }));
            }

            var query = _context.Empleados
                .AsNoTracking()
                .Include(e => e.Departamento)
                .Include(e => e.Puesto)
                .AsQueryable();

            // Búsqueda básica
            if (!string.IsNullOrWhiteSpace(q))
            {
                var qNorm = q.Trim().ToLower();
                query = query.Where(e =>
                    (e.NombreCompleto ?? "").ToLower().Contains(qNorm) ||
                    (e.DPI ?? "").ToLower().Contains(qNorm) ||
                    (e.NIT ?? "").ToLower().Contains(qNorm) ||
                    (e.Correo ?? "").ToLower().Contains(qNorm));
            }

            if (departamentoId.HasValue)
                query = query.Where(e => e.DepartamentoId == departamentoId.Value);

            if (puestoId.HasValue)
                query = query.Where(e => e.PuestoId == puestoId.Value);

            if (!string.IsNullOrWhiteSpace(estadoLaboral))
            {
                var est = estadoLaboral.Trim().ToUpper();
                query = query.Where(e => e.EstadoLaboral == est);
            }

            if (fechaInicio.HasValue)
                query = query.Where(e => e.FechaContratacion >= fechaInicio.Value.Date);

            if (fechaFin.HasValue)
                query = query.Where(e => e.FechaContratacion <= fechaFin.Value.Date);

            var total = await query.CountAsync();

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
                    DepartamentoId = e.DepartamentoId,
                    PuestoId = e.PuestoId,
                    NombreDepartamento = e.Departamento != null ? e.Departamento.Nombre : null,
                    NombrePuesto = e.Puesto != null ? e.Puesto.Nombre : null
                })
                .ToListAsync();

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(empleados);
        }

        // GET: api/Empleados/5
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

            return Ok(new EmpleadoDto
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
                DepartamentoId = e.DepartamentoId,
                PuestoId = e.PuestoId,
                NombreDepartamento = e.Departamento?.Nombre,
                NombrePuesto = e.Puesto?.Nombre,
                EstadoLaboral = e.EstadoLaboral
            });
        }

        // POST: api/Empleados
        [HttpPost]
        [Authorize(Roles = "Admin,RRHH")]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostEmpleado([FromBody] EmpleadoCreateUpdateDto dto)
        {
            // Validaciones básicas
            if (dto.FechaNacimiento.HasValue && dto.FechaNacimiento > DateTime.Today)
                return UnprocessableEntity(new ProblemDetails
                {
                    Title = "Fecha de nacimiento inválida",
                    Detail = "La fecha de nacimiento no puede ser futura."
                });

            if (dto.FechaContratacion > DateTime.Today)
                return UnprocessableEntity(new ProblemDetails
                {
                    Title = "Fecha de contratación inválida",
                    Detail = "La fecha de contratación no puede ser futura."
                });

            // Validar departamento si se proporciona
            if (dto.DepartamentoId.HasValue)
            {
                var depto = await _context.Departamentos.FirstOrDefaultAsync(d => d.Id == dto.DepartamentoId);
                if (depto == null) 
                    return BadRequest(new ProblemDetails { Title = "Departamento inválido", Detail = "El departamento seleccionado no existe." });
                if (!depto.Activo) 
                    return UnprocessableEntity(new ProblemDetails { Title = "Departamento inactivo", Detail = "El departamento seleccionado está inactivo." });
            }

            // Validar puesto si se proporciona
            if (dto.PuestoId.HasValue)
            {
                var puesto = await _context.Puestos.Include(p => p.Departamento).FirstOrDefaultAsync(p => p.Id == dto.PuestoId);
                if (puesto == null) 
                    return BadRequest(new ProblemDetails { Title = "Puesto inválido", Detail = "El puesto seleccionado no existe." });
                if (!puesto.Activo) 
                    return UnprocessableEntity(new ProblemDetails { Title = "Puesto inactivo", Detail = "El puesto seleccionado está inactivo." });
                
                // Validar que el puesto pertenece al departamento si ambos están especificados
                if (dto.DepartamentoId.HasValue && puesto.DepartamentoId != dto.DepartamentoId)
                    return UnprocessableEntity(new ProblemDetails { Title = "Inconsistencia departamento-puesto", Detail = "El puesto seleccionado no pertenece al departamento especificado." });
            }

            // Validar duplicados
            if (!string.IsNullOrWhiteSpace(dto.DPI))
            {
                var dpiExiste = await _context.Empleados.AnyAsync(e => e.DPI == dto.DPI);
                if (dpiExiste) 
                    return UnprocessableEntity(new ProblemDetails { Title = "DPI duplicado", Detail = "Ya existe un empleado con ese DPI." });
            }

            if (!string.IsNullOrWhiteSpace(dto.NIT))
            {
                var nitExiste = await _context.Empleados.AnyAsync(e => e.NIT == dto.NIT);
                if (nitExiste) 
                    return UnprocessableEntity(new ProblemDetails { Title = "NIT duplicado", Detail = "Ya existe un empleado con ese NIT." });
            }

            if (!string.IsNullOrWhiteSpace(dto.Correo))
            {
                var correoExiste = await _context.Empleados.AnyAsync(e => e.Correo == dto.Correo);
                if (correoExiste) 
                    return UnprocessableEntity(new ProblemDetails { Title = "Correo duplicado", Detail = "Ya existe un empleado con ese correo." });
            }

            var nuevo = new Empleado
            {
                NombreCompleto = dto.NombreCompleto,
                Correo = dto.Correo,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                SalarioMensual = dto.SalarioMensual,
                DepartamentoId = dto.DepartamentoId,
                PuestoId = dto.PuestoId,
                FechaContratacion = dto.FechaContratacion,
                FechaNacimiento = dto.FechaNacimiento,
                EstadoLaboral = "ACTIVO", // Siempre empieza activo
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

        // PUT: api/Empleados/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,RRHH")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizarEmpleado(int id, EmpleadoCreateUpdateDto dto)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null)
                return NotFound(new ProblemDetails { Title = "Empleado no encontrado", Detail = $"No existe un empleado con ID {id}." });

            // Validaciones básicas
            if (dto.FechaNacimiento.HasValue && dto.FechaNacimiento > DateTime.Today)
                return UnprocessableEntity(new ProblemDetails { Title = "Fecha de nacimiento inválida", Detail = "La fecha de nacimiento no puede ser futura." });

            if (dto.FechaContratacion > DateTime.Today)
                return UnprocessableEntity(new ProblemDetails { Title = "Fecha de contratación inválida", Detail = "La fecha de contratación no puede ser futura." });

            // Validar departamento si se proporciona
            if (dto.DepartamentoId.HasValue)
            {
                var depto = await _context.Departamentos.FirstOrDefaultAsync(d => d.Id == dto.DepartamentoId);
                if (depto == null) 
                    return BadRequest(new ProblemDetails { Title = "Departamento inválido", Detail = "El departamento seleccionado no existe." });
                if (!depto.Activo) 
                    return UnprocessableEntity(new ProblemDetails { Title = "Departamento inactivo", Detail = "El departamento seleccionado está inactivo." });
            }

            // Validar puesto si se proporciona
            if (dto.PuestoId.HasValue)
            {
                var puesto = await _context.Puestos.Include(p => p.Departamento).FirstOrDefaultAsync(p => p.Id == dto.PuestoId);
                if (puesto == null) 
                    return BadRequest(new ProblemDetails { Title = "Puesto inválido", Detail = "El puesto seleccionado no existe." });
                if (!puesto.Activo) 
                    return UnprocessableEntity(new ProblemDetails { Title = "Puesto inactivo", Detail = "El puesto seleccionado está inactivo." });
                
                // Validar que el puesto pertenece al departamento si ambos están especificados
                if (dto.DepartamentoId.HasValue && puesto.DepartamentoId != dto.DepartamentoId)
                    return UnprocessableEntity(new ProblemDetails { Title = "Inconsistencia departamento-puesto", Detail = "El puesto seleccionado no pertenece al departamento especificado." });
            }

            // Validar duplicados excluyendo el empleado actual
            if (!string.IsNullOrWhiteSpace(dto.DPI))
            {
                var otroConMismoDPI = await _context.Empleados.AnyAsync(e => e.Id != id && e.DPI == dto.DPI);
                if (otroConMismoDPI) 
                    return UnprocessableEntity(new ProblemDetails { Title = "DPI duplicado", Detail = "Ya existe otro empleado con el mismo DPI." });
            }

            if (!string.IsNullOrWhiteSpace(dto.NIT))
            {
                var otroConMismoNIT = await _context.Empleados.AnyAsync(e => e.Id != id && e.NIT == dto.NIT);
                if (otroConMismoNIT) 
                    return UnprocessableEntity(new ProblemDetails { Title = "NIT duplicado", Detail = "Ya existe otro empleado con el mismo NIT." });
            }

            if (!string.IsNullOrWhiteSpace(dto.Correo))
            {
                var otroConMismoCorreo = await _context.Empleados.AnyAsync(e => e.Id != id && e.Correo == dto.Correo);
                if (otroConMismoCorreo) 
                    return UnprocessableEntity(new ProblemDetails { Title = "Correo duplicado", Detail = "Ya existe otro empleado con el mismo correo." });
            }

            // Actualizar campos (excepto EstadoLaboral, que se maneja por separado)
            empleado.NombreCompleto = dto.NombreCompleto;
            empleado.Correo = dto.Correo;
            empleado.Telefono = dto.Telefono;
            empleado.Direccion = dto.Direccion;
            empleado.FechaNacimiento = dto.FechaNacimiento;
            empleado.FechaContratacion = dto.FechaContratacion;
            empleado.DPI = dto.DPI;
            empleado.NIT = dto.NIT;
            empleado.SalarioMensual = dto.SalarioMensual;
            empleado.DepartamentoId = dto.DepartamentoId;
            empleado.PuestoId = dto.PuestoId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Cambiar estado laboral de un empleado
        /// </summary>
        /// <param name="id">ID del empleado</param>
        /// <param name="dto">Nuevo estado: ACTIVO, SUSPENDIDO o RETIRADO</param>
        /// <returns>204 NoContent si la operación fue exitosa</returns>
        /// <remarks>
        /// Ejemplo de request body:
        /// 
        ///     PUT /api/Empleados/5/estado
        ///     {
        ///       "estadoLaboral": "SUSPENDIDO"
        ///     }
        /// 
        /// </remarks>
        [HttpPut("{id}/estado")]
        [Authorize(Roles = "Admin,RRHH")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CambiarEstadoEmpleado(int id, CambiarEstadoEmpleadoDto dto)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null)
                return NotFound(new ProblemDetails { Title = "Empleado no encontrado", Detail = $"No existe un empleado con ID {id}." });

            // El DTO ya valida que sea ACTIVO, SUSPENDIDO o RETIRADO
            empleado.EstadoLaboral = dto.EstadoLaboral;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("validar-duplicados")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidarDuplicados([FromBody] EmpleadoCreateUpdateDto dto)
        {
            var dpiExiste = !string.IsNullOrWhiteSpace(dto.DPI) && await _context.Empleados.AnyAsync(e => e.DPI == dto.DPI);
            var nitExiste = !string.IsNullOrWhiteSpace(dto.NIT) && await _context.Empleados.AnyAsync(e => e.NIT == dto.NIT);
            var correoExiste = !string.IsNullOrWhiteSpace(dto.Correo) && await _context.Empleados.AnyAsync(e => e.Correo == dto.Correo);

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

        // DELETE: Comentado según requerimientos - usar cambiar estado en su lugar
        // [HttpDelete("{id}")]
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Solo Admin puede eliminar físicamente
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
                Telefono = empleado.Telefono,
                Direccion = empleado.Direccion,
                FechaContratacion = empleado.FechaContratacion,
                SalarioMensual = empleado.SalarioMensual,
                DepartamentoId = empleado.DepartamentoId,
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
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
