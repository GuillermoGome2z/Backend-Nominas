namespace ProyectoNomina.Backend.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expira { get; set; }
        public bool Revocado { get; set; } = false;
    }
}
