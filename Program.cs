using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Filters;
using System.Text;
using QuestPDF.Infrastructure;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http.Features;
using ProyectoNomina.Backend.Middleware;

namespace ProyectoNomina.Backend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configuración para desarrollo local - Puerto 5009
            builder.WebHost.UseUrls("http://localhost:5009");

            // Entity Framework - SQL Server
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // JWT Authentication
            var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key no configurado");
            var key = Encoding.ASCII.GetBytes(jwtKey);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:4200", "http://localhost:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // Services - Solo los que existen
            // builder.Services.AddScoped<NominaService>(); // Agregar cuando existan

            // Action Filter para auditoría
            builder.Services.AddScoped<AuditoriaActionFilter>();

            // Controllers
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<AuditoriaActionFilter>();
            });

            // File upload configuration
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
            });

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Proyecto Nominas API", Version = "v1" });
                
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            // Configure QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            // Build app
            var app = builder.Build();

            // Middleware
            app.UseMiddleware<ErrorHandlerMiddleware>();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            // Static files
            app.UseStaticFiles();

            app.MapControllers();

            // Health check endpoint
            app.MapGet("/health", () => "OK");

            // Auto-apply migrations en desarrollo
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                try
                {
                    await context.Database.EnsureCreatedAsync();
                    Console.WriteLine("✅ Base de datos inicializada correctamente");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error inicializando base de datos: {ex.Message}");
                }
            }

            Console.WriteLine("🚀 Proyecto Nóminas iniciado en http://localhost:5009");
            Console.WriteLine("📖 Swagger disponible en http://localhost:5009/swagger");
            
            await app.RunAsync();
        }
    }
}