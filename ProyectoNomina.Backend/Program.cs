using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Services;
using ProyectoNomina.Backend.Filters;
using System.Text;

namespace ProyectoNomina.Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1️⃣ Conexión a Base de Datos
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2️⃣ Configuración de JWT (Json Web Token) para autenticación
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true, // ✅ Verifica el emisor del token
                        ValidateAudience = true, // ✅ Verifica el receptor del token
                        ValidateLifetime = true, // ✅ Verifica si el token aún no ha expirado
                        ValidateIssuerSigningKey = true, // ✅ Verifica la clave del token
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                    };
                });

            // 3️⃣ Servicios personalizados
            builder.Services.AddScoped<JwtService>();            // Servicio para generar y validar JWT
            builder.Services.AddScoped<NominaService>();         // Servicio para cálculos de nómina
            builder.Services.AddScoped<ReporteService>();        // Servicio para generar reportes PDF
            builder.Services.AddScoped<AuditoriaService>();      // Servicio para registrar acciones de auditoría
            builder.Services.AddScoped<AuditoriaActionFilter>(); // Filtro que registra auditoría por cada acción
            builder.Services.AddHttpContextAccessor();           // Necesario para acceder al usuario actual

            // 4️⃣ Agregar filtro global para registrar auditoría de todas las acciones
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<AuditoriaActionFilter>();
            });

            // 5️⃣ Configurar Swagger con soporte para JWT
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new() { Title = "ProyectoNomina", Version = "v1" });

                // Configuración de esquema de seguridad JWT
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Escribe: Bearer {tu token JWT}"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // 6️⃣ Configuración del pipeline de middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();     // Habilita Swagger en desarrollo
                app.UseSwaggerUI();  // Interfaz visual de Swagger
            }

            app.UseHttpsRedirection();  // Redirige HTTP a HTTPS

            app.UseAuthentication();    // Middleware de autenticación JWT
            app.UseAuthorization();     // Middleware de autorización (roles y políticas)

            app.MapControllers();       // Mapea los controladores a rutas
            app.Run();                  // Inicia la aplicación
        }
    }
}


