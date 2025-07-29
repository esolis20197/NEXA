using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXA.Models;
using NEXA.Filters;

namespace NEXA.Controllers
{
    [FiltroSeguridad]
    public class GastosController : BaseController
    {
        private readonly NEXAContext _context;

        public GastosController(NEXAContext context) : base(context)
        {
            _context = context;
        }

        // GET: Gastos
        public async Task<IActionResult> Index()
        {
            return View(await _context.Gastos.ToListAsync());
        }

        // GET: Gastos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var gasto = await _context.Gastos.FirstOrDefaultAsync(m => m.Id == id);
            if (gasto == null) return NotFound();

            return View(gasto);
        }

        // GET: Gastos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Gastos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreGasto,Fecha,Descripcion,Monto,Categoria")] Gasto gasto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gasto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(gasto);
        }

        // GET: Gastos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var gasto = await _context.Gastos.FindAsync(id);
            if (gasto == null) return NotFound();

            bool ligado = await _context.CuentasPorPagar.AnyAsync(c => c.GastoId == id);

            if (ligado)
            {
                TempData["ErrorMensaje"] = $"No se puede editar el gasto \"{gasto.NombreGasto}\" porque está vinculado a una cuenta por pagar.";
                return RedirectToAction(nameof(Index));
            }

            return View(gasto);
        }

        // POST: Gastos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreGasto,Fecha,Descripcion,Monto,Categoria")] Gasto gasto)
        {
            if (id != gasto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gasto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GastoExists(gasto.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(gasto);
        }

        // POST: Gastos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gasto = await _context.Gastos.FindAsync(id);

            if (gasto == null)
            {
                TempData["ErrorMensaje"] = "El gasto no fue encontrado.";
                return RedirectToAction(nameof(Index));
            }

            bool estaLigado = await _context.CuentasPorPagar.AnyAsync(c => c.GastoId == id);

            if (estaLigado)
            {
                TempData["ErrorMensaje"] = $"No se puede eliminar el gasto \"{gasto.NombreGasto}\" porque está vinculado a una cuenta por pagar.";
                return RedirectToAction(nameof(Index));
            }

            _context.Gastos.Remove(gasto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GastoExists(int id)
        {
            return _context.Gastos.Any(e => e.Id == id);
        }
    }
}
