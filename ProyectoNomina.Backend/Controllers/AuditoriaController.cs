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
    // Por [Authorize] a nivel de clase, documentamos 401 y 403:
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class AuditoriaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditoriaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        // Códigos para este GET (no-CRUD): 200 / 400 / 500
        [ProducesResponseType(typeof(IEnumerable<AuditoriaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
