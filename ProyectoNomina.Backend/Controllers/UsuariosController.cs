using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Backend.Services;
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

        // ✅ REGISTRO: Permitir sin token
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<Usuario>> RegistrarUsuario(Usuario usuario)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Correo == usuario.Correo))
                return BadRequest("Ya existe un usuario con este correo.");

            usuario.ClaveHash = BCrypt.Net.BCrypt.HashPassword(usuario.ClaveHash);
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
        }

        // ✅ LOGIN: Permitir sin token
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> Login([FromBody] LoginDto credenciales)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.Correo == credenciales.Correo);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(credenciales.Clave, usuario.ClaveHash))
                return Unauthorized("Credenciales inválidas.");

            var roles = usuario.UsuarioRoles.Select(ur => ur.Rol.Nombre).ToList();
            var token = _jwtService.GenerarToken(usuario, roles);

            return Ok(new { token });
        }

        // ✅ GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .ToListAsync();
        }

        // ✅ GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);

            return usuario == null ? NotFound() : usuario;
        }

        // ✅ DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // ✅ DTO para login
    public class LoginDto
    {
        public string Correo { get; set; }
        public string Clave { get; set; }
    }
}
