using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Backend.Services;
using ProyectoNomina.Shared.Models.DTOs;

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

        // ✅ REGISTRO
        [HttpPost("registrar")]
        [AllowAnonymous]
        public async Task<ActionResult> RegistrarUsuario([FromBody] RegistrarUsuarioDto dto)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Correo == dto.Correo))
                return BadRequest("Ya existe un usuario con este correo.");

            var usuario = new Usuario
            {
                NombreCompleto = dto.NombreCompleto,
                Correo = dto.Correo,
                ClaveHash = BCrypt.Net.BCrypt.HashPassword(dto.Clave),
                Rol = "Usuario"
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok("Usuario registrado correctamente.");
        }

        [HttpGet("existe-usuario")]
        [AllowAnonymous]
        public async Task<IActionResult> ExisteUsuario()
        {
            bool existe = await _context.Usuarios.AnyAsync();
            return Ok(existe);
        }

        // ✅ LOGIN
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

        // ✅ GET todos los usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios
                .ToListAsync();
        }

        // ✅ GET por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);

            return usuario == null ? NotFound() : usuario;
        }

        // ✅ DELETE por ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        //Actializa los roles de los usuarios
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
    }
}
