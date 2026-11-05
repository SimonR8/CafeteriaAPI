using System.Security.Cryptography;
using System.Text;
using CafeteriaAPI.Data;
using CafeteriaAPI.DTOs;
using CafeteriaAPI.JWT;
using CafeteriaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeteriaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly CafeteriaDbContext _db;

        public AuthController(JwtService jwtService, CafeteriaDbContext db)
        {
            _jwtService = jwtService;
            _db = db;
        }

        [AllowAnonymous]
        [HttpPost("registro")]
        public IActionResult Registro([FromBody] RegistroModel model)
        {
            var username = model.Username.Trim();
            if (_db.Usuarios.Any(u => u.Username.ToLower() == username.ToLower()))
                return Conflict("Ese nombre de usuario ya está registrado.");

            CrearHash(model.Password, out string hash, out string salt);
            var usuario = new Usuario
            {
                Nombre = model.Nombre.Trim(),
                Username = username,
                PasswordHash = hash,
                PasswordSalt = salt,
                Rol = "EmpleadoAuxiliar"
            };

            _db.Usuarios.Add(usuario);
            _db.SaveChanges();
            var token = _jwtService.GenerateToken(usuario);
            return Ok(new { token, usuario = new { usuario.Id, usuario.Nombre, usuario.Username, usuario.Rol } });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var username = model.Username.Trim().ToLower();
            var usuario = _db.Usuarios.FirstOrDefault(u => u.Username.ToLower() == username);

            if (usuario == null || !VerificarPassword(model.Password, usuario.PasswordHash, usuario.PasswordSalt))
                return Unauthorized("Usuario o contraseña incorrectos.");

            var token = _jwtService.GenerateToken(usuario);
            return Ok(new { token, usuario = new { usuario.Id, usuario.Nombre, usuario.Username, usuario.Rol } });
        }

        private static void CrearHash(string password, out string hash, out string salt)
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(16);
            byte[] hashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, 100000, HashAlgorithmName.SHA256, 32);
            salt = Convert.ToBase64String(saltBytes);
            hash = Convert.ToBase64String(hashBytes);
        }

        private static bool VerificarPassword(string password, string hashGuardado, string saltGuardado)
        {
            byte[] salt = Convert.FromBase64String(saltGuardado);
            byte[] hashCalculado = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(hashCalculado, Convert.FromBase64String(hashGuardado));
        }
    }
}
