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
            var rol = HttpContext.Session.GetString("UsuarioRol");
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            IQueryable<Proyecto> proyectos = _context.Proyecto.Include(p => p.Usuario);

            if (rol == "Usuario" && int.TryParse(usuarioIdStr, out int usuarioId))
            {
                proyectos = proyectos.Where(p => p.UsuarioID == usuarioId);
            }

            return View(await proyectos.ToListAsync());
        }



        // GET: Proyecto/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var proyecto = await _context.Proyecto
                .Include(p => p.Usuario)
                .Include(p => p.PermisosInstalacion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (proyecto == null) return NotFound();

            return View(proyecto);
        }

        // GET: Proyecto/Create
        public IActionResult Create()
        {
            ViewData["UsuarioID"] = new SelectList(_context.Usuario, "Id", "NombreCompleto");
            return View();
        }

        // POST: Proyecto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,FechaInicio,FechaFin,Descripcion,Fotos,UsuarioID,estado,RequiereDocumentos")] Proyecto proyecto)
        {
            if (ModelState.IsValid)
            {
                proyecto.estado = "Pendiente";
                _context.Add(proyecto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["UsuarioID"] = new SelectList(_context.Usuario, "Id", "NombreCompleto", proyecto.UsuarioID);
            return View(proyecto);
        }

        // GET: Proyecto/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var proyecto = await _context.Proyecto.FindAsync(id);
            if (proyecto == null) return NotFound();

            ViewData["UsuarioID"] = new SelectList(_context.Usuario, "Id", "NombreCompleto", proyecto.UsuarioID);
            return View(proyecto);
        }

        // POST: Proyecto/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,estado,RequiereDocumentos")] Proyecto proyecto)
        {
            if (id != proyecto.Id)
                return NotFound();

            var proyectoExistente = await _context.Proyecto.FindAsync(id);
            if (proyectoExistente == null)
                return NotFound();

            // Actualizar únicamente los campos permitidos
            proyectoExistente.estado = proyecto.estado;
            proyectoExistente.RequiereDocumentos = proyecto.RequiereDocumentos;

            try
            {
                _context.Update(proyectoExistente);
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
