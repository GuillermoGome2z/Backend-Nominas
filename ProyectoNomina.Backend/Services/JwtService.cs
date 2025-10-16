using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ProyectoNomina.Backend.Models;

namespace ProyectoNomina.Backend.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Genera el JWT principal para el usuario autenticado.
        /// Expiración configurable en JwtSettings:ExpirationHours (por defecto 1 hora).
        /// Incluye claims estándar (sub, jti, iat) y el rol del usuario.
        /// </summary>
        public string GenerarToken(Usuario usuario)
        {
            if (usuario is null)
                throw new ArgumentNullException(nameof(usuario));

            var issuer = _configuration["JwtSettings:Issuer"]
                ?? throw new InvalidOperationException("JwtSettings:Issuer no configurado");
            var audience = _configuration["JwtSettings:Audience"]
                ?? throw new InvalidOperationException("JwtSettings:Audience no configurado");
            var secret = _configuration["JwtSettings:SecretKey"]
                ?? throw new InvalidOperationException("JwtSettings:SecretKey no configurado");

            // Recomendación: secret de al menos 32 chars para HMAC-SHA256
            if (secret.Length < 32)
                throw new InvalidOperationException("JwtSettings:SecretKey debe tener al menos 32 caracteres.");

            var now = DateTime.UtcNow;
            var expirationHours = GetExpirationHours(_configuration["JwtSettings:ExpirationHours"]);

            var claims = new List<Claim>
            {
                // Estándar
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpoch(now).ToString(), ClaimValueTypes.Integer64),

                // Identidad del usuario
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto ?? string.Empty),
                new Claim(ClaimTypes.Email, usuario.Correo ?? string.Empty),

                // Rol (tu modelo expone un único rol string)
                new Claim(ClaimTypes.Role, usuario.Rol ?? string.Empty)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,                                      // nbf
                expires: now.AddHours(expirationHours),             // exp
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Genera un refresh token aleatorio y seguro (64 bytes -> Base64).
        /// </summary>
        public string GenerarRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        // ----------------- Helpers -----------------

        private static long ToUnixEpoch(DateTime dateTimeUtc)
            => (long)Math.Round((dateTimeUtc - DateTime.UnixEpoch).TotalSeconds);

        private static double GetExpirationHours(string? raw)
        {
            // Default 1 hora si no está configurado o es inválido
            if (double.TryParse(raw, out var hours) && hours > 0 && hours <= 24 * 7)
                return hours;

            return 1d;
        }
    }
}
