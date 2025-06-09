using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ProyectoNomina.Frontend;
using ProyectoNomina.Frontend.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using System.Net.Http.Headers;
using ProyectoNomina.Frontend.Services;

// ✅ Alias para evitar ambigüedad con AuthorizationMessageHandler
using LocalAuthHandler = ProyectoNomina.Frontend.Auth.AuthorizationMessageHandler;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ✅ Registrar Blazored.LocalStorage para manejar JWT
builder.Services.AddBlazoredLocalStorage();

// ✅ Registrar servicios de autenticación y autorización
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ✅ Registrar servicios personalizados
builder.Services.AddScoped<DepartamentoService>();
builder.Services.AddScoped<SesionUsuarioService>(); // ⬅️ Este es el que faltaba

// ✅ Registrar handler personalizado para incluir el token JWT en las peticiones
builder.Services.AddScoped<LocalAuthHandler>();

// ✅ Configurar HttpClient con el handler personalizado
builder.Services.AddScoped(sp =>
{
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var handler = new LocalAuthHandler(localStorage);

    var httpClient = new HttpClient(handler)
    {
        BaseAddress = new Uri("https://localhost:7187") 
    };

    httpClient.DefaultRequestHeaders.Accept.Clear();
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    return httpClient;
});

await builder.Build().RunAsync();
