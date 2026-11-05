using System.ComponentModel.DataAnnotations;

namespace CafeteriaAPI.DTOs
{
    public class RegistroModel
    {
        [Required, MinLength(2)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MinLength(3)]
        public string Username { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
