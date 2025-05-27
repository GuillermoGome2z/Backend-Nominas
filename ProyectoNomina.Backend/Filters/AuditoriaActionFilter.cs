using Microsoft.AspNetCore.Mvc.Filters;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using System.Security.Claims;

namespace ProyectoNomina.Backend.Filters
{
    public class AuditoriaActionFilter : IAsyncActionFilter
    {
        private readonly AppDbContext _context;

        public AuditoriaActionFilter(AppDbContext context)
        {
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Continuar la ejecución primero
            var resultado = await next();

            // Si la respuesta es exitosa (200-299)
            if (resultado.Result is Microsoft.AspNetCore.Mvc.ObjectResult objectResult &&
                objectResult.StatusCode >= 200 && objectResult.StatusCode < 300)
            {
                var usuario = context.HttpContext.User.Identity?.Name ?? "Anónimo";

                var metodo = context.HttpContext.Request.Method;
                var endpoint = context.HttpContext.Request.Path;

                var auditoria = new Auditoria
                {
                    Usuario = usuario,
                    Accion = metodo,
                    Fecha = DateTime.Now,
                    Detalles = $"Accedió a {endpoint}"
                };

                _context.Auditorias.Add(auditoria);
                await _context.SaveChangesAsync();
            }
        }
    }
}
