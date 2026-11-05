namespace CafeteriaAPI.Models
{
    // Cliente hereda de Persona (ya trae Id, Nombre y Telefono)
    public class Cliente : Persona
    {
        // Puntos que gana el cliente por cada compra
        public int Puntos { get; set; }

        // Regla de la cafeteria: con 100 puntos o mas, 10% de descuento
        public override decimal ObtenerPorcentajeDescuento()
        {
            if (Puntos >= 100)
            {
                return 0.10m;
            }

            return 0m;
        }
    }
}
