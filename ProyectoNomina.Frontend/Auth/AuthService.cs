using Blazored.LocalStorage;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProyectoNomina.Frontend.Auth
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;
        private readonly NavigationManager _navigation;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient http, ILocalStorageService localStorage, NavigationManager navigation, AuthenticationStateProvider authStateProvider)
        {
            _http = http;
            _localStorage = localStorage;
            _navigation = navigation;
            _authStateProvider = authStateProvider;
        }

        public async Task Login(string correo, string contraseña)
        {
            var response = await _http.PostAsJsonAsync("api/Auth/login", new { Correo = correo, Contraseña = contraseña });

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException("Credenciales incorrectas");

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            await _localStorage.SetItemAsync("jwtToken", result.Token);

            await ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);

            _navigation.NavigateTo("/");
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("jwtToken");
            await ((CustomAuthStateProvider)_authStateProvider).Logout();
            _navigation.NavigateTo("/login");
        }

        public class LoginResponse
        {
            public string Token { get; set; }
        }
    }
}
