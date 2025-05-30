using ProyectoNomina.Backend.Models;
using ProyectoNomina.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace ProyectoNomina.Backend.Services
{
    public class NominaService
    {
        private readonly AppDbContext _context;

        public NominaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task Calcular(Nomina nomina)
        {
            var empleados = await _context.Empleados.ToListAsync();

            foreach (var empleado in empleados)
            {
                var salarioBase = empleado.SalarioMensual;
                var igss = salarioBase * 0.0483M;
                var bonificacion = 250M;
                var salarioNeto = salarioBase - igss + bonificacion;

                var detalle = new DetalleNomina
                {
                    EmpleadoId = empleado.Id,
                    SalarioBruto = salarioBase,
                    Deducciones = igss,
                    Bonificaciones = bonificacion,
                    SalarioNeto = salarioNeto,
                    DesgloseDeducciones = $"IGSS: Q{igss:F2}"
                };

                nomina.Detalles.Add(detalle);
            }
        }
    }
}
