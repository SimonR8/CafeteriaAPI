using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using CafeteriaAPI.Controllers;
using CafeteriaAPI.DTOs;
using CafeteriaAPI.JWT;
using CafeteriaAPI.Models;
using CafeteriaAPI.Tests.Ayudas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CafeteriaAPI.Tests
{
    // Pruebas de autenticacion: registro, inicio de sesion y generacion del token JWT.
    public class AuthTests : IDisposable
    {
        private readonly BaseDeDatosDePrueba _bd = new BaseDeDatosDePrueba();
        private readonly JwtService _jwt;
        private readonly AuthController _controlador;

        public AuthTests()
        {
            // Configuracion falsa, solo para las pruebas (la clave debe tener 32 caracteres o mas)
            var configuracion = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "ClaveDePruebas_ClaveDePruebas_123456",
                    ["Jwt:Issuer"] = "PruebasIssuer",
                    ["Jwt:Audience"] = "PruebasAudience",
                    ["Jwt:DurationInMinutes"] = "60"
                })
                .Build();

            _jwt = new JwtService(configuracion);
            _controlador = new AuthController(_jwt, _bd.Db);
        }

        public void Dispose() => _bd.Dispose();

        private static RegistroModel Registro(string username = "ana01", string password = "clave123") =>
            new RegistroModel { Nombre = "Ana Torres", Username = username, Password = password };

        // El API devuelve un objeto anonimo { token, usuario }: se pasa a JSON para leerlo
        private static JsonElement ComoJson(object? valor)
        {
            return JsonDocument.Parse(JsonSerializer.Serialize(valor)).RootElement;
        }

        // ---------- Registro ----------

        [Fact]
        public void Registro_UsuarioNuevo_DevuelveTokenYRolEmpleadoAuxiliar()
        {
            var resultado = _controlador.Registro(Registro());

            var ok = Assert.IsType<OkObjectResult>(resultado);
            var json = ComoJson(ok.Value);
            Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("token").GetString()));
            Assert.Equal("EmpleadoAuxiliar", json.GetProperty("usuario").GetProperty("Rol").GetString());
        }

        [Fact]
        public void Registro_NoGuardaLaContrasenaEnTextoPlano()
        {
            _controlador.Registro(Registro(password: "clave123"));

            using var db = _bd.NuevoContexto();
            var usuario = db.Usuarios.Single();
            Assert.NotEqual("clave123", usuario.PasswordHash);
            Assert.False(string.IsNullOrEmpty(usuario.PasswordHash));
            Assert.False(string.IsNullOrEmpty(usuario.PasswordSalt));
        }

        [Fact]
        public void Registro_DosUsuariosConLaMismaClave_TienenHashDistinto()
        {
            // Cada usuario tiene su propia "sal", asi que el hash nunca coincide
            _controlador.Registro(Registro("ana01", "clave123"));
            _controlador.Registro(Registro("luis02", "clave123"));

            using var db = _bd.NuevoContexto();
            var hashes = db.Usuarios.Select(u => u.PasswordHash).ToList();
            Assert.Equal(2, hashes.Distinct().Count());
        }

        [Fact]
        public void Registro_UsernameRepetido_DevuelveConflictSinImportarMayusculas()
        {
            _controlador.Registro(Registro("ana01"));

            var resultado = _controlador.Registro(Registro("ANA01"));

            Assert.IsType<ConflictObjectResult>(resultado);
            using var db = _bd.NuevoContexto();
            Assert.Equal(1, db.Usuarios.Count());
        }

        [Fact]
        public void Registro_QuitaLosEspaciosDelUsername()
        {
            _controlador.Registro(Registro("  pepe  "));

            using var db = _bd.NuevoContexto();
            Assert.Equal("pepe", db.Usuarios.Single().Username);
        }

        // ---------- Login ----------

        [Fact]
        public void Login_ConCredencialesCorrectas_DevuelveToken()
        {
            _controlador.Registro(Registro("ana01", "clave123"));

            var resultado = _controlador.Login(new LoginModel { Username = "ana01", Password = "clave123" });

            var ok = Assert.IsType<OkObjectResult>(resultado);
            Assert.False(string.IsNullOrWhiteSpace(ComoJson(ok.Value).GetProperty("token").GetString()));
        }

        [Fact]
        public void Login_NoDistingueMayusculasEnElUsername()
        {
            _controlador.Registro(Registro("ana01", "clave123"));

            var resultado = _controlador.Login(new LoginModel { Username = "ANA01", Password = "clave123" });

            Assert.IsType<OkObjectResult>(resultado);
        }

        [Fact]
        public void Login_ConContrasenaIncorrecta_DevuelveUnauthorized()
        {
            _controlador.Registro(Registro("ana01", "clave123"));

            var resultado = _controlador.Login(new LoginModel { Username = "ana01", Password = "otraClave" });

            Assert.IsType<UnauthorizedObjectResult>(resultado);
        }

        [Fact]
        public void Login_ConUsuarioInexistente_DevuelveUnauthorized()
        {
            var resultado = _controlador.Login(new LoginModel { Username = "fantasma", Password = "clave123" });

            Assert.IsType<UnauthorizedObjectResult>(resultado);
        }

        // ---------- JwtService ----------

        [Fact]
        public void JwtService_GenerateToken_CreaUnTokenConEmisorAudienciaYDatosDelUsuario()
        {
            var usuario = new Usuario { Id = 7, Nombre = "Ana", Username = "ana01", Rol = "EmpleadoAuxiliar" };

            var token = _jwt.GenerateToken(usuario);

            Assert.Equal(3, token.Split('.').Length);   // encabezado.contenido.firma

            var leido = new JwtSecurityTokenHandler().ReadJwtToken(token);
            Assert.Equal("PruebasIssuer", leido.Issuer);
            Assert.Contains("PruebasAudience", leido.Audiences);
            Assert.Contains(leido.Claims, c => c.Value == "ana01");
            Assert.Contains(leido.Claims, c => c.Value == "EmpleadoAuxiliar");
            Assert.True(leido.ValidTo > DateTime.UtcNow);
        }
    }
}
