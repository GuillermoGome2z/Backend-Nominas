namespace ProyectoNomina.Shared.Models.DTOs
{
    public class DashboardKpisDto
    {
        public int TotalEmpleados { get; set; }
        public List<ActivosPorDepartamentoDto> ActivosPorDepartamento { get; set; } = new();
        public int NominasUltimoMesCount { get; set; }
        public decimal NominasUltimoMesTotal { get; set; }
    }

    public class ActivosPorDepartamentoDto
    {
        public string Departamento { get; set; } = string.Empty;
        public int Activos { get; set; }
    }
}
