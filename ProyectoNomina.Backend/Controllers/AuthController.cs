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
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            // Validación mínima del payload
            if (request is null || string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Contraseña))
                return BadRequest("Correo y contraseña son requeridos.");

            // ⚠️ Credenciales de prueba (ajusta a tu verificación real de hash)
            var credencialesOK = request.Correo == "admin@empresa.com" && request.Contraseña == "Admin123!";
            if (!credencialesOK)
                return Unauthorized("Credenciales inválidas");

            // 🔴 ANTES: se creaba un Usuario con Id=1 “a mano” (posible FK inválida).
            // ✅ AHORA: obtenemos el usuario REAL desde la BD por Correo.
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Correo == request.Correo);

            if (usuario == null)
                return Unauthorized("Usuario no encontrado en la base de datos.");

            // Rol: si usas tabla Roles/UsuarioRoles, arma el string si lo necesitas en el token
            // (aquí usamos el campo Rol del modelo para mantener tu lógica actual)
            var jwt = _jwtService.GenerarToken(usuario);

            // Generar refresh token (7 días)
            var refresh = new RefreshToken
            {
                UsuarioId = usuario.Id,                 // <- Id REAL de BD
                Token = _jwtService.GenerarRefreshToken(),
                Expira = DateTime.UtcNow.AddDays(7),
                Revocado = false
            };

            _context.RefreshTokens.Add(refresh);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log útil en desarrollo
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.InnerException?.Message);

                // Respuesta estándar (tu middleware de errores también la capturará)
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    status = 500,
                    message = "Error interno al guardar el refresh token.",
                    detail = ex.InnerException?.Message ?? ex.Message
                });
            }

            // Devolver el refresh token por header sin romper tu DTO
            Response.Headers.Append("X-Refresh-Token", refresh.Token);

            return Ok(new LoginResponseDto
            {
                Token = jwt,
                NombreUsuario = usuario.NombreCompleto,
                Rol = usuario.Rol
            });
        }

        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.InnerException?.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    status = 500,
                    message = "Error interno al actualizar el refresh token.",
                    detail = ex.InnerException?.Message ?? ex.Message
                });
            }

            var nuevoJwt = _jwtService.GenerarToken(usuario);

            return Ok(new
            {
                token = nuevoJwt,
                refreshToken = nuevoRefresh.Token
            });
        }
    }
}
