namespace CafeteriaAPI.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }

        // El propio producto sabe si le alcanza el inventario
        public bool HayStock(int cantidad)
        {
            return Stock >= cantidad;
        }

        public void DescontarStock(int cantidad)
        {
            Stock = Stock - cantidad;
        }

        public void ReponerStock(int cantidad)
        {
            Stock = Stock + cantidad;
        }
    }
}
