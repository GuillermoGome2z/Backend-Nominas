using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> ExisteUsuario()
        {
            bool existe = await _context.Usuarios.AnyAsync();
            return Ok(existe);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto credenciales)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == credenciales.Correo);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(credenciales.Contraseña, usuario.ClaveHash))
                return Unauthorized("Credenciales inválidas.");

            var token = _jwtService.GenerarToken(usuario);

            return Ok(new LoginResponseDto
            {
                Token = token,
                NombreUsuario = usuario.NombreCompleto,
                Rol = usuario.Rol
            });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            return usuario == null ? NotFound() : usuario;
        }

        [HttpDelete("{id}")]
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
        public async Task<IActionResult> ActualizarRol(int id, [FromBody] ActualizarRolDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound("Usuario no encontrado.");

            usuario.Rol = dto.Rol;
            await _context.SaveChangesAsync();

            return Ok("Rol actualizado correctamente.");
        }

        // ✅ NUEVO: Obtener el EmpleadoId del usuario autenticado
        [HttpGet("empleado-actual")]
        [Authorize]
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
