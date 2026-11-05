namespace CafeteriaAPI.DTOs
{
    public class PedidoDTO
    {
        public int ClienteId { get; set; }
        public List<ItemPedidoDTO> Items { get; set; } = new List<ItemPedidoDTO>();
    }
}
