using Microsoft.Extensions.Options;
using NEXA.Models;
using System.Net;
using System.Net.Mail;

namespace NEXA.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            var mensaje = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };

            mensaje.To.Add(destinatario);

            using var smtp = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mensaje);
        }

        public async Task<string> LeerPlantillaEmailAsync(string nombreArchivo)
        {
            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "Templates", nombreArchivo);
            if (!File.Exists(ruta))
                throw new FileNotFoundException($"Plantilla no encontrada: {ruta}");

            return await File.ReadAllTextAsync(ruta);
        }

    }
}
