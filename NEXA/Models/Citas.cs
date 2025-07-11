using System;
using System.ComponentModel.DataAnnotations;

namespace NEXA.Models
{
    public class Citas
    {
        public int Id { get; set; }

        public string NombreCita { get; set; }

        public string TipoCita { get; set; } // Soporte - Mantenimiento

        public string Encargado { get; set; } = "Definir encargado";

        public DateTime FechaCita { get; set; }

        public string Detalles { get; set; } // Detalles importantes sobre la cita

        [Required(ErrorMessage = "El número telefónico es obligatorio.")]
        [RegularExpression(@"^\d{8,10}$", ErrorMessage = "Debe ingresar un número válido de 8 a 10 dígitos.")]
        public int NumeroTelefonico { get; set; }

        public string ProducConsulta { get; set; } // Producto por el cual se solicita la cita

        public string Direccion { get; set; } // Nueva propiedad agregada

        public string Estado { get; set; } = "Pendiente"; // Pendiente - Aprobado - Rechazado

        public int? UsuarioID { get; set; }

        public Usuario? Usuario { get; set; }
    }
}
