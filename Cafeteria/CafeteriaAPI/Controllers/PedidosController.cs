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
    public class PedidosController : ControllerBase
    {
        private readonly CafeteriaDbContext _db;

        public PedidosController(CafeteriaDbContext db)
        {
            _db = db;
        }

        // Consulta base: trae los pedidos junto con sus detalles
        private IQueryable<Pedido> PedidosConDetalles()
        {
            return _db.Pedidos.Include(p => p.Detalles).AsNoTracking();
        }

        // GET: api/pedidos
        [HttpGet]
        public ActionResult<IEnumerable<Pedido>> ObtenerTodos()
        {
            return Ok(PedidosConDetalles().OrderBy(p => p.Id).ToList());
        }

        // GET: api/pedidos/ObtenerPedidoPorId?id=1
        [HttpGet("ObtenerPedidoPorId")]
        public ActionResult<Pedido> ObtenerPorId(int id)
        {
            var pedido = PedidosConDetalles().FirstOrDefault(p => p.Id == id);

            if (pedido == null)
            {
                return NotFound($"No se encontró un pedido con el ID {id}.");
            }

            return Ok(pedido);
        }

        // GET: api/pedidos/ObtenerPedidosPorCliente?clienteId=1
        [HttpGet("ObtenerPedidosPorCliente")]
        public ActionResult<IEnumerable<Pedido>> ObtenerPorCliente(int clienteId)
        {
            var resultado = PedidosConDetalles()
                .Where(p => p.ClienteId == clienteId)
                .OrderBy(p => p.Id)
                .ToList();

            if (!resultado.Any())
            {
                return NotFound($"El cliente con ID {clienteId} no tiene pedidos.");
            }

            return Ok(resultado);
        }

        // GET: api/pedidos/ObtenerPedidosPorEstado?estado=Pendiente
        [HttpGet("ObtenerPedidosPorEstado")]
        public ActionResult<IEnumerable<Pedido>> ObtenerPorEstado(EstadoPedido estado)
        {
            var resultado = PedidosConDetalles()
                .Where(p => p.Estado == estado)
                .OrderBy(p => p.Id)
                .ToList();

            if (!resultado.Any())
            {
                return NotFound($"No hay pedidos en estado '{estado}'.");
            }

            return Ok(resultado);
        }

        // POST: api/pedidos
        [HttpPost]
        public ActionResult<Pedido> CrearPedido([FromBody] PedidoDTO pedidoDto)
        {
            // 1. El cliente tiene que existir
            var cliente = _db.Clientes.FirstOrDefault(c => c.Id == pedidoDto.ClienteId);

            if (cliente == null)
            {
                return NotFound($"No se encontró un cliente con el ID {pedidoDto.ClienteId}.");
            }

            // 2. El pedido tiene que traer al menos un producto
            if (pedidoDto.Items.Count == 0)
            {
                return BadRequest("El pedido debe tener al menos un producto.");
            }

            // 3. Primero revisamos todo, sin cambiar nada todavia.
            //    Asi, si algo falla, no se descuenta stock a medias.
            //    Si el mismo producto viene repetido, se suman sus cantidades
            //    antes de revisar el stock (si no, el stock podria quedar negativo).
            var cantidadesPorProducto = pedidoDto.Items
                .GroupBy(i => i.ProductoId)
                .Select(g => new { ProductoId = g.Key, Cantidad = g.Sum(i => i.Cantidad) })
                .ToList();

            foreach (var item in cantidadesPorProducto)
            {
                var producto = _db.Productos.FirstOrDefault(p => p.Id == item.ProductoId);

                if (producto == null)
                {
                    return NotFound($"No se encontró un producto con el ID {item.ProductoId}.");
                }

                if (!producto.HayStock(item.Cantidad))
                {
                    return BadRequest($"No hay suficiente stock de '{producto.Nombre}'. Quedan {producto.Stock}.");
                }
            }

            // 4. Todo esta bien: creamos el pedido (el Id lo genera la base de datos)
            var nuevoPedido = new Pedido
            {
                ClienteId = cliente.Id,
                NombreCliente = cliente.Nombre,
                Fecha = DateTime.Now,
                Estado = EstadoPedido.Pendiente
            };

            foreach (ItemPedidoDTO item in pedidoDto.Items)
            {
                var producto = _db.Productos.First(p => p.Id == item.ProductoId);

                producto.DescontarStock(item.Cantidad);

                var detalle = new DetallePedido
                {
                    ProductoId = producto.Id,
                    NombreProducto = producto.Nombre,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = producto.Precio
                };

                nuevoPedido.Detalles.Add(detalle);
            }

            // 5. Calculamos el total (el cliente puede tener descuento)
            nuevoPedido.CalcularTotales(cliente.ObtenerPorcentajeDescuento());

            // 6. El cliente gana 1 punto por cada $1.000 pagados
            cliente.Puntos = cliente.Puntos + (int)(nuevoPedido.Total / 1000);

            // 7. Un solo SaveChanges: el pedido, los detalles, el stock y los puntos
            //    se guardan juntos (en una transaccion). Si algo falla, no se guarda nada.
            _db.Pedidos.Add(nuevoPedido);
            _db.SaveChanges();

            return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoPedido.Id }, nuevoPedido);
        }

        // PUT: api/pedidos/1/estado
        [HttpPut("{id}/estado")]
        public ActionResult<Pedido> CambiarEstado(int id, [FromBody] CambiarEstadoDTO estadoDto)
        {
            var pedido = _db.Pedidos.Include(p => p.Detalles).FirstOrDefault(p => p.Id == id);

            if (pedido == null)
            {
                return NotFound($"No se encontró un pedido con el ID {id}.");
            }

            // Un pedido que ya se entregó o se canceló no se puede cambiar
            if (pedido.Estado == EstadoPedido.Entregado || pedido.Estado == EstadoPedido.Cancelado)
            {
                return BadRequest($"El pedido ya está en estado '{pedido.Estado}' y no se puede cambiar.");
            }

            // Si se cancela, devolvemos los productos al inventario
            // y le quitamos al cliente los puntos que había ganado
            if (estadoDto.Estado == EstadoPedido.Cancelado)
            {
                foreach (DetallePedido detalle in pedido.Detalles)
                {
                    var producto = _db.Productos.FirstOrDefault(p => p.Id == detalle.ProductoId);

                    if (producto != null)
                    {
                        producto.ReponerStock(detalle.Cantidad);
                    }
                }

                var cliente = _db.Clientes.FirstOrDefault(c => c.Id == pedido.ClienteId);

                if (cliente != null)
                {
                    cliente.Puntos = cliente.Puntos - (int)(pedido.Total / 1000);

                    if (cliente.Puntos < 0)
                    {
                        cliente.Puntos = 0;
                    }
                }
            }

            pedido.Estado = estadoDto.Estado;
            _db.SaveChanges();

            return Ok(pedido);
        }
    }
}
