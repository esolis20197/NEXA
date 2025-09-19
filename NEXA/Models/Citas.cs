using System;
using System.ComponentModel.DataAnnotations;

namespace NEXA.Models
{
    public class Citas
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Nombre de la cita")]
        [Required(ErrorMessage = "El nombre de la cita es obligatorio.")]
        public string NombreCita { get; set; }

        [Display(Name = "Tipo de cita")]
        [Required(ErrorMessage = "El tipo de cita es obligatorio.")]
        public string TipoCita { get; set; } // Soporte - Mantenimiento

        [Display(Name = "Encargado")]
        public string Encargado { get; set; } = "Definir encargado";

        [Display(Name = "Fecha de la cita")]
        [Required(ErrorMessage = "La fecha de la cita es obligatoria.")]
        public DateTime FechaCita { get; set; }

        [Display(Name = "Detalles")]
        [Required(ErrorMessage = "Los detalles de la cita son obligatorios.")]
        public string Detalles { get; set; }

        [Display(Name = "Número telefónico")]
        [Required(ErrorMessage = "El número telefónico es obligatorio.")]
        [RegularExpression(@"^\d{8,10}$", ErrorMessage = "Debe ingresar un número válido de 8 a 10 dígitos.")]
        public int NumeroTelefonico { get; set; }

        [Display(Name = "Producto de consulta")]
        [Required(ErrorMessage = "Debe especificar el producto de consulta.")]
        public string ProducConsulta { get; set; }

        [Display(Name = "Dirección")]
        [Required(ErrorMessage = "La dirección es obligatoria.")]
        public string Direccion { get; set; }

        [Display(Name = "Estado de la cita")]
        public string Estado { get; set; } = "Pendiente"; // Pendiente - Aprobado - Rechazado

        [Display(Name = "Usuario asignado")]
        public int? UsuarioID { get; set; }

        public Usuario? Usuario { get; set; }
    }
}
