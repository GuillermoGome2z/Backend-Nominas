using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;


namespace ProyectoNomina.Client.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;

        private ClaimsPrincipal _anonimo = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // 🔐 Obtener el token desde localStorage
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwtToken");

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(_anonimo);

            // 🔍 Validar si el token ha expirado
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                // Token expirado
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "jwtToken");
                return new AuthenticationState(_anonimo);
            }

            // ✅ Crear ClaimsPrincipal a partir del token
            var claims = jwtToken.Claims;
            var usuario = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

            return new AuthenticationState(usuario);
        }

        public async Task NotificarCambioEstadoAutenticacion(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Claims;
            var usuario = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

            // 🔔 Notificar a Blazor que cambió el estado de autenticación
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(usuario)));
        }

        public void NotificarLogout()
        {
            // 🔔 Notificar que ya no hay usuario autenticado
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonimo)));
        }
    }
}
