using Microsoft.EntityFrameworkCore;
using NEXA.Models;

namespace NEXA.Services
{
    public class CorreoPromocionalJobService
    {
        private readonly NEXAContext _context;
        private readonly EmailService _emailService;

        public CorreoPromocionalJobService(NEXAContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task ProcesarCorreosPendientesAsync()
        {
            var correosPendientes = await _context.CorreosPromocionales
                .Where(c => c.Estado == "Pendiente" && c.FechaEnvio <= DateTime.Now)
                .ToListAsync();

            foreach (var correo in correosPendientes)
            {
                var plantilla = await _emailService.LeerPlantillaEmailAsync("PromocionalTemplate.html");

                var clientes = await _context.ClientesPorGrupos
                    .Where(cpg => cpg.GrupoId == correo.GrupoId)
                    .Select(cpg => cpg.Usuario)
                    .ToListAsync();

                foreach (var cliente in clientes)
                {
                    if (string.IsNullOrWhiteSpace(cliente.Correo))
                        continue;

                    var cuerpoHtml = plantilla.Replace("{{Asunto}}", correo.Asunto)
                                              .Replace("{{Contenido}}", correo.Contenido.Replace("\n", "<br>"));

                    try
                    {
                        await _emailService.EnviarCorreoAsync(cliente.Correo, correo.Asunto, cuerpoHtml);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error enviando a {cliente.Correo}: {ex.Message}");
                    }
                }

                correo.Estado = "Enviado";
            }

            await _context.SaveChangesAsync();
        }
    }
}
