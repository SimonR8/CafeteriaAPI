using CafeteriaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CafeteriaAPI.Data
{
    // Puente entre las clases C# y la base de datos SQLite.
    // Cada DbSet es una tabla.
    public class CafeteriaDbContext : DbContext
    {
        public CafeteriaDbContext(DbContextOptions<CafeteriaDbContext> options) : base(options)
        {
        }

        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Pedido> Pedidos => Set<Pedido>();
        public DbSet<DetallePedido> DetallesPedido => Set<DetallePedido>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Producto
            modelBuilder.Entity<Producto>(e =>
            {
                e.ToTable("Productos");
                e.Property(p => p.Nombre).IsRequired().HasMaxLength(60);
                e.Property(p => p.Categoria).IsRequired();
                e.Property(p => p.Precio).HasPrecision(10, 2);
            });

            // Cliente (hereda de Persona, que es abstracta y no tiene tabla propia:
            // Id, Nombre y Telefono quedan dentro de la tabla Clientes)
            modelBuilder.Entity<Cliente>(e =>
            {
                e.ToTable("Clientes");
                e.Property(c => c.Nombre).IsRequired();
            });

            // Pedido: el estado se guarda como texto ("Pendiente", "Entregado"...)
            modelBuilder.Entity<Pedido>(e =>
            {
                e.ToTable("Pedidos");
                e.Property(p => p.Estado).HasConversion<string>();
                e.Property(p => p.Subtotal).HasPrecision(10, 2);
                e.Property(p => p.Descuento).HasPrecision(10, 2);
                e.Property(p => p.Total).HasPrecision(10, 2);

                // Un cliente tiene muchos pedidos. Restrict = no se puede borrar
                // un cliente que ya tiene pedidos (la misma regla del controlador).
                e.HasOne<Cliente>()
                    .WithMany()
                    .HasForeignKey(p => p.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Composicion: un pedido contiene sus detalles y si se borra el pedido,
                // se borran sus detalles.
                e.HasMany(p => p.Detalles)
                    .WithOne()
                    .HasForeignKey(d => d.PedidoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DetallePedido>(e =>
            {
                e.ToTable("DetallesPedido");
                e.Property(d => d.PrecioUnitario).HasPrecision(10, 2);
            });

            // Usuario: el nombre de usuario no se puede repetir
            modelBuilder.Entity<Usuario>(e =>
            {
                e.ToTable("Usuarios");
                e.HasIndex(u => u.Username).IsUnique();
            });

            // Datos iniciales (los mismos que antes tenia BaseDeDatos)
            modelBuilder.Entity<Producto>().HasData(
                new Producto { Id = 1, Nombre = "Café americano", Categoria = "Bebida caliente", Precio = 3500, Stock = 50 },
                new Producto { Id = 2, Nombre = "Capuchino", Categoria = "Bebida caliente", Precio = 5500, Stock = 40 },
                new Producto { Id = 3, Nombre = "Latte", Categoria = "Bebida caliente", Precio = 6000, Stock = 40 },
                new Producto { Id = 4, Nombre = "Chocolate caliente", Categoria = "Bebida caliente", Precio = 5000, Stock = 30 },
                new Producto { Id = 5, Nombre = "Limonada de coco", Categoria = "Bebida fría", Precio = 6500, Stock = 25 },
                new Producto { Id = 6, Nombre = "Pan de bono", Categoria = "Panadería", Precio = 2500, Stock = 60 },
                new Producto { Id = 7, Nombre = "Almojábana", Categoria = "Panadería", Precio = 2800, Stock = 60 },
                new Producto { Id = 8, Nombre = "Croissant", Categoria = "Panadería", Precio = 4500, Stock = 20 },
                new Producto { Id = 9, Nombre = "Torta de chocolate", Categoria = "Postre", Precio = 7500, Stock = 15 }
            );

            modelBuilder.Entity<Cliente>().HasData(
                new Cliente { Id = 1, Nombre = "Laura Gómez", Telefono = "3001234567", Puntos = 120 },
                new Cliente { Id = 2, Nombre = "Andrés Pérez", Telefono = "3109876543", Puntos = 0 }
            );
        }
    }
}
