using System.ComponentModel.DataAnnotations;

namespace CafeteriaAPI.DTOs
{
    // Un producto que el cliente quiere pedir y cuantas unidades
    public class ItemPedidoDTO
    {
        public int ProductoId { get; set; }

        [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100.")]
        public int Cantidad { get; set; }
    }
}
