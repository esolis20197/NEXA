using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXA.Models;
using NEXA.Filters;

namespace NEXA.Controllers
{
    [FiltroSeguridad]
    public class CuentasPorPagarController : BaseController
    {
        private readonly NEXAContext _context;
        public CuentasPorPagarController(NEXAContext context) : base(context)
        {
            _context = context;
        }

        // GET: CuentasPorPagar
        public async Task<IActionResult> Index()
        {
            return View(await _context.CuentasPorPagar.ToListAsync());
        }

        // GET: CuentasPorPagar/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cuenta = await _context.CuentasPorPagar.FirstOrDefaultAsync(m => m.Id == id);
            if (cuenta == null) return NotFound();

            return View(cuenta);
        }

        // GET: CuentasPorPagar/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreDeuda,Fecha,Descripcion,Monto,Proveedor,PlazaParaPagar,Estado,GastoId")] CuentaPorPagar cuentaPorPagar)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cuentaPorPagar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(cuentaPorPagar);
        }

        // GET: CuentasPorPagar/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cuentaPorPagar = await _context.CuentasPorPagar.FindAsync(id);
            if (cuentaPorPagar == null) return NotFound();

            return View(cuentaPorPagar);
        }

        // POST: CuentasPorPagar/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreDeuda,Fecha,Descripcion,Monto,Proveedor,PlazaParaPagar,Estado,GastoId")] CuentaPorPagar cuentaPorPagar)
        {
            if (id != cuentaPorPagar.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // Recupera Estado y GastoId originales (para no perderlos)
                var cuentaOriginal = await _context.CuentasPorPagar.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                if (cuentaOriginal != null)
                {
                    cuentaPorPagar.Estado = cuentaOriginal.Estado;
                    cuentaPorPagar.GastoId = cuentaOriginal.GastoId;
                }

                try
                {
                    _context.Update(cuentaPorPagar);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuentaPorPagarExists(cuentaPorPagar.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // DEPURACIÓN: Ver errores si hay
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key].Errors;
                foreach (var error in errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en {key}: {error.ErrorMessage}");
                }
            }

            return View(cuentaPorPagar);
        }

        // POST: CuentasPorPagar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cuentaPorPagar = await _context.CuentasPorPagar.FindAsync(id);

            // NO permitir eliminar si ya está asociada a un gasto
            if (cuentaPorPagar.GastoId != null)
            {
                TempData["ErrorMensaje"] = "No se puede eliminar esta cuenta porque ya fue pagada y está asociada a un gasto.";
                return RedirectToAction(nameof(Index));
            }

            _context.CuentasPorPagar.Remove(cuentaPorPagar);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CuentaPorPagarExists(int id)
        {
            return _context.CuentasPorPagar.Any(e => e.Id == id);
        }

        // GET: CuentasPorPagar/MarcarPagada/5
        public async Task<IActionResult> MarcarPagada(int? id)
        {
            if (id == null) return NotFound();

            var cuenta = await _context.CuentasPorPagar.FindAsync(id);
            if (cuenta == null || cuenta.Estado == "Pagada") return NotFound();

            ViewBag.Categorias = new List<string> { "Servicios", "Productos", "Transporte", "Alquiler", "Mantenimiento", "Planillas", "Publicidad", "Impuestos", "Otros" };
            return View(cuenta);
        }

        // POST: CuentasPorPagar/MarcarPagada/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarPagada(int id, string categoria, DateTime fechaPago)
        {
            var cuenta = await _context.CuentasPorPagar.FindAsync(id);
            if (cuenta == null || cuenta.Estado == "Pagada") return NotFound();

            var gasto = new Gasto
            {
                NombreGasto = cuenta.NombreDeuda,
                Fecha = fechaPago,
                Descripcion = $"{cuenta.Descripcion} (Proveedor: {cuenta.Proveedor})",
                Monto = cuenta.Monto,
                Categoria = categoria
            };

            _context.Gastos.Add(gasto);
            await _context.SaveChangesAsync();

            cuenta.Estado = "Pagada";
            cuenta.GastoId = gasto.Id;
            _context.CuentasPorPagar.Update(cuenta);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
