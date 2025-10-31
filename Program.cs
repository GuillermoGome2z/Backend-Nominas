using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Services;
using ProyectoNomina.Backend.Services.Reportes;
using ProyectoNomina.Backend.Filters;
using System.Text;
using QuestPDF.Infrastructure;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using ProyectoNomina.Backend.Middleware;
using ProyectoNomina.Backend.Options;

namespace ProyectoNomina.Backend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // En producción (Railway) escucha en el puerto asignado por el entorno
            if (builder.Environment.IsProduction())
            {
                var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
                builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
                Console.WriteLine($"=== Production - Listening on {port} ===");
            }

            // QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // ***** CAMBIO NECESARIO: construir la cadena de conexión *****
            string BuildPgConnFromEnvOrConfig(WebApplicationBuilder b)
            {
                var url = Environment.GetEnvironmentVariable("DATABASE_URL");
                if (!string.IsNullOrWhiteSpace(url))
                {
                    // Ej: postgresql://user:pass@host:port/dbname
                    var uri = new Uri(url);
                    var userInfo = uri.UserInfo.Split(':', 2);

                    var npg = new Npgsql.NpgsqlConnectionStringBuilder
                    {
                        Host = uri.Host,
                        Port = uri.Port,
                        Database = uri.AbsolutePath.Trim('/'),
                        Username = userInfo[0],
                        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "",
                        SslMode = Npgsql.SslMode.Require,
                        TrustServerCertificate = true
                    };
                    return npg.ToString();
                }

                // Fallback para desarrollo local
                var fromConfig = b.Configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(fromConfig))
                    throw new InvalidOperationException(
                        "No hay cadena de conexión. Define DATABASE_URL en Railway o ConnectionStrings:DefaultConnection para local.");
                return fromConfig;
            }

            var pgConn = BuildPgConnFromEnvOrConfig(builder);

            // 1) DB (PostgreSQL)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(pgConn));

            // 2) JWT
            var issuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer no configurado");
            var audience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience no configurado");
            var secretKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key no configurado");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // 3) CORS (acepta string o array en Cors:AllowedOrigins)
            string[] allowedOrigins;
            var arr = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            if (arr != null && arr.Length > 0)
                allowedOrigins = arr;
            else
            {
                var s = builder.Configuration["Cors:AllowedOrigins"];
                allowedOrigins = string.IsNullOrWhiteSpace(s)
                    ? new[] { "http://localhost:5173" }
                    : s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }

            builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("cors", p =>
                {
                    p.WithOrigins(allowedOrigins)
                     .AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials()
                     .WithExposedHeaders("Content-Disposition", "X-Refresh-Token", "X-Total-Count");
                });
            });

            // 4) Servicios
            builder.Services.AddScoped<JwtService>();
            builder.Services.AddScoped<NominaService>();
            builder.Services.AddScoped<IPayrollService, PayrollService>();
            builder.Services.AddScoped<ReporteService>();
            builder.Services.AddScoped<ExpedientesReportService>();
            builder.Services.AddScoped<AuditoriaService>();
            builder.Services.AddScoped<AuditoriaActionFilter>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<IDetalleNominaAuditService, DetalleNominaAuditService>();

            // 5) MVC + JSON
            builder.Services.AddControllers(o =>
            {
                o.Filters.AddService<AuditoriaActionFilter>();
            })
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.ReferenceHandler =
                    System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problem = new ValidationProblemDetails(context.ModelState)
                    {
                        Status = StatusCodes.Status422UnprocessableEntity,
                        Type = "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2",
                        Title = "Errores de validación",
                        Detail = "Revisa los errores por campo."
                    };
                    return new UnprocessableEntityObjectResult(problem);
                };
            });

            // 6) Swagger + JWT
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo { Title = "ProyectoNomina", Version = "v1" });
                o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Escribe: Bearer {tu token JWT}"
                });
                o.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme, Id = "Bearer"
                            }
                        }, Array.Empty<string>()
                    }
                });
            });

            // 7) Límites de subida (20MB)
            builder.Services.Configure<FormOptions>(o =>
            {
                o.MultipartBodyLengthLimit = 20 * 1024 * 1024;
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartHeadersLengthLimit = 64 * 1024;
            });

            var app = builder.Build();

            // Migraciones automáticas (una sola vez al arranque)
            if (app.Environment.IsProduction())
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                try
                {
                    Console.WriteLine("=== DB: aplicando migraciones (si hay) ===");
                    await db.Database.MigrateAsync();
                }
                catch (Exception ex)
                {
                    // Ignora si ya existen tablas
                    if (ex.Message.Contains("already exists") || ex.Message.Contains("42P07"))
                        Console.WriteLine("Tablas ya existen, continuando…");
                    else
                        Console.WriteLine($"Aviso migraciones: {ex.Message}");
                }
            }

            // Seed fijo (reglas laborales)
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await ReglasLaboralesSeeder.SeedAsync(dbContext);
            }

            // Middlewares
            app.UseSwagger();
            app.UseSwaggerUI();

            if (app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }
            else
            {
                app.UseHsts();
            }

            // Vary: Origin (mejor CORS caching)
            app.Use(async (ctx, next) =>
            {
                ctx.Response.OnStarting(() =>
                {
                    if (!ctx.Response.Headers.ContainsKey("Vary"))
                        ctx.Response.Headers.Append("Vary", "Origin");
                    return Task.CompletedTask;
                });
                await next();
            });

            app.UseCors("cors");
            app.UseAuthentication();
            app.UseAuthorization();

            // carpeta Uploads/Expedientes
            var expedientesPath = Path.Combine(app.Environment.ContentRootPath, "Uploads", "Expedientes");
            Directory.CreateDirectory(expedientesPath);

            // Endpoints
            app.MapControllers();
            app.MapGet("/health", () => Results.Ok("OK"));
            app.MapGet("/", () => Results.Redirect("/swagger"));

            app.Run();
        }
    }
}
