using System.Net.Http.Json;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public AuthService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var response = await _http.PostAsJsonAsync("api/Usuarios/login", request);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<LoginResponseDto>();

            return null;
        }
    }
}
