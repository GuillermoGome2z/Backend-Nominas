// 1️⃣ Modelo Auditoria.cs ya corregido
// 2️⃣ Ahora implementaremos el servicio y filtro paso a paso

using Microsoft.AspNetCore.Mvc.Filters;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using System.Security.Claims;

namespace ProyectoNomina.Backend.Filters
{
    public class AuditoriaActionFilter : IActionFilter
    {
        private readonly AppDbContext _context;

        public AuditoriaActionFilter(AppDbContext context)
        {
            _context = context;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Se ejecuta antes de que el controlador procese la solicitud
            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string nombreUsuario = "Anónimo";

            if (!string.IsNullOrEmpty(userId))
            {
                var usuarioDb = _context.Usuarios.FirstOrDefault(u => u.Id.ToString() == userId);
                if (usuarioDb != null)
                {
                    nombreUsuario = usuarioDb.NombreCompleto;
                }
            }
            var endpoint = context.HttpContext.Request.Path;
            var metodo = context.HttpContext.Request.Method;

            var detalles = $"Endpoint: {endpoint}, Método: {metodo}";

            var auditoria = new Auditoria
            {
                Accion = metodo,
                Usuario = nombreUsuario,
                Fecha = DateTime.Now,
                Detalles = detalles,
                Endpoint = endpoint,
                Metodo = metodo
            };

            _context.Auditoria.Add(auditoria);
            _context.SaveChanges();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No se utiliza por ahora
        }
    }
}
