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
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(IConfiguration config, AppDbContext context, JwtService jwtService)
        {
            _config = config;
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        // Documentación de respuestas requeridas: 200 / 400 / 401 / 422 / 500
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            // ⚠️ Usuario fijo para pruebas (ajusta a tu lógica real cuando conectes con Usuarios)
            if (request.Correo == "admin@empresa.com" && request.Contraseña == "Admin123!")
            {
                // Puedes buscar el usuario real en BD si quieres:
                // var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.Correo);
                // if (usuario == null) return Unauthorized("Usuario no encontrado.");

                // Usuario de ejemplo para generar el JWT:
                var usuario = new Usuario
                {
                    Id = 1,
                    NombreCompleto = "Administrador",
                    Correo = request.Correo,
                    Rol = "Admin"
                };

                var jwt = _jwtService.GenerarToken(usuario);

                // Generar y guardar refresh token (7 días)
                var refresh = new RefreshToken
                {
                    UsuarioId = usuario.Id,
                    Token = _jwtService.GenerarRefreshToken(),
                    Expira = DateTime.UtcNow.AddDays(7),
                    Revocado = false
                };

                _context.RefreshTokens.Add(refresh);
                await _context.SaveChangesAsync();

                // Devolver el refresh token por header sin romper tu DTO:
                Response.Headers.Append("X-Refresh-Token", refresh.Token);

                return Ok(new LoginResponseDto
                {
                    Token = jwt,
                    NombreUsuario = usuario.NombreCompleto,
                    Rol = usuario.Rol
                });
            }

            return Unauthorized("Credenciales inválidas");
        }

        [HttpPost("refresh")]
        public async Task<ActionResult> Refresh([FromBody] string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return BadRequest("Se requiere el refresh token.");

            var stored = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.Revocado);

            if (stored == null || stored.Expira < DateTime.UtcNow)
                return Unauthorized("Refresh token inválido o expirado.");

            var usuario = await _context.Usuarios.FindAsync(stored.UsuarioId);
            if (usuario == null)
                return Unauthorized("Usuario no encontrado.");

            // Revocar el token antiguo
            stored.Revocado = true;

            // Crear uno nuevo
            var nuevoRefresh = new RefreshToken
            {
                UsuarioId = usuario.Id,
                Token = _jwtService.GenerarRefreshToken(),
                Expira = DateTime.UtcNow.AddDays(7),
                Revocado = false
            };

            _context.RefreshTokens.Add(nuevoRefresh);
            await _context.SaveChangesAsync();

            var nuevoJwt = _jwtService.GenerarToken(usuario);

            return Ok(new
            {
                token = nuevoJwt,
                refreshToken = nuevoRefresh.Token
            });
        }
    }
}
