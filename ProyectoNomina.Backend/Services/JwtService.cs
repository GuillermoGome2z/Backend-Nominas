using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        public string GenerarToken(Usuario usuario, List<string> roles)
        {
            // ✅ 1. Crear lista de claims (datos incluidos en el token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Correo),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString())
            };

            // ✅ 2. Agregar roles como claims
            foreach (var rol in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol));
            }

            // ✅ 3. Crear la clave de firma a partir del appsettings
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // ✅ 4. Crear token con configuración
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: credentials
            );

            // ✅ 5. Devolver el token generado en formato string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

