using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXA.Filters;
using NEXA.Models;

namespace NEXA.Controllers
{
    [FiltroSeguridad]
    public class CarritoController : BaseController
    {
        public CarritoController(NEXAContext context) : base(context) { }

        // POST: /Carrito/Agregar
        [HttpPost]
        public async Task<IActionResult> Agregar(int inventarioId, int cantidad)
        {
            if (cantidad <= 0) return BadRequest("Cantidad inválida.");

            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            if (usuarioIdStr == null || !int.TryParse(usuarioIdStr, out int usuarioId))
                return Unauthorized();

            var inventario = await _context.Inventario.FindAsync(inventarioId);
            if (inventario == null || inventario.Stock < cantidad)
                return BadRequest("Stock insuficiente.");

            var carrito = await _context.Carritos
                .Include(c => c.Detalles)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carrito == null)
            {
                carrito = new Carrito { UsuarioId = usuarioId };
                _context.Carritos.Add(carrito);
                await _context.SaveChangesAsync();
            }

            var detalle = carrito.Detalles.FirstOrDefault(d => d.InventarioId == inventarioId);
            if (detalle != null)
            {
                if (detalle.Cantidad + cantidad > inventario.Stock)
                    return BadRequest("Excede el stock disponible.");

                detalle.Cantidad += cantidad;
            }
            else
            {
                carrito.Detalles.Add(new CarritoDetalle
                {
                    InventarioId = inventarioId,
                    Cantidad = cantidad,
                    PrecioUnitario = inventario.Precio
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Carrito");
        }

        public async Task<IActionResult> Index()
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            if (usuarioIdStr == null || !int.TryParse(usuarioIdStr, out int usuarioId))
                return Unauthorized();

            var carrito = await _context.Carritos
                .Include(c => c.Detalles)
                .ThenInclude(d => d.Inventario)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            return View(carrito);
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var detalle = await _context.CarritoDetalles.FindAsync(id);
            if (detalle != null)
            {
                _context.CarritoDetalles.Remove(detalle);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarCantidad(int id, int cantidad)
        {
            if (cantidad <= 0) return RedirectToAction("Index");

            var detalle = await _context.CarritoDetalles
                .Include(d => d.Inventario)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (detalle != null && cantidad <= detalle.Inventario.Stock)
            {
                detalle.Cantidad = cantidad;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
