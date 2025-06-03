using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NEXA.Models;
using NEXA.Filters;

namespace NEXA.Controllers
{
    [FiltroSeguridad]
    public class UsuariosController : Controller
    {
        private readonly NEXAContext _context;

        public UsuariosController(NEXAContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            return View(await _context.Usuario.ToListAsync());
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreUsuario,NombreCompleto,Correo,Cedula,Telefono,Contraseña,Rol")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreUsuario,NombreCompleto,Correo,Cedula,Telefono,Contraseña,Rol")] Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }



        public async Task<IActionResult> EditUsuario(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUsuario(Usuario model)
        {
            var usuarioDb = _context.Usuario.FirstOrDefault(u => u.Id == model.Id);

            if (usuarioDb != null)
            {
                usuarioDb.NombreUsuario = model.NombreUsuario;             
                usuarioDb.Correo = model.Correo;
                usuarioDb.Cedula = model.Cedula;
                usuarioDb.Telefono = model.Telefono;

                _context.SaveChanges();
                return RedirectToAction("Perfil");
            }

            return View(model);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuario.Remove(usuario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuario.Any(e => e.Id == id);
        }


        public async Task<IActionResult> Perfil()
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioIdStr) || !int.TryParse(usuarioIdStr, out int usuarioId))
            {
                return RedirectToAction("InicioSesion", "Acceso");
            }

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound("Usuario no encontrado ");
            }
            return View(usuario);
        }

        public async Task<IActionResult> EditContrasena(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditContrasena(int id, string ContrasenaActual, string NuevaContrasena, string ConfirmarContrasena)
        {
            var usuarioDb = _context.Usuario.FirstOrDefault(u => u.Id == id);

            if (usuarioDb != null)
            {
                if (usuarioDb.Contraseña != ContrasenaActual)
                {
                    ModelState.AddModelError("ContrasenaActual", "La contraseña actual es incorrecta.");
                    return View(usuarioDb);
                }

                if (NuevaContrasena != ConfirmarContrasena)
                {
                    ModelState.AddModelError("ConfirmarContrasena", "La nueva contraseña y la confirmación no coinciden.");
                    return View(usuarioDb);
                }

                usuarioDb.Contraseña = NuevaContrasena;

                _context.SaveChanges();
                return RedirectToAction("Perfil");
            }

            return View();
        }

    }
}