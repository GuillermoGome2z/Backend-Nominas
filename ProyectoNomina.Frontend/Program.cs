using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ProyectoNomina.Frontend;
using ProyectoNomina.Frontend.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using System.Net.Http.Headers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 🌐 BaseAddress del backend
builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7187") // 🔗 URL del backend
    };

    httpClient.DefaultRequestHeaders.Accept.Clear();
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    return httpClient;
});

// ✅ Blazored.LocalStorage para JWT
builder.Services.AddBlazoredLocalStorage();

// ✅ Autenticación
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// ✅ Registro correcto del servicio de autenticación
builder.Services.AddScoped<IAuthService, AuthService>(); // 🔧 esta línea estaba mal

await builder.Build().RunAsync();
