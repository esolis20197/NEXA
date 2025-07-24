using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXA.Models;
using NEXA.Services;
using System.Security.Cryptography;

namespace NEXA.Controllers
{
    public class AccesoController : Controller
    {

        private readonly NEXAContext _context;
        private readonly GometaApiService _gometaService;


        public AccesoController(NEXAContext context, GometaApiService gometaService)
        {
            _context = context;
            _gometaService = gometaService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult InicioSesion()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult InicioSesion(LoginModeloUsuario modelo)
        {
            if (ModelState.IsValid)
            {
                var usuario = _context.Usuario
                    .FirstOrDefault(u => u.Correo == modelo.Correo && u.Contraseña == modelo.Contraseña);

                if (usuario != null)
                {

                    HttpContext.Session.SetString("UsuarioId", usuario.Id.ToString());
                    HttpContext.Session.SetString("UsuarioNombre", usuario.NombreCompleto);
                    HttpContext.Session.SetString("UsuarioRol", usuario.Rol.ToString());

                    TempData["Mensaje"] = $"Bienvenido {usuario.NombreUsuario}";
                    return RedirectToAction("IndexPanel", "Home");
                }

                TempData["MensajeInicioFallido"] = "Correo o contrasena incorrectos.";
                return RedirectToAction("InicioSesion");
            }
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult RegistroUsuario()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult RegistroUsuario(RegistroModeloUsuario modelo)
        {
            if (ModelState.IsValid)
            {
                // Validar que el usuario no exista previamente
                if (_context.Usuario.Any(u => u.NombreUsuario == modelo.NombreUsuario || u.Correo == modelo.Correo))
                {
                    TempData["MensajeRegistroIncorrecto"] = "El nombre de usuario o correo ya estan registrados.";
                    return View(modelo);
                }

                // Crear un nuevo usuario con la información proporcionada
                var nuevoUsuario = new Usuario
                {
                    NombreUsuario = modelo.NombreUsuario,
                    NombreCompleto = modelo.NombreCompleto,
                    Correo = modelo.Correo,
                    Cedula = modelo.Cedula,
                    Telefono = modelo.Telefono,
                    Contraseña = modelo.Contraseña,
                    Rol = modelo.Rol
                };

                // Guardar en la base de datos
                _context.Usuario.Add(nuevoUsuario);
                _context.SaveChanges();

                TempData["MensajeRegistroCorrecto"] = "Usuario registrado correctamente, ahora puedes iniciar sesion.";
                return RedirectToAction("InicioSesion");
            }

            return View(modelo);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> BuscarNombrePorCedula(string cedula)
        {
            if (string.IsNullOrEmpty(cedula))
                return Json(new { exito = false });

            var resultado = await _gometaService.BuscarCedula(cedula);

            if (resultado != null)
            {
                return Json(new { exito = true, nombre = resultado.nombre });
            }

            return Json(new { exito = false });
        }


        [HttpGet]
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            TempData["MensajeSession"] = "Sesión cerrada correctamente.";
            return RedirectToAction("InicioSesion", "Acceso");
        }

        private string GenerarToken()
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var tokenChars = new char[6];
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] datos = new byte[6];
                rng.GetBytes(datos);
                for (int i = 0; i < tokenChars.Length; i++)
                {
                    tokenChars[i] = caracteres[datos[i] % caracteres.Length];
                }
            }
            return new string(tokenChars);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult RecuperarContrasena()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> RecuperarContrasena(string correo)
        {
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == correo);

            if (usuario == null)
            {
                TempData["MensajeRecuperacion"] = "No se encontró ningún usuario con ese correo.";
                return View();
            }

            // Eliminamos todos los tokens existentes para este usuario
            var tokensExistentes = _context.PasswordResetTokens.Where(t => t.UsuarioId == usuario.Id);
            _context.PasswordResetTokens.RemoveRange(tokensExistentes);
            await _context.SaveChangesAsync();

            string token = GenerarToken().ToUpper();

            // Crear el registro del token
            var tokenRegistro = new PasswordResetToken
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(15),
                UsuarioId = usuario.Id
            };

            // Guardar token en la base
            _context.PasswordResetTokens.Add(tokenRegistro);
            await _context.SaveChangesAsync();

            var emailService = HttpContext.RequestServices.GetRequiredService<EmailService>();

            // Leer plantilla y personalizar            
            string html = await emailService.LeerPlantillaEmailAsync("EmailRecuperarContrasena.html");

            html = html.Replace("{{nombre}}", usuario.NombreCompleto)
                       .Replace("{{contenido}}", token);
            // Enviar correo
            await emailService.EnviarCorreoAsync(usuario.Correo, "Recuperación de contraseña", html);

            TempData["CorreoRecuperacion"] = correo;
            return RedirectToAction("ValidarToken");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ValidarToken()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ValidarToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError("", "Debes ingresar el código.");
                return View();
            }

            var tokenRegistro = await _context.PasswordResetTokens
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.Token.ToUpper() == token.ToUpper() && t.Expiration > DateTime.UtcNow);


            if (tokenRegistro == null)
            {
                ModelState.AddModelError("", "Código inválido o expirado.");
                return View();
            }

            TempData["TokenValido"] = token;
            return RedirectToAction("RestablecerContrasena", new { token });
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult RestablecerContrasena(string token)
        {

            ViewData["Token"] = token;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> RestablecerContrasena(string token, string nuevaContrasena, string confirmarContrasena)
        {
            if (nuevaContrasena != confirmarContrasena)
            {
                ModelState.AddModelError("", "Las contraseñas no coinciden.");
                ViewData["Token"] = token;
                return View();
            }

            var tokenRegistro = await _context.PasswordResetTokens
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.Token == token && t.Expiration > DateTime.UtcNow);

            if (tokenRegistro == null)
            {
                ModelState.AddModelError("", "Código inválido o expirado.");
                return View();
            }

            var usuario = tokenRegistro.Usuario;

            usuario.Contraseña = nuevaContrasena;

            _context.PasswordResetTokens.Remove(tokenRegistro);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Contraseña restablecida correctamente.";
            return RedirectToAction("InicioSesion");
        }

    }
}