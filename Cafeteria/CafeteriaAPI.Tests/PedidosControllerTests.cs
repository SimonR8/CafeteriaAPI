using CafeteriaAPI.Controllers;
using CafeteriaAPI.DTOs;
using CafeteriaAPI.Models;
using CafeteriaAPI.Tests.Ayudas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeteriaAPI.Tests
{
    // Datos iniciales usados aqui:
    //   Cliente 1 = Laura (120 puntos, tiene descuento)   Cliente 2 = Andrés (0 puntos)
    //   Producto 1 = Café americano ($3.500, stock 50)    Producto 3 = Latte ($6.000, stock 40)
    //   Producto 9 = Torta de chocolate ($7.500, stock 15)
    public class PedidosControllerTests : IDisposable
    {
        private readonly BaseDeDatosDePrueba _bd = new BaseDeDatosDePrueba();
        private readonly PedidosController _controlador;

        public PedidosControllerTests()
        {
            _controlador = new PedidosController(_bd.Db);
        }

        public void Dispose() => _bd.Dispose();

        // Arma un PedidoDTO: Dto(2, (1, 2)) = cliente 2 pide 2 unidades del producto 1
        private static PedidoDTO Dto(int clienteId, params (int productoId, int cantidad)[] items)
        {
            return new PedidoDTO
            {
                ClienteId = clienteId,
                Items = items.Select(i => new ItemPedidoDTO { ProductoId = i.productoId, Cantidad = i.cantidad }).ToList()
            };
        }

        // Crea un pedido valido y devuelve el pedido creado
        private Pedido CrearPedidoValido(int clienteId, params (int productoId, int cantidad)[] items)
        {
            var accion = _controlador.CrearPedido(Dto(clienteId, items));
            var creado = Assert.IsType<CreatedAtActionResult>(accion.Result);
            return Assert.IsType<Pedido>(creado.Value);
        }

        // ---------- Crear pedido: casos de error ----------

        [Fact]
        public void CrearPedido_ClienteInexistente_DevuelveNotFound()
        {
            var accion = _controlador.CrearPedido(Dto(999, (1, 1)));

            Assert.IsType<NotFoundObjectResult>(accion.Result);
        }

        [Fact]
        public void CrearPedido_SinProductos_DevuelveBadRequest()
        {
            var accion = _controlador.CrearPedido(Dto(2));

            Assert.IsType<BadRequestObjectResult>(accion.Result);
        }

        [Fact]
        public void CrearPedido_ProductoInexistente_DevuelveNotFound()
        {
            var accion = _controlador.CrearPedido(Dto(2, (999, 1)));

            Assert.IsType<NotFoundObjectResult>(accion.Result);
        }

        [Fact]
        public void CrearPedido_SinStockSuficiente_DevuelveBadRequestYNoCambiaNada()
        {
            // Hay 15 tortas y se piden 16
            var accion = _controlador.CrearPedido(Dto(2, (9, 16)));

            Assert.IsType<BadRequestObjectResult>(accion.Result);

            using var db = _bd.NuevoContexto();
            Assert.Equal(15, db.Productos.Find(9)!.Stock);
            Assert.Empty(db.Pedidos);
        }

        [Fact]
        public void CrearPedido_UnaLineaBuenaYOtraSinStock_NoDescuentaNada()
        {
            // El stock se revisa completo antes de descontar: no debe quedar a medias
            var accion = _controlador.CrearPedido(Dto(2, (1, 2), (9, 16)));

            Assert.IsType<BadRequestObjectResult>(accion.Result);

            using var db = _bd.NuevoContexto();
            Assert.Equal(50, db.Productos.Find(1)!.Stock);
            Assert.Equal(15, db.Productos.Find(9)!.Stock);
        }

        [Fact]
        public void CrearPedido_MismoProductoRepetido_SumaLasCantidadesAntesDeRevisarElStock()
        {
            // Hay 15 tortas. Dos lineas de 10 suman 20, asi que no alcanza.
            var accion = _controlador.CrearPedido(Dto(2, (9, 10), (9, 10)));

            Assert.IsType<BadRequestObjectResult>(accion.Result);

            using var db = _bd.NuevoContexto();
            Assert.Equal(15, db.Productos.Find(9)!.Stock);
        }

        // ---------- Crear pedido: caso correcto ----------

        [Fact]
        public void CrearPedido_Valido_CalculaTotalDescuentaStockYSumaPuntos()
        {
            // Andrés (0 puntos) pide 2 cafés: 2 x 3.500 = 7.000
            var pedido = CrearPedidoValido(2, (1, 2));

            Assert.Equal(EstadoPedido.Pendiente, pedido.Estado);
            Assert.Equal(7000m, pedido.Subtotal);
            Assert.Equal(0m, pedido.Descuento);
            Assert.Equal(7000m, pedido.Total);
            Assert.Single(pedido.Detalles);

            using var db = _bd.NuevoContexto();
            Assert.Equal(48, db.Productos.Find(1)!.Stock);   // 50 - 2
            Assert.Equal(7, db.Clientes.Find(2)!.Puntos);    // 1 punto por cada $1.000
        }

        [Fact]
        public void CrearPedido_ClienteCon100PuntosOMas_RecibeDescuentoDel10()
        {
            // Laura (120 puntos) pide 2 lattes: 2 x 6.000 = 12.000, descuento 1.200, total 10.800
            var pedido = CrearPedidoValido(1, (3, 2));

            Assert.Equal(12000m, pedido.Subtotal);
            Assert.Equal(1200m, pedido.Descuento);
            Assert.Equal(10800m, pedido.Total);

            using var db = _bd.NuevoContexto();
            Assert.Equal(130, db.Clientes.Find(1)!.Puntos);  // 120 + 10
        }

        [Fact]
        public void CrearPedido_Valido_QuedaGuardadoEnLaBaseDeDatosConSusDetalles()
        {
            var pedido = CrearPedidoValido(2, (1, 2), (3, 1));

            using var db = _bd.NuevoContexto();
            var guardado = db.Pedidos.Include(p => p.Detalles).Single(p => p.Id == pedido.Id);

            Assert.Equal(2, guardado.ClienteId);
            Assert.Equal("Andrés Pérez", guardado.NombreCliente);
            Assert.Equal(EstadoPedido.Pendiente, guardado.Estado);
            Assert.Equal(2, guardado.Detalles.Count);
        }

        [Fact]
        public void CrearPedido_GuardaElPrecioDelMomento_AunqueElProductoCambieDespues()
        {
            var pedido = CrearPedidoValido(2, (1, 1));

            // Despues de vender, el cafe sube de precio
            var producto = _bd.Db.Productos.Find(1)!;
            producto.Precio = 9999m;
            _bd.Db.SaveChanges();

            using var db = _bd.NuevoContexto();
            var detalle = db.DetallesPedido.Single(d => d.PedidoId == pedido.Id);
            Assert.Equal(3500m, detalle.PrecioUnitario);
        }

        // ---------- Cambiar estado ----------

        [Fact]
        public void CambiarEstado_PedidoInexistente_DevuelveNotFound()
        {
            var accion = _controlador.CambiarEstado(999, new CambiarEstadoDTO { Estado = EstadoPedido.Preparando });

            Assert.IsType<NotFoundObjectResult>(accion.Result);
        }

        [Fact]
        public void CambiarEstado_APreparando_ActualizaElEstado()
        {
            var pedido = CrearPedidoValido(2, (1, 1));

            var accion = _controlador.CambiarEstado(pedido.Id, new CambiarEstadoDTO { Estado = EstadoPedido.Preparando });

            Assert.IsType<OkObjectResult>(accion.Result);
            using var db = _bd.NuevoContexto();
            Assert.Equal(EstadoPedido.Preparando, db.Pedidos.Find(pedido.Id)!.Estado);
        }

        [Fact]
        public void CambiarEstado_PedidoEntregado_YaNoSePuedeCambiar()
        {
            var pedido = CrearPedidoValido(2, (1, 2));
            _controlador.CambiarEstado(pedido.Id, new CambiarEstadoDTO { Estado = EstadoPedido.Entregado });

            var accion = _controlador.CambiarEstado(pedido.Id, new CambiarEstadoDTO { Estado = EstadoPedido.Cancelado });

            Assert.IsType<BadRequestObjectResult>(accion.Result);
            using var db = _bd.NuevoContexto();
            Assert.Equal(EstadoPedido.Entregado, db.Pedidos.Find(pedido.Id)!.Estado);
            Assert.Equal(48, db.Productos.Find(1)!.Stock);   // el stock no se repuso
        }

        [Fact]
        public void CambiarEstado_Cancelar_ReponeElStockYQuitaLosPuntos()
        {
            var pedido = CrearPedidoValido(2, (1, 2));   // stock 48, puntos 7

            var accion = _controlador.CambiarEstado(pedido.Id, new CambiarEstadoDTO { Estado = EstadoPedido.Cancelado });

            Assert.IsType<OkObjectResult>(accion.Result);
            using var db = _bd.NuevoContexto();
            Assert.Equal(EstadoPedido.Cancelado, db.Pedidos.Find(pedido.Id)!.Estado);
            Assert.Equal(50, db.Productos.Find(1)!.Stock);
            Assert.Equal(0, db.Clientes.Find(2)!.Puntos);
        }

        [Fact]
        public void CambiarEstado_PedidoCancelado_YaNoSePuedeCambiar()
        {
            var pedido = CrearPedidoValido(2, (1, 1));
            _controlador.CambiarEstado(pedido.Id, new CambiarEstadoDTO { Estado = EstadoPedido.Cancelado });

            var accion = _controlador.CambiarEstado(pedido.Id, new CambiarEstadoDTO { Estado = EstadoPedido.Preparando });

            Assert.IsType<BadRequestObjectResult>(accion.Result);
        }

        // ---------- Consultas ----------

        [Fact]
        public void ObtenerPorId_PedidoExistente_DevuelveElPedidoConSusDetalles()
        {
            var pedido = CrearPedidoValido(2, (1, 2), (3, 1));

            var accion = _controlador.ObtenerPorId(pedido.Id);

            var ok = Assert.IsType<OkObjectResult>(accion.Result);
            var encontrado = Assert.IsType<Pedido>(ok.Value);
            Assert.Equal(2, encontrado.Detalles.Count);
        }

        [Fact]
        public void ObtenerPorId_PedidoInexistente_DevuelveNotFound()
        {
            var accion = _controlador.ObtenerPorId(999);

            Assert.IsType<NotFoundObjectResult>(accion.Result);
        }

        [Fact]
        public void ObtenerPorCliente_SoloDevuelveLosPedidosDeEseCliente()
        {
            CrearPedidoValido(1, (1, 1));
            CrearPedidoValido(2, (1, 1));
            CrearPedidoValido(2, (3, 1));

            var accion = _controlador.ObtenerPorCliente(2);

            var ok = Assert.IsType<OkObjectResult>(accion.Result);
            var pedidos = Assert.IsAssignableFrom<IEnumerable<Pedido>>(ok.Value).ToList();
            Assert.Equal(2, pedidos.Count);
            Assert.All(pedidos, p => Assert.Equal(2, p.ClienteId));
        }

        [Fact]
        public void ObtenerPorCliente_SinPedidos_DevuelveNotFound()
        {
            var accion = _controlador.ObtenerPorCliente(2);

            Assert.IsType<NotFoundObjectResult>(accion.Result);
        }

        [Fact]
        public void ObtenerPorEstado_FiltraPorElEstadoPedido()
        {
            var pedido1 = CrearPedidoValido(2, (1, 1));
            CrearPedidoValido(2, (3, 1));
            _controlador.CambiarEstado(pedido1.Id, new CambiarEstadoDTO { Estado = EstadoPedido.Preparando });

            var preparando = _controlador.ObtenerPorEstado(EstadoPedido.Preparando);
            var pendientes = _controlador.ObtenerPorEstado(EstadoPedido.Pendiente);
            var entregados = _controlador.ObtenerPorEstado(EstadoPedido.Entregado);

            var okPreparando = Assert.IsType<OkObjectResult>(preparando.Result);
            Assert.Single(Assert.IsAssignableFrom<IEnumerable<Pedido>>(okPreparando.Value));
            var okPendientes = Assert.IsType<OkObjectResult>(pendientes.Result);
            Assert.Single(Assert.IsAssignableFrom<IEnumerable<Pedido>>(okPendientes.Value));
            Assert.IsType<NotFoundObjectResult>(entregados.Result);
        }
    }
}
