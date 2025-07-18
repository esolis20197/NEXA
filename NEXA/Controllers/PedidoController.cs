using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXA.Filters;
using NEXA.Models;

namespace NEXA.Controllers
{
    [FiltroSeguridad]
    public class PedidoController : BaseController
    {
        public PedidoController(NEXAContext context) : base(context) { }

        [HttpPost]
        public async Task<IActionResult> Crear()
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            if (usuarioIdStr == null || !int.TryParse(usuarioIdStr, out int usuarioId))
                return Unauthorized();

            var carrito = await _context.Carritos
                .Include(c => c.Detalles)
                .ThenInclude(d => d.Inventario)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carrito == null || !carrito.Detalles.Any())
                return RedirectToAction("Index", "Carrito");

            foreach (var detalle in carrito.Detalles)
            {
                if (detalle.Cantidad > detalle.Inventario.Stock)
                    return BadRequest($"No hay suficiente stock para {detalle.Inventario.Nombre}");
            }

            var pedido = new Pedido
            {
                UsuarioId = usuarioId,
                Estado = "Pendiente",
                FechaPedido = DateTime.Now,
                Detalles = carrito.Detalles.Select(d => new PedidoDetalle
                {
                    InventarioId = d.InventarioId,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                }).ToList()
            };

            foreach (var detalle in carrito.Detalles)
            {
                detalle.Inventario.Stock -= detalle.Cantidad;
            }

            _context.Pedidos.Add(pedido);
            _context.Carritos.Remove(carrito);
            await _context.SaveChangesAsync();

            return RedirectToAction("Confirmacion");
        }

        public IActionResult Confirmacion()
        {
            return View();
        }

        // Muestra todas por defecto
        public async Task<IActionResult> Historial(string estado = null)
        {            
            ViewBag.EstadoSeleccionado = estado;

            IQueryable<Pedido> pedidosQuery = _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Inventario);

            // Si se especifica estado, filtramos por ese estado
            if (!string.IsNullOrEmpty(estado))
            {
                pedidosQuery = pedidosQuery.Where(p => p.Estado == estado);
            }

            var pedidos = await pedidosQuery
                .OrderBy(p => p.FechaPedido)
                .ToListAsync();

            return View(pedidos);
        }



        public async Task<IActionResult> Ver(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Inventario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            return View(pedido);
        }

        [HttpPost]
        public async Task<IActionResult> Completar(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            pedido.Estado = "Completado";
            await _context.SaveChangesAsync();

            return RedirectToAction("Historial");
        }

        [HttpPost]
        public async Task<IActionResult> Rechazar(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Inventario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            pedido.Estado = "Rechazado";

            foreach (var detalle in pedido.Detalles)
            {
                detalle.Inventario.Stock += detalle.Cantidad;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Historial");

        }
    }
}
