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
            if (request is null || string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Contraseña))
                return BadRequest("Correo y contraseña son requeridos.");

            // Buscar usuario REAL en BD por correo
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Correo == request.Correo);

            if (usuario is null)
                return Unauthorized("Credenciales inválidas.");

            //  Verificar hash con BCrypt
            var passwordOk = BCrypt.Net.BCrypt.Verify(request.Contraseña, usuario.ClaveHash);
            if (!passwordOk)
                return Unauthorized("Credenciales inválidas.");

            // (Opcional) política de “un solo refresh activo por usuario”:
            // Revocar todos los refresh no revocados del usuario antes de emitir uno nuevo.
            var tokensPrevios = await _context.RefreshTokens
                .Where(t => t.UsuarioId == usuario.Id && !t.Revocado && t.Expira > DateTime.UtcNow)
                .ToListAsync();

            if (tokensPrevios.Count > 0)
            {
                foreach (var t in tokensPrevios) t.Revocado = true;
            }

            //  Generar JWT
            var jwt = _jwtService.GenerarToken(usuario);

            //  Generar nuevo refresh token (7 días)
            var refresh = new RefreshToken
            {
                UsuarioId = usuario.Id,
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
                // En prod, usa logger; aquí mantenemos coherencia con tu patrón
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.InnerException?.Message);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    status = 500,
                    message = "Error interno al guardar el refresh token.",
                    detail = ex.InnerException?.Message ?? ex.Message
                });
            }

            //  Devolver el refresh token por header para que el front lo pueda leer (exponer en CORS)
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

            // Necesitamos entidad rastreada para poder modificar (revocar)
            var stored = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.Revocado);

            if (stored == null || stored.Expira < DateTime.UtcNow)
                return Unauthorized("Refresh token inválido o expirado.");

            var usuario = await _context.Usuarios.FindAsync(stored.UsuarioId);
            if (usuario == null)
                return Unauthorized("Usuario no encontrado.");

            //  Rotación: revocar el token antiguo
            stored.Revocado = true;

            //  Crear uno nuevo
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

        //  Logout: revoca un refresh token vigente. Devuelve 204
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return BadRequest("Se requiere el refresh token.");

            var stored = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.Revocado);

            if (stored != null)
            {
                stored.Revocado = true;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}
