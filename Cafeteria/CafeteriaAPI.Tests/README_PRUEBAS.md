# Pruebas unitarias – CafeteriaAPI.Tests

Proyecto xUnit (.NET 8) con las pruebas del API de la cafetería.

## Cómo ejecutarlas
- **Visual Studio:** menú *Prueba > Explorador de pruebas* y botón *Ejecutar todas las pruebas*.
- **Terminal:** desde la carpeta `Cafeteria`, `dotnet test CafeteriaAPI.Tests`.

## Qué se prueba
| Archivo | Qué cubre |
|---|---|
| `ModelosTests.cs` | `Producto` (stock), `Cliente` (descuento y polimorfismo), `Pedido.CalcularTotales`, `DetallePedido.Subtotal` |
| `DtoValidacionTests.cs` | Reglas de validación de los DTOs |
| `PedidosControllerTests.cs` | Crear pedido (errores y caso correcto), cambiar estado, cancelar, consultas |
| `ProductosControllerTests.cs` | Consultas, crear, actualizar y eliminar productos |
| `ClientesControllerTests.cs` | Consultas, crear, actualizar y eliminar clientes (regla: no borrar con pedidos) |
| `AuthTests.cs` | Registro, login, hash de contraseña y token JWT |

## Cómo funcionan las pruebas de controladores
`Ayudas/BaseDeDatosDePrueba.cs` crea una base SQLite **en memoria** para cada prueba,
ya con los datos iniciales (productos 1–9 y clientes 1–2). Así las pruebas no tocan
el archivo `cafeteria.db` real y no dependen unas de otras.
Cada prueba sigue el patrón *Arrange – Act – Assert* (preparar, ejecutar, verificar).
