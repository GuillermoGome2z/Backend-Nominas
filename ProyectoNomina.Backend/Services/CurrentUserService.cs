using System.Security.Claims;

namespace ProyectoNomina.Backend.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _http;
        public CurrentUserService(IHttpContextAccessor http) => _http = http;

        public string? GetUserId()
        {
            var user = _http.HttpContext?.User;
            if (user == null || !(user.Identity?.IsAuthenticated ?? false)) return null;

            return user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? user.FindFirstValue(ClaimTypes.Email)
                ?? user.Identity?.Name;
        }
    }
}

