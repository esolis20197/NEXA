using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXA.Models;

namespace NEXA.Controllers
{
    public class CitasController : BaseController
    {
        private readonly NEXAContext _context;

        public CitasController(NEXAContext context) : base(context)
        {
            _context = context;
        }

        // GET: Citas
        public async Task<IActionResult> Index(string estado = null)
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            var rolUsuario = HttpContext.Session.GetString("UsuarioRol");

            if (string.IsNullOrEmpty(usuarioIdStr) || !int.TryParse(usuarioIdStr, out int usuarioId))
            {
                TempData["Mensaje"] = "Debe iniciar sesión para ver sus citas.";
                return RedirectToAction("InicioSesion", "Acceso");
            }

            ViewBag.UsuarioRol = rolUsuario;
            ViewBag.EstadoSeleccionado = estado;

            IQueryable<Citas> citasQuery = _context.Citas.Include(c => c.Usuario);

            // Si no es administrador, solo puede ver sus propias citas
            if (rolUsuario != "Administrador")
            {
                citasQuery = citasQuery.Where(c => c.UsuarioID == usuarioId);
            }

            // Si se especifica estado, filtramos por ese estado
            if (!string.IsNullOrEmpty(estado))
            {
                citasQuery = citasQuery.Where(c => c.Estado == estado);
            }

            var citas = await citasQuery.ToListAsync();
            return View(citas);
        }



        // GET: Citas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cita = await _context.Citas
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null) return NotFound();

            return View(cita);
        }

        // GET: Citas/Create
        public IActionResult Create()
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioIdStr))
            {
                TempData["Mensaje"] = "Debe iniciar sesión para crear una cita.";
                return RedirectToAction("InicioSesion", "Acceso");
            }

            return View();
        }

        // POST: Citas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NombreCita,TipoCita,Encargado,FechaCita,Detalles,NumeroTelefonico,ProducConsulta,Direccion")] Citas citas)
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioIdStr) || !int.TryParse(usuarioIdStr, out int usuarioId))
            {
                TempData["Mensaje"] = "Debe iniciar sesión para crear una cita.";
                return RedirectToAction("InicioSesion", "Acceso");
            }

            citas.UsuarioID = usuarioId;

            if (ModelState.IsValid)
            {
                citas.Estado = "Pendiente";
                citas.Encargado = "Definir Encargado";
                _context.Add(citas);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(citas);
        }

        // GET: Citas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var rolUsuario = HttpContext.Session.GetString("UsuarioRol");

            if (rolUsuario != "Administrador")
            {
                TempData["Mensaje"] = "No tiene permiso para editar citas.";
                return RedirectToAction(nameof(Index));
            }

            if (id == null) return NotFound();

            var cita = await _context.Citas
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null) return NotFound();

            return View(cita);
        }

        // POST: Citas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreCita,TipoCita,Encargado,FechaCita,Detalles,NumeroTelefonico,ProducConsulta,Direccion,Estado")] Citas citaEditada)
        {
            var rolUsuario = HttpContext.Session.GetString("UsuarioRol");

            if (rolUsuario != "Administrador")
            {
                TempData["Mensaje"] = "No tiene permiso para editar citas.";
                return RedirectToAction(nameof(Index));
            }

            if (id != citaEditada.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var citaOriginal = await _context.Citas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (citaOriginal == null) return NotFound();

                    citaEditada.UsuarioID = citaOriginal.UsuarioID;

                    _context.Update(citaEditada);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CitaExists(citaEditada.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(citaEditada);
        }

        // POST: Citas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rolUsuario = HttpContext.Session.GetString("UsuarioRol");

            if (rolUsuario != "Administrador")
            {
                TempData["Mensaje"] = "No tiene permiso para eliminar citas.";
                return RedirectToAction(nameof(Index));
            }

            var cita = await _context.Citas.FindAsync(id);
            if (cita != null)
            {
                _context.Citas.Remove(cita);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CitaExists(int id)
        {
            return _context.Citas.Any(e => e.Id == id);
        }
    }
}
