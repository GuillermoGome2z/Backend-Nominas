namespace ProyectoNomina.Frontend.Auth
{
    public interface IAuthService
    {
        Task Login(string correo, string contraseña);
        Task Logout();
    }
}
