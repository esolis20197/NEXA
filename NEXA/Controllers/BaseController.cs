using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using NEXA.Models;

namespace NEXA.Controllers
{
    public class BaseController : Controller
    {
        protected readonly NEXAContext _context;

        public BaseController(NEXAContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (!string.IsNullOrEmpty(usuarioIdStr) && int.TryParse(usuarioIdStr, out int usuarioId))
            {
                var carrito = _context.Carritos
                                .Include(c => c.Detalles)
                                .FirstOrDefault(c => c.UsuarioId == usuarioId);

                ViewBag.CantidadCarrito = carrito?.Detalles.Sum(d => d.Cantidad) ?? 0;
            }
            else
            {
                ViewBag.CantidadCarrito = 0;
            }
        }
    }
}