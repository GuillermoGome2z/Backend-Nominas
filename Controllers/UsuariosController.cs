using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Backend.Services;
using ProyectoNomina.Shared.Models.DTOs;
using System.Security.Claims;
using BCrypt.Net;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public UsuariosController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPut("asignar-empleado")]
        [Authorize(Roles = "Admin,RRHH")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AsignarEmpleadoAUsuario(int usuarioId, int empleadoId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return NotFound("Usuario no encontrado.");

            bool empleadoYaAsignado = await _context.Usuarios.AnyAsync(u => u.EmpleadoId == empleadoId);
            if (empleadoYaAsignado)
                return BadRequest("Ese empleado ya está vinculado a otro usuario.");

            usuario.EmpleadoId = empleadoId;
            await _context.SaveChangesAsync();

            return Ok("Empleado asignado correctamente.");
        }

        [HttpGet("sin-empleado")]
        [Authorize(Roles = "Admin,RRHH")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<UsuarioDto>>> ObtenerUsuariosSinEmpleado()
        {
            var usuarios = await _context.Usuarios
                .Where(u => u.EmpleadoId == null)
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    NombreCompleto = u.NombreCompleto,
                    Correo = u.Correo,
                    Rol = u.Rol
                })
                .ToListAsync();

            return usuarios;
        }

        [HttpPost("registrar")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Registrar(UsuarioRegistroDto dto)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Correo == dto.Correo))
                return BadRequest("Ya existe un usuario con este correo.");

            if (dto.EmpleadoId != null)
            {
                bool yaAsignado = await _context.Usuarios.AnyAsync(u => u.EmpleadoId == dto.EmpleadoId);
                if (yaAsignado)
                    return BadRequest("Ese empleado ya está vinculado a otro usuario.");
            }

            var usuario = new Usuario
            {
                NombreCompleto = dto.Nombre,
                Correo = dto.Correo,
                ClaveHash = BCrypt.Net.BCrypt.HashPassword(dto.Contraseña),
                Rol = "Usuario",
                EmpleadoId = dto.EmpleadoId
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok("✅ Usuario registrado correctamente.");
        }

        [HttpGet("existe-usuario")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExisteUsuario()
        {
            bool existe = await _context.Usuarios.AnyAsync();
            return Ok(existe);
        }

        // Login movido a AuthController según especificaciones del prompt
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios([FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
{
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;

    var baseQuery = _context.UsuarioRoles
        .AsNoTracking()
        .Include(ur => ur.Usuario)
        .Include(ur => ur.Rol);

    var total = await baseQuery.CountAsync();

    var asignaciones = await baseQuery
    .OrderBy(ur => ur.UsuarioId)
    .ThenBy(ur => ur.RolId)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();

    Response.Headers["X-Total-Count"] = total.ToString();
    return Ok(asignaciones);
}

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            return usuario == null ? NotFound() : usuario;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/actualizar-rol")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ActualizarRol(int id, [FromBody] ActualizarRolDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound("Usuario no encontrado.");

            usuario.Rol = dto.Rol;
            await _context.SaveChangesAsync();

            return Ok("Rol actualizado correctamente.");
        }

        //  Obtener el EmpleadoId del usuario autenticado
        [HttpGet("empleado-actual")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> ObtenerEmpleadoActual()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("No se pudo identificar el usuario.");

            var usuario = await _context.Usuarios.FindAsync(int.Parse(userId));

            if (usuario == null || usuario.EmpleadoId == null)
                return NotFound("Este usuario no tiene un empleado asignado.");

            return Ok(usuario.EmpleadoId);
        }
    }
}
