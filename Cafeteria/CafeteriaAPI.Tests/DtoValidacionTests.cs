using System.ComponentModel.DataAnnotations;
using CafeteriaAPI.DTOs;

namespace CafeteriaAPI.Tests
{
    // Pruebas de las reglas de validacion (DataAnnotations) de los DTOs.
    public class DtoValidacionTests
    {
        private static List<ValidationResult> Validar(object objeto)
        {
            var resultados = new List<ValidationResult>();
            Validator.TryValidateObject(objeto, new ValidationContext(objeto), resultados, validateAllProperties: true);
            return resultados;
        }

        [Fact]
        public void ProductoDTO_Valido_NoTieneErrores()
        {
            var dto = new ProductoDTO { Nombre = "Té chai", Categoria = "Bebida caliente", Precio = 4000m, Stock = 10 };

            Assert.Empty(Validar(dto));
        }

        [Theory]
        [InlineData("", "Postre", 5000, 5)]        // nombre vacio
        [InlineData("Torta", "", 5000, 5)]         // categoria vacia
        [InlineData("Torta", "Postre", 0, 5)]      // precio menor al minimo
        [InlineData("Torta", "Postre", 5000, -1)]  // stock negativo
        public void ProductoDTO_ConDatosInvalidos_TieneErrores(string nombre, string categoria, int precio, int stock)
        {
            var dto = new ProductoDTO { Nombre = nombre, Categoria = categoria, Precio = precio, Stock = stock };

            Assert.NotEmpty(Validar(dto));
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(100, true)]
        [InlineData(0, false)]
        [InlineData(101, false)]
        public void ItemPedidoDTO_LaCantidadDebeEstarEntre1y100(int cantidad, bool esValido)
        {
            var dto = new ItemPedidoDTO { ProductoId = 1, Cantidad = cantidad };

            Assert.Equal(esValido, Validar(dto).Count == 0);
        }

        [Fact]
        public void ClienteDTO_SinNombre_NoEsValido()
        {
            var dto = new ClienteDTO { Nombre = "", Telefono = "3001234567" };

            Assert.NotEmpty(Validar(dto));
        }

        [Theory]
        [InlineData("Ana", "ana01", "123456", true)]
        [InlineData("Ana", "ana01", "12345", false)]   // clave muy corta
        [InlineData("Ana", "an", "123456", false)]     // usuario muy corto
        [InlineData("A", "ana01", "123456", false)]    // nombre muy corto
        public void RegistroModel_ValidaLongitudesMinimas(string nombre, string username, string password, bool esValido)
        {
            var modelo = new RegistroModel { Nombre = nombre, Username = username, Password = password };

            Assert.Equal(esValido, Validar(modelo).Count == 0);
        }
    }
}
