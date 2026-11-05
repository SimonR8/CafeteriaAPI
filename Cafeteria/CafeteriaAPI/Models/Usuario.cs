namespace CafeteriaAPI.Models
{
    // Usuario del sistema. La contraseña nunca se almacena en texto plano.
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        public string Rol { get; set; } = "EmpleadoAuxiliar";
    }
}
