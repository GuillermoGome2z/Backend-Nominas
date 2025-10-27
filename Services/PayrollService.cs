using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Shared.Models.DTOs;
using System.Text.Json;

namespace ProyectoNomina.Backend.Services
{
    public interface IPayrollService
    {
        Task<PayrollPreviewDto> SimularAsync(PeriodoNominaInput input);
        Task<Nomina> ProcesarAsync(PeriodoNominaInput input);
        Task<Nomina> RecalcularAsync(int nominaId);
        Task CerrarAsync(int nominaId);
    }

    public class PayrollService : IPayrollService
    {
        private readonly AppDbContext _context;
        private const decimal IGSS_MAXIMO_BASE = 5000m; // Q5,000 máximo para IGSS
        private const decimal AGUINALDO_BONO14_EXENCION_ANUAL = 60000m; // Q60,000 anuales

        public PayrollService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PayrollPreviewDto> SimularAsync(PeriodoNominaInput input)
        {
            var reglas = await ObtenerReglasVigentesAsync();
            var empleados = await ObtenerEmpleadosFiltradosAsync(input);

            var preview = new PayrollPreviewDto
            {
                Periodo = input.Periodo,
                TipoNomina = input.TipoNomina,
                FechaCorte = input.FechaCorte ?? DateTime.Today,
                Empleados = new List<EmpleadoNominaDto>()
            };

            decimal totalBruto = 0, totalDeducciones = 0, totalNeto = 0;

            foreach (var empleado in empleados)
            {
                var empleadoDto = await CalcularEmpleadoAsync(empleado, input, reglas);
                preview.Empleados.Add(empleadoDto);

                totalBruto += empleadoDto.SalarioBruto;
                totalDeducciones += empleadoDto.TotalDeducciones;
                totalNeto += empleadoDto.SalarioNeto;
            }

            preview.TotalBruto = totalBruto;
            preview.TotalDeducciones = totalDeducciones;
            preview.TotalNeto = totalNeto;
            preview.CantidadEmpleados = empleados.Count;

            return preview;
        }

        public async Task<Nomina> ProcesarAsync(PeriodoNominaInput input)
        {
            // Validar que no exista nómina CERRADA para el mismo período/tipo
            var existeCerrada = await _context.Nominas
                .AnyAsync(n => n.Periodo == input.Periodo 
                            && n.TipoNomina == input.TipoNomina 
                            && n.Estado == "CERRADA");

            if (existeCerrada)
                throw new InvalidOperationException($"Ya existe una nómina CERRADA para {input.TipoNomina} {input.Periodo}");

            // Buscar si ya existe una nómina BORRADOR
            var nominaExistente = await _context.Nominas
                .Include(n => n.DetallesNomina)
                .Include(n => n.AportesPatronales)
                .FirstOrDefaultAsync(n => n.Periodo == input.Periodo 
                                       && n.TipoNomina == input.TipoNomina 
                                       && n.Estado == "BORRADOR");

            Nomina nomina;
            if (nominaExistente != null)
            {
                // Limpiar detalles existentes
                _context.DetalleNominas.RemoveRange(nominaExistente.DetallesNomina);
                if (nominaExistente.AportesPatronales != null)
                    _context.NominaAportesPatronales.Remove(nominaExistente.AportesPatronales);
                
                nomina = nominaExistente;
            }
            else
            {
                // Crear nueva nómina
                nomina = new Nomina
                {
                    Periodo = input.Periodo,
                    TipoNomina = input.TipoNomina,
                    Descripcion = $"Nómina {input.TipoNomina} - {input.Periodo}",
                    FechaGeneracion = DateTime.Now,
                    FechaInicio = input.FechaInicio,
                    FechaFin = input.FechaFin,
                    FechaCorte = input.FechaCorte ?? DateTime.Today,
                    Estado = "BORRADOR",
                    DetallesNomina = new List<DetalleNomina>()
                };
                _context.Nominas.Add(nomina);
            }

            // Simular y procesar
            var preview = await SimularAsync(input);
            
            // Convertir preview a entidades
            foreach (var empleadoDto in preview.Empleados)
            {
                var detalle = new DetalleNomina
                {
                    EmpleadoId = empleadoDto.EmpleadoId,
                    SalarioBruto = empleadoDto.SalarioBruto,
                    Bonificaciones = empleadoDto.Bonificaciones,
                    IgssEmpleado = empleadoDto.IgssEmpleado,
                    Isr = empleadoDto.Isr,
                    Prestamos = empleadoDto.Prestamos,
                    Anticipos = empleadoDto.Anticipos,
                    OtrasDeducciones = empleadoDto.OtrasDeducciones,
                    Deducciones = empleadoDto.TotalDeducciones,
                    SalarioNeto = empleadoDto.SalarioNeto,
                    DesgloseDeducciones = $"IGSS: Q{empleadoDto.IgssEmpleado:F2}, ISR: Q{empleadoDto.Isr:F2}"
                };
                
                nomina.DetallesNomina.Add(detalle);
            }

            // Actualizar totales de la nómina
            nomina.TotalBruto = preview.TotalBruto;
            nomina.TotalDeducciones = preview.TotalDeducciones;
            nomina.TotalNeto = preview.TotalNeto;
            nomina.MontoTotal = preview.TotalNeto;
            nomina.CantidadEmpleados = preview.CantidadEmpleados;

            // Calcular aportes patronales
            await CalcularAportesPatronalesAsync(nomina, input);

            await _context.SaveChangesAsync();
            return nomina;
        }

        public async Task<Nomina> RecalcularAsync(int nominaId)
        {
            var nomina = await _context.Nominas
                .Include(n => n.DetallesNomina)
                .FirstOrDefaultAsync(n => n.Id == nominaId);

            if (nomina == null)
                throw new ArgumentException("Nómina no encontrada");

            if (nomina.Estado == "CERRADA")
                throw new InvalidOperationException("No se puede recalcular una nómina CERRADA");

            var input = new PeriodoNominaInput
            {
                Periodo = nomina.Periodo!,
                TipoNomina = nomina.TipoNomina,
                FechaInicio = nomina.FechaInicio ?? DateTime.Now,
                FechaFin = nomina.FechaFin ?? DateTime.Now,
                FechaCorte = nomina.FechaCorte
            };

            return await ProcesarAsync(input);
        }

        public async Task CerrarAsync(int nominaId)
        {
            var nomina = await _context.Nominas.FindAsync(nominaId);
            
            if (nomina == null)
                throw new ArgumentException("Nómina no encontrada");

            if (nomina.Estado == "CERRADA")
                throw new InvalidOperationException("La nómina ya está CERRADA");

            nomina.Estado = "CERRADA";
            nomina.FechaAprobacion = DateTime.Now;
            nomina.AprobadoPor = "System"; // TODO: Obtener usuario actual

            await _context.SaveChangesAsync();
        }

        private async Task<EmpleadoNominaDto> CalcularEmpleadoAsync(Empleado empleado, PeriodoNominaInput input, ReglasLaborales reglas)
        {
            // 1. DETERMINAR BASE DE CÁLCULO
            decimal baseSalario = await ObtenerBaseSalarioAsync(empleado, input);

            // 2. CALCULAR BONO DECRETO (solo nóminas ordinarias)
            decimal bonoDecreto = (input.TipoNomina == "ORDINARIA") ? reglas.BonoDecretoMonto : 0m;

            // 3. CALCULAR BONIFICACIONES ADICIONALES
            decimal otrasBonificaciones = await ObtenerBonificacionesAsync(empleado.Id);

            decimal salarioBruto = baseSalario + bonoDecreto + otrasBonificaciones;

            // 4. CALCULAR IGSS (CON LÍMITE MÁXIMO Y EXENCIONES)
            decimal igssEmpleado = CalcularIgssEmpleado(salarioBruto, input.TipoNomina, reglas.IgssEmpleadoPct);

            // 5. CALCULAR ISR (CON EXENCIONES ESPECIALES)
            decimal isr = await CalcularIsrAsync(empleado, salarioBruto, igssEmpleado, input, reglas);

            // 6. OTRAS DEDUCCIONES
            decimal prestamos = await ObtenerPrestamosAsync(empleado.Id);
            decimal anticipos = await ObtenerAnticiposAsync(empleado.Id);
            decimal otrasDeducciones = await ObtenerOtrasDeduccionesAsync(empleado.Id);

            decimal totalDeducciones = igssEmpleado + isr + prestamos + anticipos + otrasDeducciones;
            decimal salarioNeto = salarioBruto - totalDeducciones;

            return new EmpleadoNominaDto
            {
                EmpleadoId = empleado.Id,
                NombreCompleto = empleado.NombreCompleto,
                Departamento = empleado.Departamento?.Nombre,
                Puesto = empleado.Puesto?.Nombre,
                SalarioBruto = salarioBruto,
                Bonificaciones = bonoDecreto + otrasBonificaciones,
                IgssEmpleado = igssEmpleado,
                Isr = isr,
                Prestamos = prestamos,
                Anticipos = anticipos,
                OtrasDeducciones = otrasDeducciones,
                TotalDeducciones = totalDeducciones,
                SalarioNeto = salarioNeto
            };
        }

        /// <summary>
        /// REGLA CRÍTICA: IGSS con límite máximo de Q5,000 y exenciones
        /// </summary>
        private decimal CalcularIgssEmpleado(decimal salarioBruto, string tipoNomina, decimal igssPct)
        {
            // EXENCIÓN CRÍTICA: Aguinaldo y Bono14 NO pagan IGSS
            if (tipoNomina == "AGUINALDO" || tipoNomina == "BONO14")
                return 0m;

            // Aplicar límite máximo de Q5,000
            decimal baseIgss = Math.Min(salarioBruto, IGSS_MAXIMO_BASE);
            return Math.Round(baseIgss * igssPct, 2);
        }

        /// <summary>
        /// REGLA CRÍTICA: Base de cálculo según tipo de nómina
        /// </summary>
        private async Task<decimal> ObtenerBaseSalarioAsync(Empleado empleado, PeriodoNominaInput input)
        {
            // Aguinaldo y Bono14: promedio últimos 12 meses
            if (input.TipoNomina == "AGUINALDO" || input.TipoNomina == "BONO14")
            {
                return await CalcularPromedio12MesesAsync(empleado.Id, input.FechaCorte ?? DateTime.Today);
            }

            // Ordinaria/Extraordinaria: salario actual
            return empleado.SalarioMensual;
        }

        /// <summary>
        /// REGLA CRÍTICA: ISR con exenciones para Aguinaldo/Bono14
        /// </summary>
        private async Task<decimal> CalcularIsrAsync(Empleado empleado, decimal salarioBruto, decimal igssEmpleado, PeriodoNominaInput input, ReglasLaborales reglas)
        {
            // Aguinaldo/Bono14: verificar exención anual de Q60,000
            if (input.TipoNomina == "AGUINALDO" || input.TipoNomina == "BONO14")
            {
                decimal acumuladoAno = await ObtenerAcumuladoAguinaldoBono14Async(empleado.Id, input.FechaCorte?.Year ?? DateTime.Today.Year);
                decimal nuevoAcumulado = acumuladoAno + salarioBruto;
                
                if (nuevoAcumulado <= AGUINALDO_BONO14_EXENCION_ANUAL)
                    return 0m; // Exento completamente
                
                // Si excede, calcular ISR solo sobre el exceso
                decimal exceso = nuevoAcumulado - AGUINALDO_BONO14_EXENCION_ANUAL;
                decimal baseImponible = Math.Min(exceso, salarioBruto);
                return IsrHelper.CalcularIsr(baseImponible, reglas.IsrEscalaJson, reglas.RedondeoDecimales, reglas.PoliticaRedondeo);
            }

            // Ordinaria/Extraordinaria: cálculo normal (salario bruto - IGSS)
            decimal baseImponibleNormal = salarioBruto - igssEmpleado;
            return IsrHelper.CalcularIsr(baseImponibleNormal, reglas.IsrEscalaJson, reglas.RedondeoDecimales, reglas.PoliticaRedondeo);
        }

        private async Task<decimal> CalcularPromedio12MesesAsync(int empleadoId, DateTime fechaCorte)
        {
            var fecha12MesesAtras = fechaCorte.AddMonths(-12);
            
            var salarios = await _context.DetalleNominas
                .Include(d => d.Nomina)
                .Where(d => d.EmpleadoId == empleadoId 
                         && d.Nomina.FechaCorte >= fecha12MesesAtras 
                         && d.Nomina.FechaCorte <= fechaCorte
                         && d.Nomina.TipoNomina == "ORDINARIA") // Solo ordinarias para el promedio
                .Select(d => d.SalarioBruto)
                .ToListAsync();

            return salarios.Any() ? salarios.Average() : 0m;
        }

        private async Task<decimal> ObtenerAcumuladoAguinaldoBono14Async(int empleadoId, int ano)
        {
            var inicioAno = new DateTime(ano, 1, 1);
            var finAno = new DateTime(ano, 12, 31);

            return await _context.DetalleNominas
                .Include(d => d.Nomina)
                .Where(d => d.EmpleadoId == empleadoId
                         && d.Nomina.FechaCorte >= inicioAno
                         && d.Nomina.FechaCorte <= finAno
                         && (d.Nomina.TipoNomina == "AGUINALDO" || d.Nomina.TipoNomina == "BONO14"))
                .SumAsync(d => d.SalarioBruto);
        }

        private async Task<ReglasLaborales> ObtenerReglasVigentesAsync()
        {
            return await _context.ReglasLaborales
                .Where(r => r.Pais == "GT" && r.Activo)
                .OrderByDescending(r => r.VigenteDesde)
                .FirstAsync();
        }

        private async Task<List<Empleado>> ObtenerEmpleadosFiltradosAsync(PeriodoNominaInput input)
        {
            var query = _context.Empleados
                .Include(e => e.Departamento)
                .Include(e => e.Puesto)
                .Where(e => e.EstadoLaboral == "ACTIVO");

            if (input.DepartamentoIds?.Any() == true)
                query = query.Where(e => input.DepartamentoIds.Contains(e.DepartamentoId ?? 0));

            if (input.EmpleadoIds?.Any() == true)
                query = query.Where(e => input.EmpleadoIds.Contains(e.Id));

            return await query.ToListAsync();
        }

        private async Task<decimal> ObtenerBonificacionesAsync(int empleadoId)
        {
            await Task.CompletedTask; // TODO: Implementar
            return 0m;
        }
        
        private async Task<decimal> ObtenerPrestamosAsync(int empleadoId)
        {
            await Task.CompletedTask; // TODO: Implementar
            return 0m;
        }
        
        private async Task<decimal> ObtenerAnticiposAsync(int empleadoId)
        {
            await Task.CompletedTask; // TODO: Implementar
            return 0m;
        }
        
        private async Task<decimal> ObtenerOtrasDeduccionesAsync(int empleadoId)
        {
            await Task.CompletedTask; // TODO: Implementar
            return 0m;
        }

        private async Task CalcularAportesPatronalesAsync(Nomina nomina, PeriodoNominaInput input)
        {
            var reglas = await ObtenerReglasVigentesAsync();
            
            var aportes = new NominaAportesPatronales
            {
                NominaId = nomina.Id,
                TotalIgssPatronal = Math.Round(nomina.TotalBruto * reglas.IgssPatronalPct, 2),
                TotalIrtra = (input.TipoNomina == "ORDINARIA") ? Math.Round(nomina.TotalBruto * reglas.IrtraPct, 2) : 0m,
                TotalIntecap = (input.TipoNomina == "ORDINARIA") ? Math.Round(nomina.TotalBruto * reglas.IntecapPct, 2) : 0m,
                CalculadoEn = DateTime.Now,
                CalculadoPor = "PayrollService"
            };

            aportes.TotalAportesPatronales = aportes.TotalIgssPatronal + aportes.TotalIrtra + aportes.TotalIntecap;
            
            _context.NominaAportesPatronales.Add(aportes);
        }
    }
}