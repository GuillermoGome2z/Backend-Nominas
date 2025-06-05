using ProyectoNomina.Backend.Models;
using ProyectoNomina.Backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

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
            var empleados = await _context.Empleados
    .Where(e => e.EstadoLaboral == "Activo")
    .Include(e => e.Deducciones)
    .Include(e => e.Bonificaciones)
    .ToListAsync();

            foreach (var empleado in empleados)
            {
                decimal salarioBase = empleado.SalarioMensual;
                decimal igss = salarioBase * 0.0483M;
                decimal bonificacionFija = 250M;

                decimal otrasDeducciones = empleado.Deducciones?.Sum(d => d.Monto) ?? 0M;
                decimal otrasBonificaciones = empleado.Bonificaciones?.Sum(b => b.Monto) ?? 0M;

                decimal totalDeducciones = igss + otrasDeducciones;
                decimal totalBonificaciones = bonificacionFija + otrasBonificaciones;
                decimal salarioNeto = salarioBase - totalDeducciones + totalBonificaciones;

                var detalle = new DetalleNomina
                {
                    EmpleadoId = empleado.Id,
                    SalarioBruto = salarioBase,
                    Deducciones = totalDeducciones,
                    Bonificaciones = totalBonificaciones,
                    SalarioNeto = salarioNeto,
                    DesgloseDeducciones = $"IGSS: Q{igss:F2}" +
                        (otrasDeducciones > 0 ? $", Otras: Q{otrasDeducciones:F2}" : "")
                };

                nomina.Detalles.Add(detalle);
            }

            _context.Nominas.Update(nomina); // Guardar los detalles dentro de la nómina existente
            await _context.SaveChangesAsync();
        }
    }
}
