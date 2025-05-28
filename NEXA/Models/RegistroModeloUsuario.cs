using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace NEXA.Models
{
    public class RegistroModeloUsuario
    {
        public string NombreUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Cedula { get; set; }
        public string Contraseña { get; set; }
        public string Rol { get; set; } = "Usuario";  // Administrador, Usuario
    }

}
