using ProyectoNomina.Shared.Models.DTOs;
using System.Net.Http.Json;

namespace ProyectoNomina.Frontend.Services
{
    public class DepartamentoService
    {
        private readonly HttpClient _http;

        public DepartamentoService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<DepartamentoDto>> ObtenerDepartamentos()
        {
            return await _http.GetFromJsonAsync<List<DepartamentoDto>>("api/Departamentos") ?? new();
        }
    }
}
