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
    public class EmpleadosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmpleadosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmpleadoDto>>> GetEmpleados()
        {
            var empleados = await _context.Empleados
                .Include(e => e.Departamento)
                .Include(e => e.Puesto)
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

            return Ok(empleados);
        }

        [HttpGet("{id}")]
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
