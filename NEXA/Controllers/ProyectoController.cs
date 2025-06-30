using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NEXA.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace NEXA.Controllers
{
    public class ProyectoController : BaseController
    {
        private readonly NEXAContext _context;

        public ProyectoController(NEXAContext context) : base(context)
        {
            _context = context;
        }

        // GET: Proyecto
        public async Task<IActionResult> Index()
        {
            var nEXAContext = _context.Proyecto.Include(p => p.Usuario);
            return View(await nEXAContext.ToListAsync());
        }

        // GET: Proyecto/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var proyecto = await _context.Proyecto
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (proyecto == null) return NotFound();

            return View(proyecto);
        }

        // GET: Proyecto/Create
        public IActionResult Create()
        {
            ViewData["UsuarioID"] = new SelectList(_context.Usuario, "Id", "NombreUsuario");
            return View();
        }

        // POST: Proyecto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,FechaInicio,FechaFin,Descripcion,Fotos,UsuarioID,estado")] Proyecto proyecto)
        {
            if (ModelState.IsValid)
            {
                proyecto.estado = "Pendiente";
                _context.Add(proyecto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["UsuarioID"] = new SelectList(_context.Usuario, "Id", "NombreUsuario", proyecto.UsuarioID);
            return View(proyecto);
        }

        // GET: Proyecto/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var proyecto = await _context.Proyecto.FindAsync(id);
            if (proyecto == null) return NotFound();

            ViewData["UsuarioID"] = new SelectList(_context.Usuario, "Id", "NombreUsuario", proyecto.UsuarioID);
            return View(proyecto);
        }

        // POST: Proyecto/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,FechaInicio,FechaFin,Descripcion,Fotos,UsuarioID,estado")] Proyecto proyecto)
        {
            if (id != proyecto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(proyecto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProyectoExists(proyecto.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["UsuarioID"] = new SelectList(_context.Usuario, "Id", "NombreUsuario", proyecto.UsuarioID);
            return View(proyecto);
        }

        // POST: Proyectos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proyecto = await _context.Proyecto.FindAsync(id);
            if (proyecto != null)
            {
                _context.Proyecto.Remove(proyecto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Proyectos/Finalizar/5
        [HttpPost]
        public async Task<IActionResult> Finalizar(int id)
        {
            var proyecto = await _context.Proyecto.FindAsync(id);
            if (proyecto == null)
                return NotFound();

            proyecto.estado = "Finalizado";
            _context.Update(proyecto);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool ProyectoExists(int id)
        {
            return _context.Proyecto.Any(e => e.Id == id);
        }
    }
}
