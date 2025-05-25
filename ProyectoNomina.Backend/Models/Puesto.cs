namespace ProyectoNomina.Backend.Models
{
    public class Puesto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal SalarioBase { get; set; }

        public ICollection<Empleado> Empleados { get; set; }
    }
}

