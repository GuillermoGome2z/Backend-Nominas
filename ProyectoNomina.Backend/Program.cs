using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Services;
using ProyectoNomina.Backend.Filters;
using System.Text;
using QuestPDF.Infrastructure;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features; // ← agregado para multipart
using ProyectoNomina.Backend.Middleware;
using ProyectoNomina.Backend.Options;

namespace ProyectoNomina.Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            QuestPDF.Settings.License = LicenseType.Community;

            // 1) DB
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2) JWT
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey no configurado");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                    };
                });

            // 3) CORS (leer orígenes desde appsettings)
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                                 ?? new[] { "http://localhost:5173" }; // Vite por defecto

                                 builder.Services.Configure<AzureBlobOptions>(builder.Configuration.GetSection("AzureBlob"));
builder.Services.AddSingleton<IFileStorageService, AzureBlobStorageService>();


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          // Exponer cabeceras útiles (descargas, paginación, refresh token)
                          .WithExposedHeaders("Content-Disposition", "X-Refresh-Token", "X-Total-Count");
                });
            });

            // 4) Servicios
            builder.Services.AddScoped<JwtService>();
            builder.Services.AddScoped<NominaService>();
            builder.Services.AddScoped<ReporteService>();
            builder.Services.AddScoped<AuditoriaService>();
            builder.Services.AddScoped<AuditoriaActionFilter>();
            builder.Services.AddHttpContextAccessor();

            // 5) MVC + filtro global
            builder.Services.AddControllers(options =>
            {
                options.Filters.AddService<AuditoriaActionFilter>();
            });

            // Forzar 422 en errores de validación 
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Status = StatusCodes.Status422UnprocessableEntity,
                        Type = "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2",
                        Title = "La solicitud no pudo ser procesada por errores de validación.",
                        Detail = "Revisa los errores por campo."
                    };

                    return new UnprocessableEntityObjectResult(problemDetails);
                };
            });

            // 6) Swagger con JWT
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "ProyectoNomina", Version = "v1" });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Escribe: Bearer {tu token JWT}"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // 🟢 NUEVO BLOQUE - soporte para carga de archivos grandes y límite de 20 MB
            builder.Services.Configure<FormOptions>(o =>
            {
                o.MultipartBodyLengthLimit = 20 * 1024 * 1024; // 20 MB
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartHeadersLengthLimit = 64 * 1024;
            });

            builder.WebHost.ConfigureKestrel(k =>
            {
                k.Limits.MaxRequestBodySize = 20 * 1024 * 1024; // 20 MB
            });
            // 🟢 FIN BLOQUE NUEVO

            var app = builder.Build();

            // ---Forwarded Headers (útil en producción detrás de proxy) ---
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
            });
            // ---------------------------------------------------------------------

            // 7) Middleware

            // Middleware global de manejo de errores 
            app.UseGlobalErrorHandler();

            app.UseSwagger();
            app.UseSwaggerUI();

            // --- HSTS solo fuera de Development ---
            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }
            // ---------------------------------------------

            app.UseHttpsRedirection();

            // app.UseStaticFiles();  // ← no necesario, manejas descargas vía endpoints seguros

            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            // - crear carpeta de Uploads/Expedientes si no existe
            var expedientesPath = Path.Combine(app.Environment.ContentRootPath, "Uploads", "Expedientes");
            if (!Directory.Exists(expedientesPath))
            {
                Directory.CreateDirectory(expedientesPath);
            }
            

            // 8) Endpoints básicos
            app.MapControllers();

            // Health y redirección a swagger
            app.MapGet("/health", () => Results.Ok(new { ok = true, time = DateTime.UtcNow }));
            app.MapGet("/", () => Results.Redirect("/swagger"));

            // Ejecutar seed runner en runtime 
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                ProyectoNomina.Backend.Data.DataSeeder.SeedAsync(db).GetAwaiter().GetResult();
            }

            app.Run();
        }
    }
}
