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

        /// <summary>
        /// Autenticación de usuarios
        /// Responde 200 en éxito, 401 credenciales inválidas, 403 según rol
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            // Validación básica del request
            if (request is null || string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Contraseña))
                return BadRequest(new ProblemDetails 
                { 
                    Title = "Datos inválidos", 
                    Detail = "Correo y contraseña son requeridos." 
                });

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Correo == request.Correo);

            // 401 para credenciales inválidas
            if (usuario is null)
                return Unauthorized(new ProblemDetails 
                { 
                    Title = "Credenciales inválidas", 
                    Detail = "El correo o la contraseña son incorrectos." 
                });

            var passwordOk = BCrypt.Net.BCrypt.Verify(request.Contraseña, usuario.ClaveHash);
            if (!passwordOk)
                return Unauthorized(new ProblemDetails 
                { 
                    Title = "Credenciales inválidas", 
                    Detail = "El correo o la contraseña son incorrectos." 
                });

            // Verificación adicional de estado del usuario (opcional para 403)
            // Por ejemplo, si el usuario está deshabilitado:
            // if (!usuario.Activo) 
            //     return Forbid(new ProblemDetails { Title = "Usuario deshabilitado", Detail = "Su cuenta está deshabilitada." });

            // (Opcional) política: un solo refresh activo por usuario
            var tokensPrevios = await _context.RefreshTokens
                .Where(t => t.UsuarioId == usuario.Id && !t.Revocado && t.Expira > DateTime.UtcNow)
                .ToListAsync();
            if (tokensPrevios.Count > 0)
                foreach (var t in tokensPrevios) t.Revocado = true;

            var jwt = _jwtService.GenerarToken(usuario);

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
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.InnerException?.Message);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    status = 500,
                    message = "Error interno al guardar el refresh token.",
                    detail = ex.InnerException?.Message ?? ex.Message
                });
            }

            // Exponer en CORS (ya lo dejaste en Program.cs)
            Response.Headers.Append("X-Refresh-Token", refresh.Token);

            return Ok(new LoginResponseDto
            {
                Token = jwt,
                NombreUsuario = usuario.NombreCompleto,
                Rol = usuario.Rol
            });
        }

        // ====== REFRESH usando DTO ======
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(RefreshResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<RefreshResponseDto>> Refresh([FromBody] RefreshRequestDto request)
        {
            // [ApiController] + ApiBehaviorOptions devuelve 422 si el modelo no es válido,
            // pero mantenemos BadRequest por si llega whitespace.
            if (request is null || string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest("Se requiere el refresh token.");

            var stored = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == request.RefreshToken && !t.Revocado);

            if (stored == null || stored.Expira < DateTime.UtcNow)
                return Unauthorized("Refresh token inválido o expirado.");

            var usuario = await _context.Usuarios.FindAsync(stored.UsuarioId);
            if (usuario == null)
                return Unauthorized("Usuario no encontrado.");

            // Rotación: revocar y crear uno nuevo
            stored.Revocado = true;

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

            return Ok(new RefreshResponseDto
            {
                Token = nuevoJwt,
                RefreshToken = nuevoRefresh.Token
            });
        }

        // ====== LOGOUT usando DTO ======
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Logout([FromBody] RefreshRequestDto request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest("Se requiere el refresh token.");

            var stored = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == request.RefreshToken && !t.Revocado);

            if (stored != null)
            {
                stored.Revocado = true;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}
