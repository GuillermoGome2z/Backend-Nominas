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
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using ProyectoNomina.Backend.Middleware;
using ProyectoNomina.Backend.Options;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace ProyectoNomina.Backend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configuración de puerto para Railway
            if (builder.Environment.IsProduction())
            {
                var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
                builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
                Console.WriteLine($"=== Railway Production Mode - Listening on port {port} ===");
            }

            // Configuración QuestPDF (reportes PDF)
            QuestPDF.Settings.License = LicenseType.Community;
            
            // Configurar codificación UTF-8 para el sistema
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            // 1) DB - POSTGRESQL
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2) JWT (Railway-compatible config)
            var issuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer no configurado");
            var audience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience no configurado");
            var secretKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key no configurado");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ClockSkew = TimeSpan.Zero // endurece expiración JWT
                    };
                });

            // 3) CORS ESTRICTO (Railway-compatible)
            var corsOriginsConfig = builder.Configuration["Cors:AllowedOrigins"];
            var allowedOrigins = string.IsNullOrEmpty(corsOriginsConfig) 
                ? new[] { "http://localhost:5173" } // Desarrollo
                : corsOriginsConfig.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(o => o.Trim())
                                   .ToArray();

            // Azure Blob temporalmente deshabilitado - usando almacenamiento local
            // builder.Services.Configure<AzureBlobOptions>(builder.Configuration.GetSection("AzureBlob"));
            // builder.Services.AddSingleton<IFileStorageService, AzureBlobStorageService>();
            builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("cors", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials() // Habilitado para JWT en headers
                          // expone cabeceras para paginación/descarga y refresh
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

            // ====== NUEVO: servicios para historial de DetalleNomina ======
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();              // NUEVO
            builder.Services.AddScoped<IDetalleNominaAuditService, DetalleNominaAuditService>(); // NUEVO
            // ===============================================================

            // 5) MVC + filtro global + JSON (ignora ciclos si algún endpoint devuelve entidades)
            builder.Services.AddControllers(options =>
            {
                options.Filters.AddService<AuditoriaActionFilter>();
            })
            .AddJsonOptions(o =>
            {
                // Por si alguna entidad EF se expone (igual preferimos DTOs).
                o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });

            // Forzar 422 en errores de validación (front lo pide explícito)
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

            // 6) Swagger con JWT (el front usa OpenAPI para tipado/consumo)
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

                // Si luego agregas el filtro AuthorizeResponsesOperationFilter,
                // descomenta esta línea:
                // options.OperationFilter<AuthorizeResponsesOperationFilter>();
            });

            // 7) Subida de archivos (20 MB) coherente con Kestrel e IIS
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
            builder.Services.Configure<IISServerOptions>(o => o.MaxRequestBodySize = 20 * 1024 * 1024);

            var app = builder.Build();

            // AUTO-MIGRATION EN PRODUCCIÓN (Railway) - enfoque simple
            if (app.Environment.IsProduction())
            {
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    try 
                    {
                        Console.WriteLine("=== Verificando estado de la base de datos ===");
                        
                        // Intentar conectar a la base de datos
                        var canConnect = await db.Database.CanConnectAsync();
                        Console.WriteLine($"Conexión a BD: {(canConnect ? "OK" : "FALLÓ")}");

                        // Verificar migraciones pendientes
                        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
                        Console.WriteLine($"Migraciones pendientes: {pendingMigrations.Count()}");

                        if (pendingMigrations.Any())
                        {
                            Console.WriteLine("Aplicando migraciones...");
                            await db.Database.MigrateAsync();
                            Console.WriteLine("=== Migraciones aplicadas exitosamente ===");
                        }
                        else
                        {
                            Console.WriteLine("=== Base de datos actualizada ===");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error en migración: {ex.Message}");
                        
                        // Si las tablas ya existen, simplemente continuamos
                        if (ex.Message.Contains("already exists") || ex.Message.Contains("42P07"))
                        {
                            Console.WriteLine("=== Las tablas ya existen, continuando... ===");
                            // No hacer nada, las tablas están ahí y eso es lo importante
                        }
                        else
                        {
                            Console.WriteLine("=== Error crítico, pero continuando... ===");
                            // Continuar de todos modos para no impedir el inicio
                        }
                    }
                }
            }

            // 7.5) Seed initial data
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await ReglasLaboralesSeeder.SeedAsync(dbContext);
            }

            // 8) Forwarded Headers (si vas detrás de proxy; ajusta KnownProxies/Networks según tu infra)
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor,
                // KnownProxies = { IPAddress.Parse("10.0.0.10") },
                // KnownNetworks = { new IPNetwork(IPAddress.Parse("10.0.0.0"), 8) }
            });

            // 9) Error handling
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/error");
            }
            app.UseGlobalErrorHandler();

            // Middleware para convertir request demasiado grande en 413 con ProblemDetails
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Microsoft.AspNetCore.Http.BadHttpRequestException ex) when (ex.StatusCode == (int)HttpStatusCode.RequestEntityTooLarge)
                {
                    context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                    context.Response.ContentType = "application/problem+json";
                    var problem = new ProblemDetails
                    {
                        Status = StatusCodes.Status413PayloadTooLarge,
                        Title = "Archivo demasiado grande",
                        Detail = "El tamaño máximo permitido es 20 MB. Reduce el archivo e inténtalo de nuevo."
                    };
                    await context.Response.WriteAsJsonAsync(problem);
                }
            });

            // Swagger (si deseas ocultarlo en prod, colócalo dentro de if Development)
            app.UseSwagger();
            app.UseSwaggerUI();

            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCors("cors");

            //agregar 'Vary: Origin' justo antes de iniciar la respuesta
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

            app.UseAuthentication();
            app.UseAuthorization();

            // Crear carpeta Uploads/Expedientes si no existe
            var expedientesPath = Path.Combine(app.Environment.ContentRootPath, "Uploads", "Expedientes");
            if (!Directory.Exists(expedientesPath))
            {
                Directory.CreateDirectory(expedientesPath);
            }

            // 10) Endpoints
            app.MapControllers();

            // Health endpoint para Railway
            app.MapGet("/health", () => Results.Ok("OK"));
            app.MapGet("/", () => Results.Redirect("/swagger"));

            // Manejar argumentos de línea de comandos para migraciones
            if (args.Length > 0 && args[0] == "--migrate")
            {
                Console.WriteLine("=== Aplicando migraciones de base de datos ===");
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                try
                {
                    await dbContext.Database.MigrateAsync();
                    Console.WriteLine("=== Migraciones aplicadas exitosamente ===");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error aplicando migraciones: {ex.Message}");
                    throw;
                }
                return;
            }

            // Auto-migrar en Railway si no es desarrollo
            if (!app.Environment.IsDevelopment())
            {
                Console.WriteLine("=== Verificando y aplicando migraciones automáticamente ===");
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                try
                {
                    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
                    if (pendingMigrations.Any())
                    {
                        Console.WriteLine($"Aplicando {pendingMigrations.Count()} migraciones pendientes...");
                        await dbContext.Database.MigrateAsync();
                        Console.WriteLine("=== Migraciones aplicadas exitosamente ===");
                    }
                    else
                    {
                        Console.WriteLine("No hay migraciones pendientes");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en auto-migración: {ex.Message}");
                    // No lanzar la excepción para permitir que la app se inicie
                }
            }

            app.Run();
        }
    }
}
