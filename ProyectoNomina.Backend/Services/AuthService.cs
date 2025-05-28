using System.Net.Http.Json;
using ProyectoNomina.Client.Models;

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

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/Usuarios/login", request);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<LoginResponse>();

            return null;
        }
    }
}
