namespace CafeteriaAPI.Models
{
    // Una linea del pedido: que producto, cuantos y a que precio
    public class DetallePedido
    {
        public int Id { get; set; }

        // Llave foranea hacia el pedido al que pertenece esta linea
        public int PedidoId { get; set; }

        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }

        // Guardamos el precio del momento, por si el producto cambia de precio despues
        public decimal PrecioUnitario { get; set; }

        // Se calcula, no se guarda en la base de datos
        public decimal Subtotal
        {
            get { return Cantidad * PrecioUnitario; }
        }
    }
}
