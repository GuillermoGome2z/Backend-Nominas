using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Shared.Models.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace ProyectoNomina.Backend.Controllers
{
    [Authorize(Roles = "Admin,RRHH")]
    [ApiController]
    [Route("api/[controller]")]
    public class EmpleadosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmpleadosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmpleadoDto>>> GetEmpleados()
        {
            var empleados = await _context.Empleados
                .Include(e => e.Departamento)
                .Include(e => e.Puesto)
                .Select(e => new EmpleadoDto
                {
                    Id = e.Id,
                    NombreCompleto = e.NombreCompleto,
                    DPI = e.DPI,
                    NIT = e.NIT,
                    Direccion = e.Direccion,
                    Telefono = e.Telefono,
                    FechaContratacion = e.FechaContratacion,
                    SalarioMensual = e.SalarioMensual,
                    DepartamentoId = e.DepartamentoId,
                    PuestoId = e.PuestoId,
                    NombreDepartamento = e.Departamento!.Nombre,
                    NombrePuesto = e.Puesto!.Nombre
                })
                .ToListAsync();

            return Ok(empleados);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmpleadoDto>> GetEmpleado(int id)
        {
            var e = await _context.Empleados
                .Include(e => e.Departamento)
                .Include(e => e.Puesto)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (e == null) return NotFound();

            var dto = new EmpleadoDto
            {
                Id = e.Id,
                NombreCompleto = e.NombreCompleto,
                DPI = e.DPI,
                NIT = e.NIT,
                Direccion = e.Direccion,
                Telefono = e.Telefono,
                FechaContratacion = e.FechaContratacion,
                SalarioMensual = e.SalarioMensual,
                DepartamentoId = e.DepartamentoId,
                PuestoId = e.PuestoId,
                NombreDepartamento = e.Departamento?.Nombre,
                NombrePuesto = e.Puesto?.Nombre
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> PostEmpleado([FromBody] EmpleadoCreacionDto dto)
        {
            var nuevo = new Empleado
            {
                NombreCompleto = dto.NombreCompleto,
                Correo = dto.Correo,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                SalarioMensual = dto.SalarioBase, // SalarioBase viene del DTO
                DepartamentoId = dto.DepartamentoId,
                PuestoId = dto.PuestoId,
                FechaContratacion = dto.FechaContratacion,

                // ✅ Estos campos son obligatorios
                DPI = dto.DPI,
                NIT = dto.NIT
            };

            _context.Empleados.Add(nuevo);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Empleado creado correctamente", id = nuevo.Id });
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmpleado(int id, EmpleadoDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return NotFound();

            empleado.NombreCompleto = dto.NombreCompleto;
            empleado.DPI = dto.DPI;
            empleado.NIT = dto.NIT;
            empleado.Direccion = dto.Direccion;
            empleado.Telefono = dto.Telefono;
            empleado.FechaContratacion = dto.FechaContratacion;
            empleado.SalarioMensual = dto.SalarioMensual;
            empleado.DepartamentoId = dto.DepartamentoId;
            empleado.PuestoId = dto.PuestoId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmpleado(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return NotFound();

            _context.Empleados.Remove(empleado);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}