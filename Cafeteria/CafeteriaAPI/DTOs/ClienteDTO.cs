using System.ComponentModel.DataAnnotations;

namespace CafeteriaAPI.DTOs
{
    public class ClienteDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(80, ErrorMessage = "El nombre no puede pasar de 80 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public string Telefono { get; set; } = string.Empty;
    }
}
