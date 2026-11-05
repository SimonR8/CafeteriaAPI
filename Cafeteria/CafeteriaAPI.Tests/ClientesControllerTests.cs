using CafeteriaAPI.Controllers;
using CafeteriaAPI.DTOs;
using CafeteriaAPI.Models;
using CafeteriaAPI.Tests.Ayudas;
using Microsoft.AspNetCore.Mvc;

namespace CafeteriaAPI.Tests
{
    public class ClientesControllerTests : IDisposable
    {
        private readonly BaseDeDatosDePrueba _bd = new BaseDeDatosDePrueba();
        private readonly ClientesController _controlador;

        public ClientesControllerTests()
        {
            _controlador = new ClientesController(_bd.Db);
        }

        public void Dispose() => _bd.Dispose();

        [Fact]
        public void ObtenerTodos_DevuelveLosDosClientesIniciales()
        {
            var accion = _controlador.ObtenerTodos();

            var ok = Assert.IsType<OkObjectResult>(accion.Result);
            var clientes = Assert.IsAssignableFrom<IEnumerable<Cliente>>(ok.Value);
            Assert.Equal(2, clientes.Count());
        }

        [Fact]
        public void ObtenerPorId_ClienteInexistente_DevuelveNotFound()
        {
            var accion = _controlador.ObtenerPorId(999);

            Assert.IsType<NotFoundObjectResult>(accion.Result);
        }

        [Fact]
        public void ObtenerPorNombre_EncuentraAunqueLaBusquedaVengaEnMinusculas()
        {
            var accion = _controlador.ObtenerPorNombre("laura");

            var ok = Assert.IsType<OkObjectResult>(accion.Result);
            var clientes = Assert.IsAssignableFrom<IEnumerable<Cliente>>(ok.Value).ToList();
            Assert.Single(clientes);
            Assert.Equal("Laura Gómez", clientes[0].Nombre);
        }

        [Fact]
        public void ObtenerPorNombre_SinCoincidencias_DevuelveNotFound()
        {
            var accion = _controlador.ObtenerPorNombre("zzz");

            Assert.IsType<NotFoundObjectResult>(accion.Result);
        }

        [Fact]
        public void CrearCliente_EmpiezaConCeroPuntosYQuedaGuardado()
        {
            var dto = new ClienteDTO { Nombre = "Camila Ríos", Telefono = "3151112233" };

            var accion = _controlador.CrearCliente(dto);

            var creado = Assert.IsType<CreatedAtActionResult>(accion.Result);
            var cliente = Assert.IsType<Cliente>(creado.Value);
            Assert.Equal(3, cliente.Id);
            Assert.Equal(0, cliente.Puntos);

            using var db = _bd.NuevoContexto();
            Assert.Equal("Camila Ríos", db.Clientes.Find(3)!.Nombre);
        }

        [Fact]
        public void ActualizarCliente_CambiaNombreYTelefonoPeroNoLosPuntos()
        {
            var dto = new ClienteDTO { Nombre = "Laura G.", Telefono = "3000000000" };

            var accion = _controlador.ActualizarCliente(1, dto);

            Assert.IsType<OkObjectResult>(accion.Result);
            using var db = _bd.NuevoContexto();
            var cliente = db.Clientes.Find(1)!;
            Assert.Equal("Laura G.", cliente.Nombre);
            Assert.Equal("3000000000", cliente.Telefono);
            Assert.Equal(120, cliente.Puntos);
        }

        [Fact]
        public void ActualizarCliente_Inexistente_DevuelveNotFound()
        {
            var dto = new ClienteDTO { Nombre = "X", Telefono = "1" };

            var accion = _controlador.ActualizarCliente(999, dto);

            Assert.IsType<NotFoundObjectResult>(accion.Result);
        }

        [Fact]
        public void EliminarCliente_SinPedidos_LoQuitaDeLaBaseDeDatos()
        {
            var resultado = _controlador.EliminarCliente(2);

            Assert.IsType<OkObjectResult>(resultado);
            using var db = _bd.NuevoContexto();
            Assert.Null(db.Clientes.Find(2));
        }

        [Fact]
        public void EliminarCliente_ConPedidos_DevuelveBadRequestYNoLoBorra()
        {
            var pedidos = new PedidosController(_bd.Db);
            pedidos.CrearPedido(new PedidoDTO
            {
                ClienteId = 2,
                Items = new List<ItemPedidoDTO> { new ItemPedidoDTO { ProductoId = 1, Cantidad = 1 } }
            });

            var resultado = _controlador.EliminarCliente(2);

            Assert.IsType<BadRequestObjectResult>(resultado);
            using var db = _bd.NuevoContexto();
            Assert.NotNull(db.Clientes.Find(2));
        }

        [Fact]
        public void EliminarCliente_Inexistente_DevuelveNotFound()
        {
            var resultado = _controlador.EliminarCliente(999);

            Assert.IsType<NotFoundObjectResult>(resultado);
        }
    }
}
