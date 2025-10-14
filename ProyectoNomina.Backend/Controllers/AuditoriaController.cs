using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditoriaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditoriaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuditoriaDto>>> GetAuditoria()
        {
            var auditoria = await _context.Auditoria
                .OrderByDescending(a => a.Fecha)
                .Select(a => new AuditoriaDto
                {
                    Id = a.Id,
                    Usuario = a.Usuario,
                    Accion = a.Accion,
                    Fecha = a.Fecha,
                    Detalles = a.Detalles,
                    Endpoint = a.Endpoint,
                    Metodo = a.Metodo
                })
                .ToListAsync();

            return Ok(auditoria);
        }
    }
}
