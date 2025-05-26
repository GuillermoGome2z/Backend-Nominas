using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using BCrypt.Net;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .ToListAsync();
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);

            return usuario == null ? NotFound() : usuario;
        }

        // POST: api/Usuarios (registro)
        [HttpPost("register")]
        public async Task<ActionResult<Usuario>> RegistrarUsuario(Usuario usuario)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Correo == usuario.Correo))
                return BadRequest("Ya existe un usuario con este correo.");

            usuario.ClaveHash = BCrypt.Net.BCrypt.HashPassword(usuario.ClaveHash);
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
        }

        // POST: api/Usuarios/login
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto credenciales)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == credenciales.Correo);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(credenciales.Clave, usuario.ClaveHash))
                return Unauthorized("Credenciales inválidas.");

            // Aquí luego generamos y devolvemos el JWT
            return Ok("Login exitoso");
        }

        // DELETE: api/Usuarios/5
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

    public class LoginDto
    {
        public string Correo { get; set; }
        public string Clave { get; set; }
    }
}
