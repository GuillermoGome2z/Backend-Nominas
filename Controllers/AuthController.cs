using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Backend.Services;
using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Backend.Controllers
{
    /// <summary>
    /// Controlador de autenticación y autorización de usuarios
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, JwtService jwtService, ILogger<AuthController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Autenticación de usuarios del sistema
        /// </summary>
        /// <param name="request">Datos de login (correo y contraseña)</param>
        /// <returns>Token JWT y información del usuario</returns>
        /// <response code="200">Login exitoso, retorna token JWT</response>
        /// <response code="400">Datos de entrada inválidos</response>
        /// <response code="401">Credenciales incorrectas</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validación básica del request
                if (request is null || string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Contraseña))
                {
                    _logger.LogWarning("Intento de login con datos inválidos desde IP: {IP}", HttpContext.Connection.RemoteIpAddress);
                    return BadRequest(new ProblemDetails 
                    { 
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Datos inválidos", 
                        Detail = "Correo y contraseña son requeridos.",
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    });
                }

                var usuario = await _context.Usuarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Correo == request.Correo, cancellationToken);

                // 401 para credenciales inválidas
                if (usuario is null)
                {
                    _logger.LogWarning("Intento de login con correo no encontrado: {Email}", request.Correo);
                    return Unauthorized(new ProblemDetails 
                    { 
                        Status = StatusCodes.Status401Unauthorized,
                        Title = "Credenciales inválidas", 
                        Detail = "El correo o la contraseña son incorrectos.",
                        Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
                    });
                }

                var passwordOk = BCrypt.Net.BCrypt.Verify(request.Contraseña, usuario.ClaveHash);
                if (!passwordOk)
                {
                    _logger.LogWarning("Intento de login con contraseña incorrecta para usuario: {Email}", request.Correo);
                    return Unauthorized(new ProblemDetails 
                    { 
                        Status = StatusCodes.Status401Unauthorized,
                        Title = "Credenciales inválidas", 
                        Detail = "El correo o la contraseña son incorrectos.",
                        Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
                    });
                }

                // Revocar tokens activos previos (política de un solo token activo)
                var tokensPrevios = await _context.RefreshTokens
                    .Where(t => t.UsuarioId == usuario.Id && !t.Revocado && t.Expira > DateTime.UtcNow)
                    .ToListAsync(cancellationToken);
                
                foreach (var token in tokensPrevios)
                {
                    token.Revocado = true;
                }

                // Generar nuevos tokens
                var jwt = _jwtService.GenerarToken(usuario);
                var refreshToken = new RefreshToken
                {
                    UsuarioId = usuario.Id,
                    Token = _jwtService.GenerarRefreshToken(),
                    Expira = DateTime.UtcNow.AddDays(7),
                    Revocado = false,
                    CreadoEn = DateTime.UtcNow
                };

                _context.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync(cancellationToken);

                // Exponer refresh token en header para CORS
                Response.Headers.Append("X-Refresh-Token", refreshToken.Token);

                _logger.LogInformation("Login exitoso para usuario: {Email}", usuario.Correo);

                return Ok(new LoginResponseDto
                {
                    Token = jwt,
                    NombreUsuario = usuario.NombreCompleto,
                    Rol = usuario.Rol
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno durante el login para usuario: {Email}", request.Correo);
                
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error procesando la solicitud. Intente nuevamente.",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                });
            }
        }

        /// <summary>
        /// Renovar token JWT usando refresh token
        /// </summary>
        /// <param name="request">Refresh token a renovar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Nuevo token JWT y refresh token</returns>
        /// <response code="200">Token renovado exitosamente</response>
        /// <response code="400">Refresh token requerido</response>
        /// <response code="401">Refresh token inválido o expirado</response>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RefreshResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RefreshResponseDto>> Refresh([FromBody] RefreshRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request is null || string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    _logger.LogWarning("Intento de refresh sin token desde IP: {IP}", HttpContext.Connection.RemoteIpAddress);
                    return BadRequest(new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Refresh token requerido",
                        Detail = "Se requiere un refresh token válido para renovar la sesión.",
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    });
                }

                var storedToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(t => t.Token == request.RefreshToken && !t.Revocado, cancellationToken);

                if (storedToken == null || storedToken.Expira < DateTime.UtcNow)
                {
                    _logger.LogWarning("Intento de refresh con token inválido o expirado: {Token}", request.RefreshToken[..8] + "...");
                    return Unauthorized(new ProblemDetails
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Title = "Refresh token inválido",
                        Detail = "El refresh token es inválido o ha expirado.",
                        Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
                    });
                }

                var usuario = await _context.Usuarios.FindAsync([storedToken.UsuarioId], cancellationToken);
                if (usuario == null)
                {
                    _logger.LogError("Usuario no encontrado para refresh token válido. UsuarioId: {UserId}", storedToken.UsuarioId);
                    return Unauthorized(new ProblemDetails
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Title = "Usuario no encontrado",
                        Detail = "El usuario asociado al token no existe.",
                        Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
                    });
                }

                // Rotación de tokens: revocar el actual y crear uno nuevo
                storedToken.Revocado = true;
                storedToken.RenovadoEn = DateTime.UtcNow;

                var nuevoRefreshToken = new RefreshToken
                {
                    UsuarioId = usuario.Id,
                    Token = _jwtService.GenerarRefreshToken(),
                    Expira = DateTime.UtcNow.AddDays(7),
                    Revocado = false,
                    CreadoEn = DateTime.UtcNow
                };

                _context.RefreshTokens.Add(nuevoRefreshToken);
                await _context.SaveChangesAsync(cancellationToken);

                var nuevoJwt = _jwtService.GenerarToken(usuario);

                _logger.LogInformation("Token renovado exitosamente para usuario: {Email}", usuario.Correo);

                return Ok(new RefreshResponseDto
                {
                    Token = nuevoJwt,
                    RefreshToken = nuevoRefreshToken.Token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno durante renovación de token");
                
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error procesando la renovación del token.",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                });
            }
        }

        /// <summary>
        /// Cerrar sesión del usuario revocando el refresh token
        /// </summary>
        /// <param name="request">Refresh token a revocar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>No content si es exitoso</returns>
        /// <response code="204">Logout exitoso</response>
        /// <response code="400">Refresh token requerido</response>
        [HttpPost("logout")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout([FromBody] RefreshRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request is null || string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    _logger.LogWarning("Intento de logout sin refresh token desde IP: {IP}", HttpContext.Connection.RemoteIpAddress);
                    return BadRequest(new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Refresh token requerido",
                        Detail = "Se requiere el refresh token para cerrar la sesión.",
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    });
                }

                var storedToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(t => t.Token == request.RefreshToken && !t.Revocado, cancellationToken);

                if (storedToken != null)
                {
                    storedToken.Revocado = true;
                    storedToken.RenovadoEn = DateTime.UtcNow;
                    await _context.SaveChangesAsync(cancellationToken);
                    
                    _logger.LogInformation("Logout exitoso para token: {Token}", request.RefreshToken[..8] + "...");
                }
                else
                {
                    _logger.LogWarning("Intento de logout con token no encontrado: {Token}", request.RefreshToken[..8] + "...");
                }

                // Retornar 204 sin importar si el token existía o no (por seguridad)
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno durante logout");
                
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error procesando el logout.",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                });
            }
        }
    }
}
