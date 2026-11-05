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
    public class ClientesController : ControllerBase
    {
        private readonly CafeteriaDbContext _db;

        public ClientesController(CafeteriaDbContext db)
        {
            _db = db;
        }

        // GET: api/clientes
        [HttpGet]
        public ActionResult<IEnumerable<Cliente>> ObtenerTodos()
        {
            return Ok(_db.Clientes.AsNoTracking().OrderBy(c => c.Id).ToList());
        }

        // GET: api/clientes/ObtenerClientePorId?id=1
        [HttpGet("ObtenerClientePorId")]
        public ActionResult<Cliente> ObtenerPorId(int id)
        {
            var cliente = _db.Clientes.AsNoTracking().FirstOrDefault(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound($"No se encontró un cliente con el ID {id}.");
            }

            return Ok(cliente);
        }

        // GET: api/clientes/ObtenerClientesPorNombre?nombre=laura
        [HttpGet("ObtenerClientesPorNombre")]
        public ActionResult<IEnumerable<Cliente>> ObtenerPorNombre(string nombre)
        {
            var resultado = _db.Clientes
                .AsNoTracking()
                .Where(c => EF.Functions.Like(c.Nombre, $"%{nombre}%"))
                .ToList();

            if (!resultado.Any())
            {
                return NotFound($"No se encontraron clientes con el nombre '{nombre}'.");
            }

            return Ok(resultado);
        }

        // POST: api/clientes
        [HttpPost]
        public ActionResult<Cliente> CrearCliente([FromBody] ClienteDTO clienteDto)
        {
            // El Id lo genera la base de datos
            var nuevoCliente = new Cliente
            {
                Nombre = clienteDto.Nombre,
                Telefono = clienteDto.Telefono,
                Puntos = 0
            };

            _db.Clientes.Add(nuevoCliente);
            _db.SaveChanges();

            return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoCliente.Id }, nuevoCliente);
        }

        // PUT: api/clientes/1
        [HttpPut("{id}")]
        public ActionResult<Cliente> ActualizarCliente(int id, [FromBody] ClienteDTO clienteDto)
        {
            var cliente = _db.Clientes.FirstOrDefault(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound($"No se encontró un cliente con el ID {id}.");
            }

            cliente.Nombre = clienteDto.Nombre;
            cliente.Telefono = clienteDto.Telefono;

            _db.SaveChanges();

            return Ok(cliente);
        }

        // DELETE: api/clientes/1
        [HttpDelete("{id}")]
        public ActionResult EliminarCliente(int id)
        {
            var cliente = _db.Clientes.FirstOrDefault(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound($"No se encontró un cliente con el ID {id}.");
            }

            // Regla: no se borra un cliente que ya tiene pedidos
            bool tienePedidos = _db.Pedidos.Any(p => p.ClienteId == id);

            if (tienePedidos)
            {
                return BadRequest("No se puede eliminar un cliente que ya tiene pedidos.");
            }

            _db.Clientes.Remove(cliente);
            _db.SaveChanges();

            return Ok($"El cliente '{cliente.Nombre}' fue eliminado.");
        }
    }
}
