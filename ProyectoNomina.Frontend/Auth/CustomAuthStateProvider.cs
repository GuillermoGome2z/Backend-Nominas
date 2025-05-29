using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProyectoNomina.Frontend.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly ClaimsPrincipal _anonimo = new(new ClaimsIdentity());

        public CustomAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("jwtToken");

            // 🔴 Si no hay token, usuario anónimo
            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(_anonimo);

            var handler = new JwtSecurityTokenHandler();

            JwtSecurityToken jwtToken;
            try
            {
                jwtToken = handler.ReadJwtToken(token);
            }
            catch
            {
                await _localStorage.RemoveItemAsync("jwtToken");
                return new AuthenticationState(_anonimo);
            }

            // 🔴 Si el token expiró, eliminar y devolver anónimo
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                await _localStorage.RemoveItemAsync("jwtToken");
                return new AuthenticationState(_anonimo);
            }

            var usuario = new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims, "jwt"));
            return new AuthenticationState(usuario);
        }

        public async Task NotifyUserAuthentication(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var usuario = new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims, "jwt"));

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(usuario)));
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("jwtToken");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonimo)));
        }
    }
}
