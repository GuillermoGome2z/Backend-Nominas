namespace ProyectoNominas.Backend.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string ClaveHash { get; set; }

        public ICollection<UsuarioRol> UsuarioRoles { get; set; }
    }
}
