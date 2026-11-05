using CafeteriaAPI.Models;

namespace CafeteriaAPI.Tests
{
    // Pruebas de las clases del modelo: no necesitan base de datos.
    public class ModelosTests
    {
        // ---------- Producto ----------

        [Theory]
        [InlineData(10, 5, true)]    // sobra stock
        [InlineData(10, 10, true)]   // justo el stock que hay
        [InlineData(10, 11, false)]  // falta una unidad
        [InlineData(0, 1, false)]    // agotado
        public void Producto_HayStock_DependeDeLaCantidadPedida(int stock, int cantidad, bool esperado)
        {
            var producto = new Producto { Stock = stock };

            Assert.Equal(esperado, producto.HayStock(cantidad));
        }

        [Fact]
        public void Producto_DescontarStock_RestaLasUnidades()
        {
            var producto = new Producto { Stock = 20 };

            producto.DescontarStock(5);

            Assert.Equal(15, producto.Stock);
        }

        [Fact]
        public void Producto_ReponerStock_SumaLasUnidades()
        {
            var producto = new Producto { Stock = 20 };

            producto.ReponerStock(5);

            Assert.Equal(25, producto.Stock);
        }

        // ---------- Cliente (herencia y polimorfismo) ----------

        [Theory]
        [InlineData(0, 0)]
        [InlineData(99, 0)]     // un punto menos del minimo: sin descuento
        [InlineData(100, 10)]   // justo 100 puntos: 10%
        [InlineData(250, 10)]
        public void Cliente_ObtenerPorcentajeDescuento_ApartirDe100PuntosEs10Porciento(int puntos, int porcentajeEsperado)
        {
            var cliente = new Cliente { Puntos = puntos };

            Assert.Equal(porcentajeEsperado / 100m, cliente.ObtenerPorcentajeDescuento());
        }

        [Fact]
        public void Cliente_UsadoComoPersona_AplicaSuPropioDescuento()
        {
            // Polimorfismo: la variable es de tipo Persona, pero se ejecuta el metodo de Cliente
            Persona persona = new Cliente { Nombre = "Laura", Puntos = 100 };

            Assert.Equal(0.10m, persona.ObtenerPorcentajeDescuento());
        }

        // ---------- DetallePedido ----------

        [Fact]
        public void DetallePedido_Subtotal_EsCantidadPorPrecio()
        {
            var detalle = new DetallePedido { Cantidad = 3, PrecioUnitario = 2500m };

            Assert.Equal(7500m, detalle.Subtotal);
        }

        // ---------- Pedido ----------

        private static Pedido PedidoDeEjemplo()
        {
            var pedido = new Pedido();
            pedido.Detalles.Add(new DetallePedido { Cantidad = 2, PrecioUnitario = 3500m }); // 7.000
            pedido.Detalles.Add(new DetallePedido { Cantidad = 1, PrecioUnitario = 6000m }); // 6.000
            return pedido;
        }

        [Fact]
        public void Pedido_NuevoPedido_EmpiezaPendiente()
        {
            var pedido = new Pedido();

            Assert.Equal(EstadoPedido.Pendiente, pedido.Estado);
        }

        [Fact]
        public void Pedido_CalcularTotales_SinDescuento()
        {
            var pedido = PedidoDeEjemplo();

            pedido.CalcularTotales(0m);

            Assert.Equal(13000m, pedido.Subtotal);
            Assert.Equal(0m, pedido.Descuento);
            Assert.Equal(13000m, pedido.Total);
        }

        [Fact]
        public void Pedido_CalcularTotales_ConDescuentoDel10()
        {
            var pedido = PedidoDeEjemplo();

            pedido.CalcularTotales(0.10m);

            Assert.Equal(13000m, pedido.Subtotal);
            Assert.Equal(1300m, pedido.Descuento);
            Assert.Equal(11700m, pedido.Total);
        }

        [Fact]
        public void Pedido_CalcularTotales_SinDetallesTodoEsCero()
        {
            var pedido = new Pedido();

            pedido.CalcularTotales(0.10m);

            Assert.Equal(0m, pedido.Subtotal);
            Assert.Equal(0m, pedido.Total);
        }
    }
}
