using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace NEXA.Models
{
    public class CuentaPorPagar
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la deuda es obligatorio.")]
        [Display(Name = "Motivo de la deuda")]
        public string NombreDeuda { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero.")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El proveedor es obligatorio.")]
        public string Proveedor { get; set; }

        [Required(ErrorMessage = "La fecha límite para pagar es obligatoria.")]
        [Display(Name = "Plazo para pagar")]
        public DateTime PlazaParaPagar { get; set; }

        public string Estado { get; set; }

        public int? GastoId { get; set; }
        [ValidateNever]
        public virtual Gasto Gasto { get; set; }
    }
}
