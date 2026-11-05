using CafeteriaAPI.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CafeteriaAPI.Tests.Ayudas
{
    // Crea una base de datos SQLite que vive solo en memoria y solo durante una prueba.
    // Cada prueba recibe una base nueva, ya con los datos iniciales:
    //   Productos 1 a 9 (por ejemplo: 1 = Café americano, $3.500, stock 50;
    //                    3 = Latte, $6.000, stock 40; 9 = Torta de chocolate, stock 15)
    //   Clientes 1 (Laura Gómez, 120 puntos) y 2 (Andrés Pérez, 0 puntos)
    public class BaseDeDatosDePrueba : IDisposable
    {
        private readonly SqliteConnection _conexion;
        private readonly DbContextOptions<CafeteriaDbContext> _opciones;

        // Contexto que se le pasa al controlador que se está probando
        public CafeteriaDbContext Db { get; }

        public BaseDeDatosDePrueba()
        {
            // La base en memoria desaparece al cerrar la conexion, por eso se mantiene abierta
            _conexion = new SqliteConnection("Data Source=:memory:");
            _conexion.Open();

            _opciones = new DbContextOptionsBuilder<CafeteriaDbContext>()
                .UseSqlite(_conexion)
                .Options;

            Db = new CafeteriaDbContext(_opciones);
            Db.Database.EnsureCreated();
        }

        // Un contexto nuevo sobre la misma base: sirve para comprobar lo que
        // realmente quedó guardado (sin depender de los objetos en memoria).
        public CafeteriaDbContext NuevoContexto()
        {
            return new CafeteriaDbContext(_opciones);
        }

        public void Dispose()
        {
            Db.Dispose();
            _conexion.Dispose();
        }
    }
}
