using NEXA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXA.Services;

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
                    return RedirectToAction("Index", "Home");
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
                return RedirectToAction("RegistroUsuario");
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



    }
}