namespace ProyectoNomina.Shared.Models.DTOs
{
    public class SignedUrlDto
    {
        public string Url { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
        public string? Path { get; set; }
    }
}
