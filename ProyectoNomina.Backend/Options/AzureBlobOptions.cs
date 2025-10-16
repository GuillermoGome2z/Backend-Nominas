namespace ProyectoNomina.Backend.Options
{
    public class AzureBlobOptions
    {
        public bool Enabled { get; set; } = false;
        public string? ConnectionString { get; set; }
        public string ContainerName { get; set; } = "nomina-docs";
        public int DefaultSasMinutes { get; set; } = 15;
    }
}
