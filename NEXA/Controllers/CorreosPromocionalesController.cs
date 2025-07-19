using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NEXA.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NEXA.Controllers
{
    public class CorreosPromocionalesController : BaseController
    {
        private readonly NEXAContext _context;

        public CorreosPromocionalesController(NEXAContext context) : base(context)
        {
            _context = context;
        }

        // GET: CorreosPromocionales
        public async Task<IActionResult> Index()
        {
            var correos = await _context.CorreosPromocionales
                .Include(c => c.GrupoCliente)
                .ToListAsync();

            return View(correos);
        }

        // GET: CorreosPromocionales/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var correo = await _context.CorreosPromocionales
                .Include(c => c.GrupoCliente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (correo == null) return NotFound();

            return View(correo);
        }

        // GET: CorreosPromocionales/Create
        public IActionResult Create()
        {            
            ViewData["GrupoId"] = new SelectList(_context.GrupoClientes, "Id", "NombreGrupo");
            return View();
        }

        // POST: CorreosPromocionales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Asunto,Contenido,FechaEnvio,,GrupoId")] CorreosPromocional correo)
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (ModelState.IsValid)
            {
                // Seteos automáticos
                correo.Estado = "Pendiente";                
                correo.UsuarioId = int.Parse(usuarioIdStr); ;

                _context.Add(correo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["GrupoId"] = new SelectList(_context.GrupoClientes, "Id", "NombreGrupo", correo.GrupoId);
            return View(correo);
        }

        // GET: CorreosPromocionales/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var correo = await _context.CorreosPromocionales.FindAsync(id);

            if (correo == null || correo.Estado == "Enviado")
            {
                TempData["Mensaje"] = "No se puede editar un correo ya enviado.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["GrupoId"] = new SelectList(_context.GrupoClientes, "Id", "NombreGrupo", correo.GrupoId);
            return View(correo);
        }

        // POST: CorreosPromocionales/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Asunto,Contenido,FechaEnvio,Estado,GrupoId")] CorreosPromocional correoEditado)
        {
            if (id != correoEditado.Id)
                return NotFound();

            var original = await _context.CorreosPromocionales.AsNoTracking()
                              .FirstOrDefaultAsync(c => c.Id == id);

            if (original == null)
                return NotFound();           

            if (!ModelState.IsValid)
            {
                ViewData["GrupoId"] = new SelectList(_context.GrupoClientes, "Id", "NombreGrupo", correoEditado.GrupoId);
                return View(correoEditado);
            }

            try
            {
                var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");                

                correoEditado.UsuarioId = int.Parse(usuarioIdStr);

                // Actualiza el registro con el modelo recibido
                _context.Update(correoEditado);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Correo promocional actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.CorreosPromocionales.Any(e => e.Id == correoEditado.Id))
                    return NotFound();
                else
                    throw;
            }
        }


        // POST: CorreosPromocionales/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var correo = await _context.CorreosPromocionales.FindAsync(id);

            if (correo == null || correo.Estado == "Enviado")
            {
                TempData["Mensaje"] = "No se puede eliminar un correo ya enviado.";
                return RedirectToAction(nameof(Index));
            }

            _context.CorreosPromocionales.Remove(correo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool CorreoExists(int id)
        {
            return _context.CorreosPromocionales.Any(e => e.Id == id);
        }
    }
}
