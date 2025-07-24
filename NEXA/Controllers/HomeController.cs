using Microsoft.AspNetCore.Mvc;
using NEXA.Models;
using System.Diagnostics;

namespace NEXA.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(NEXAContext context, ILogger<HomeController> logger) : base(context)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult IndexPanel()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");

            var model = new DashboardViewModel
            {
                Rol = rol ?? "SinRol",
                TotalUsuarios = _context.Usuario.Count(),
                TotalInventario = _context.Inventario.Count(),
                TotalPedidos = _context.Pedidos.Count(),
                TotalCitasPendientes = _context.Citas.Count(c => c.Estado == "Pendiente"),
                TotalPedidosPendientes = _context.Pedidos.Count(c => c.Estado == "Pendiente"),
                TotalProyectosPendientes = _context.Proyecto.Count(c => c.estado == "Pendiente"),

            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
