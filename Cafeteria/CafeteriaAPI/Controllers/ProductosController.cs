using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeteriaAPI.Data;
using CafeteriaAPI.DTOs;
using CafeteriaAPI.Models;

namespace CafeteriaAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly CafeteriaDbContext _db;

        public ProductosController(CafeteriaDbContext db)
        {
            _db = db;
        }

        // GET: api/productos
        [HttpGet]
        public ActionResult<IEnumerable<Producto>> ObtenerTodos()
        {
            return Ok(_db.Productos.AsNoTracking().OrderBy(p => p.Id).ToList());
        }

        // GET: api/productos/ObtenerProductoPorId?id=1
        [HttpGet("ObtenerProductoPorId")]
        public ActionResult<Producto> ObtenerPorId(int id)
        {
            var producto = _db.Productos.AsNoTracking().FirstOrDefault(p => p.Id == id);

            if (producto == null)
            {
                return NotFound($"No se encontró un producto con el ID {id}.");
            }

            return Ok(producto);
        }

        // GET: api/productos/ObtenerProductosPorNombre?nombre=cafe
        [HttpGet("ObtenerProductosPorNombre")]
        public ActionResult<IEnumerable<Producto>> ObtenerPorNombre(string nombre)
        {
            var resultado = _db.Productos
                .AsNoTracking()
                .Where(p => EF.Functions.Like(p.Nombre, $"%{nombre}%"))
                .ToList();

            if (!resultado.Any())
            {
                return NotFound($"No se encontraron productos con el nombre '{nombre}'.");
            }

            return Ok(resultado);
        }

        // GET: api/productos/ObtenerProductosPorCategoria?categoria=postre
        [HttpGet("ObtenerProductosPorCategoria")]
        public ActionResult<IEnumerable<Producto>> ObtenerPorCategoria(string categoria)
        {
            var resultado = _db.Productos
                .AsNoTracking()
                .Where(p => EF.Functions.Like(p.Categoria, $"%{categoria}%"))
                .ToList();

            if (!resultado.Any())
            {
                return NotFound($"No se encontraron productos de la categoría '{categoria}'.");
            }

            return Ok(resultado);
        }

        // POST: api/productos
        [HttpPost]
        public ActionResult<Producto> CrearProducto([FromBody] ProductoDTO productoDto)
        {
            // El Id lo genera la base de datos
            var nuevoProducto = new Producto
            {
                Nombre = productoDto.Nombre,
                Categoria = productoDto.Categoria,
                Precio = productoDto.Precio,
                Stock = productoDto.Stock
            };

            _db.Productos.Add(nuevoProducto);
            _db.SaveChanges();

            return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoProducto.Id }, nuevoProducto);
        }

        // PUT: api/productos/1
        [HttpPut("{id}")]
        public ActionResult<Producto> ActualizarProducto(int id, [FromBody] ProductoDTO productoDto)
        {
            var producto = _db.Productos.FirstOrDefault(p => p.Id == id);

            if (producto == null)
            {
                return NotFound($"No se encontró un producto con el ID {id}.");
            }

            producto.Nombre = productoDto.Nombre;
            producto.Categoria = productoDto.Categoria;
            producto.Precio = productoDto.Precio;
            producto.Stock = productoDto.Stock;

            _db.SaveChanges();

            return Ok(producto);
        }

        // DELETE: api/productos/1
        [HttpDelete("{id}")]
        public ActionResult EliminarProducto(int id)
        {
            var producto = _db.Productos.FirstOrDefault(p => p.Id == id);

            if (producto == null)
            {
                return NotFound($"No se encontró un producto con el ID {id}.");
            }

            _db.Productos.Remove(producto);
            _db.SaveChanges();

            return Ok($"El producto '{producto.Nombre}' fue eliminado.");
        }
    }
}
