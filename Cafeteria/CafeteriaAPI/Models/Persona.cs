namespace CafeteriaAPI.Models
{
    // Clase base (abstracta): tiene lo que comparte cualquier persona del sistema.
    // No se pueden crear objetos "Persona" directamente, solo de las clases hijas.
    public abstract class Persona
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;

        // Cada clase hija decide cuanto descuento le toca (polimorfismo)
        public abstract decimal ObtenerPorcentajeDescuento();
    }
}
