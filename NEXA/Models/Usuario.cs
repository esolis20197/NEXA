using System.ComponentModel.DataAnnotations;

namespace NEXA.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        public required string NombreUsuario { get; set; }
        public required string NombreCompleto { get; set; }
        public required string Correo { get; set; }
        public required string Cedula { get; set; }

        public required string Telefono { get; set; }
        public required string Contraseña { get; set; }
        public required string Rol { get; set; }  // Administrador, Usuario

        public ICollection<Proyecto>? Proyectos { get; set; }
        public ICollection<Citas>? Citas { get; set; }

    }
}
