namespace CafeteriaAPI.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

        // El pedido "tiene" varios detalles (composicion)
        public List<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();

        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }

        // Calcula los valores del pedido a partir de sus detalles
        public void CalcularTotales(decimal porcentajeDescuento)
        {
            decimal suma = 0;

            foreach (DetallePedido detalle in Detalles)
            {
                suma = suma + detalle.Subtotal;
            }

            Subtotal = suma;
            Descuento = suma * porcentajeDescuento;
            Total = Subtotal - Descuento;
        }
    }
}
