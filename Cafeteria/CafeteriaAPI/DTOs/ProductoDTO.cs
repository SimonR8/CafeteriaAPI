using System.ComponentModel.DataAnnotations;

namespace CafeteriaAPI.DTOs
{
    public class ProductoDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(60, ErrorMessage = "El nombre no puede pasar de 60 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public string Categoria { get; set; } = string.Empty;

        [Range(1.0, 1000000.0, ErrorMessage = "El precio debe estar entre 1 y 1.000.000.")]
        public decimal Precio { get; set; }

        [Range(0, 10000, ErrorMessage = "El stock debe estar entre 0 y 10.000.")]
        public int Stock { get; set; }
    }
}
