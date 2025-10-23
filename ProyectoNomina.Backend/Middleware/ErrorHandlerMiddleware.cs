using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc; // Para ProblemDetails

namespace ProyectoNomina.Backend.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException oce) when (context.RequestAborted.IsCancellationRequested)
            {
                // El cliente canceló la solicitud: no es un error del servidor.
                _logger.LogInformation(oce, "⚠️ Solicitud cancelada por el cliente. Path: {Path}", context.Request.Path);
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = "application/problem+json";
                    var problem = BuildProblem(
                        status: StatusCodes.Status400BadRequest,
                        title: "Solicitud cancelada",
                        detail: "La operación fue cancelada por el cliente.",
                        instance: context.Request.Path,
                        traceId: context.TraceIdentifier,
                        includeDetail: _env.IsDevelopment()
                    );
                    await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions()));
                }
            }
            catch (Exception ex)
            {
                // Si ya empezó a escribir, no podemos formatear la respuesta: re-lanzar tras log
                if (context.Response.HasStarted)
                {
                    _logger.LogError(ex, "❌ Error no controlado después de iniciar la respuesta.");
                    throw;
                }

                _logger.LogError(ex, "❌ Error no controlado: {Message}", ex.Message);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                var problem = BuildProblem(
                    status: StatusCodes.Status500InternalServerError,
                    title: "Error interno del servidor",
                    detail: ex.Message,
                    instance: context.Request.Path,
                    traceId: context.TraceIdentifier,
                    includeDetail: _env.IsDevelopment()
                );

                await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions()));
            }
        }

        private static ProblemDetails BuildProblem(
            int status,
            string title,
            string? detail,
            string instance,
            string traceId,
            bool includeDetail)
        {
            // En producción podemos ocultar el detail para no filtrar info sensible
            var finalDetail = includeDetail ? detail : "Se produjo un error. Contacte al administrador.";
            var problem = new ProblemDetails
            {
                Type = "about:blank",
                Title = title,
                Status = status,
                Detail = finalDetail,
                Instance = instance
            };

            // Adjuntar traceId para correlación
            problem.Extensions["traceId"] = traceId;
            return problem;
        }

        private static JsonSerializerOptions JsonOptions() => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public static class ErrorHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalErrorHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}
