namespace ProyectoNomina.Backend.Models
{
    public class DetalleNomina
    {
        public int Id { get; set; }

        public int NominaId { get; set; }
        public Nomina Nomina { get; set; }

        public int EmpleadoId { get; set; }
        public Empleado Empleado { get; set; }

        public decimal SalarioBruto { get; set; }
        public decimal Deducciones { get; set; }
        public decimal Bonificaciones { get; set; }
        public decimal SalarioNeto { get; set; }
    }
}
