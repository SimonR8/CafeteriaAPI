using CafeteriaAPI.Controllers;
using CafeteriaAPI.DTOs;
using CafeteriaAPI.Models;
using CafeteriaAPI.Tests.Ayudas;
using Microsoft.AspNetCore.Mvc;

namespace CafeteriaAPI.Tests
{
    public class ProductosControllerTests : IDisposable
    {
        private readonly BaseDeDatosDePrueba _bd = new BaseDeDatosDePrueba();
        private readonly ProductosController _controlador;

        public ProductosControllerTests()
        {
            _controlador = new ProductosController(_bd.Db);
        }

        public void Dispose() => _bd.Dispose();

        [Fact]
        public void ObtenerTodos_DevuelveLos9ProductosIniciales()
        {
            var accion = _controlador.ObtenerTodos();

            var ok = Assert.IsType<OkObjectResult>(accion.Result);
            var productos = Assert.IsAssignableFrom<IEnumerable<Producto>>(ok.Value);
            Assert.Equal(9, productos.Count());
        }

        [Fact]
        public void ObtenerPorId_ProductoExistente_DevuelveElProducto()
        {
            var accion = _controlador.ObtenerPorId(3);

            var ok = Assert.IsType<OkObjectResult>(accion.Result);
            var producto = Assert.IsType<Producto>(ok.Value);
            Assert.Equal("Latte", producto.Nombre);
        }

        [Fact]
        public void ObtenerPorId_ProductoInexistente_DevuelveNotFound()
        {
            var accion = _controlador.ObtenerPorId(999);

            Assert.IsType<NotFoundObjectResult>(accion.Result);
        }

        [Fact]
        public void ObtenerPorNombre_NoDistingueMayusculasDeMinusculas()
        {
            var accion = _controlador.ObtenerPorNombre("LATTE");

            var ok = Assert.IsType<OkObjectResult>(accion.Result);
            var productos = Assert.IsAssignableFrom<IEnumerable<Producto>>(ok.Value);
            Assert.Single(productos);
        }

        [Fact]
        public void ObtenerPorNombre_SinCoincidencias_DevuelveNotFound()
        {
            var accion = _controlador.ObtenerPorNombre("pizza");

            Assert.IsType<NotFoundObjectResult>(accion.Result);
        }

        [Fact]
        public void ObtenerPorCategoria_DevuelveSoloLosDeEsaCategoria()
        {
            var accion = _controlador.ObtenerPorCategoria("panadería");

            var ok = Assert.IsType<OkObjectResult>(accion.Result);
            var productos = Assert.IsAssignableFrom<IEnumerable<Producto>>(ok.Value).ToList();
            Assert.Equal(3, productos.Count);   // pan de bono, almojábana y croissant
            Assert.All(productos, p => Assert.Equal("Panadería", p.Categoria));
        }

        [Fact]
        public void CrearProducto_GuardaElProductoYLaBaseDeDatosLeAsignaElId()
        {
            var dto = new ProductoDTO { Nombre = "Té chai", Categoria = "Bebida caliente", Precio = 4200m, Stock = 12 };

            var accion = _controlador.CrearProducto(dto);

            var creado = Assert.IsType<CreatedAtActionResult>(accion.Result);
            var producto = Assert.IsType<Producto>(creado.Value);
            Assert.Equal(10, producto.Id);   // los iniciales son del 1 al 9

            using var db = _bd.NuevoContexto();
            var guardado = db.Productos.Find(10)!;
            Assert.Equal("Té chai", guardado.Nombre);
            Assert.Equal(4200m, guardado.Precio);
            Assert.Equal(12, guardado.Stock);
        }

        [Fact]
        public void ActualizarProducto_CambiaLosDatosEnLaBaseDeDatos()
        {
            var dto = new ProductoDTO { Nombre = "Café americano grande", Categoria = "Bebida caliente", Precio = 4000m, Stock = 30 };

            var accion = _controlador.ActualizarProducto(1, dto);

            Assert.IsType<OkObjectResult>(accion.Result);
            using var db = _bd.NuevoContexto();
            var producto = db.Productos.Find(1)!;
            Assert.Equal("Café americano grande", producto.Nombre);
            Assert.Equal(4000m, producto.Precio);
            Assert.Equal(30, producto.Stock);
        }

        [Fact]
        public void ActualizarProducto_Inexistente_DevuelveNotFound()
        {
            var dto = new ProductoDTO { Nombre = "X", Categoria = "Y", Precio = 1000m, Stock = 1 };

            var accion = _controlador.ActualizarProducto(999, dto);

            Assert.IsType<NotFoundObjectResult>(accion.Result);
        }

        [Fact]
        public void EliminarProducto_Existente_LoQuitaDeLaBaseDeDatos()
        {
            var resultado = _controlador.EliminarProducto(9);

            Assert.IsType<OkObjectResult>(resultado);
            using var db = _bd.NuevoContexto();
            Assert.Null(db.Productos.Find(9));
            Assert.Equal(8, db.Productos.Count());
        }

        [Fact]
        public void EliminarProducto_Inexistente_DevuelveNotFound()
        {
            var resultado = _controlador.EliminarProducto(999);

            Assert.IsType<NotFoundObjectResult>(resultado);
        }
    }
}
