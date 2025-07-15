using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using NEXA.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using NEXA.Services;
using Microsoft.EntityFrameworkCore;

namespace NEXA.Controllers
{
        public class PermisosInstalacionController : Controller
        {
            private readonly NEXAContext _context;
            private readonly IWebHostEnvironment _env;
        private readonly EmailService _emailService;

        public PermisosInstalacionController(NEXAContext context, IWebHostEnvironment env, EmailService emailService)
        {
            _context = context;
            _env = env;
            _emailService = emailService;
        }

        [HttpPost]
            public async Task<IActionResult> Subir(IFormFile archivo, int ProyectoID)
            {
                if (archivo != null && archivo.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var nombreArchivo = Path.GetFileNameWithoutExtension(archivo.FileName);
                    var extension = Path.GetExtension(archivo.FileName);
                    var nombreFinal = archivo.FileName;
                    var ruta = Path.Combine(uploadsFolder, nombreFinal);

                    using (var stream = new FileStream(ruta, FileMode.Create))
                    {
                        await archivo.CopyToAsync(stream);
                    }

                    var permiso = new PermisoInstalacion
                    {
                        NombreArchivo = archivo.FileName,
                        RutaArchivo = "/uploads/" + nombreFinal,
                        ProyectoID = ProyectoID
                    };

                    _context.PermisosInstalacion.Add(permiso);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Details", "Proyecto", new { id = ProyectoID });
                }

                return BadRequest("Archivo inválido");
            }

        [HttpPost]
        public async Task<IActionResult> Aprobar(int id)
        {
            var permiso = await _context.PermisosInstalacion
                .Include(p => p.Proyecto)
                .ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (permiso == null) return NotFound();

            permiso.Estado = "Aprobado";
            await _context.SaveChangesAsync();

            // Enviar correo
            var usuario = permiso.Proyecto.Usuario;
            if (usuario != null)
            {
                string asunto = "Documento aprobado";
                string cuerpo = $"Hola {usuario.NombreCompleto},<br/>El documento <strong>{permiso.NombreArchivo}</strong> fue <strong>aprobado</strong> para el proyecto <strong>{permiso.Proyecto.Nombre}</strong>.";
                await _emailService.EnviarCorreoAsync(usuario.Correo, asunto, cuerpo);
            }

            return RedirectToAction("Details", "Proyecto", new { id = permiso.ProyectoID });
        }

        [HttpPost]
        public async Task<IActionResult> Rechazar(int id, string motivo)
        {
            var permiso = await _context.PermisosInstalacion
                .Include(p => p.Proyecto)
                .ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (permiso == null) return NotFound();

            permiso.Estado = "Rechazado";
            await _context.SaveChangesAsync();

            // Enviar correo
            var usuario = permiso.Proyecto.Usuario;
            if (usuario != null)
            {
                string asunto = "Documento rechazado";
                string cuerpo = $@"
            Hola {usuario.NombreCompleto},<br/>
            El documento <strong>{permiso.NombreArchivo}</strong> fue <strong>rechazado</strong> para el proyecto <strong>{permiso.Proyecto.Nombre}</strong>.<br/>
            <strong>Motivo:</strong> {motivo}<br/><br/>
            Por favor, suba uno nuevo.";

                await _emailService.EnviarCorreoAsync(usuario.Correo, asunto, cuerpo);
            }

            return RedirectToAction("Details", "Proyecto", new { id = permiso.ProyectoID });
        }

    }
}
