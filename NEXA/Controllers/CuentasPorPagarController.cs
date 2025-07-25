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

        // POST: CuentasPorPagar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreDeuda,Fecha,Descripcion,Monto,Proveedor,PlazaParaPagar")] CuentaPorPagar cuentaPorPagar)
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreDeuda,Fecha,Descripcion,Monto,Proveedor,PlazaParaPagar")] CuentaPorPagar cuentaPorPagar)
        {
            if (id != cuentaPorPagar.Id) return NotFound();

            if (ModelState.IsValid)
            {
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
            return View(cuentaPorPagar);
        }

        // POST: CuentasPorPagar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cuentaPorPagar = await _context.CuentasPorPagar.FindAsync(id);
            if (cuentaPorPagar != null)
            {
                _context.CuentasPorPagar.Remove(cuentaPorPagar);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CuentaPorPagarExists(int id)
        {
            return _context.CuentasPorPagar.Any(e => e.Id == id);
        }
    }
}
