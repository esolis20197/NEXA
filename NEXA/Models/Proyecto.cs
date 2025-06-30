using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NEXA.Models;
using System.Linq;

namespace NEXA.Models
{
    public class Proyecto : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Inicio")]
        public DateTime FechaInicio { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Fin")]
        public DateTime FechaFin { get; set; }

        public string Descripcion { get; set; }

        public string Fotos { get; set; }

        [ForeignKey("Usuario")]
        public int UsuarioID { get; set; }

        public string estado { get; set; } = "Pendiente";

        public Usuario? Usuario { get; set; }

        // Validaciones personalizadas
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var _context = validationContext.GetService(typeof(NEXAContext)) as NEXAContext;

            if (_context != null)
            {
                // Verifica si ya existe un proyecto con el mismo nombre (ignorando el actual en edición)
                bool nombreDuplicado = _context.Proyecto
                    .Any(p => p.Nombre == this.Nombre && p.Id != this.Id);

                if (nombreDuplicado)
                {
                    yield return new ValidationResult(
                        "Ya existe un proyecto con este nombre.",
                        new[] { nameof(Nombre) });
                }
            }

            if (FechaInicio.Date < DateTime.Today)
            {
                yield return new ValidationResult(
                    "La fecha de inicio no puede ser en el pasado.",
                    new[] { nameof(FechaInicio) });
            }

            if (FechaFin.Date < FechaInicio.Date)
            {
                yield return new ValidationResult(
                    "La fecha de fin no puede ser anterior a la fecha de inicio.",
                    new[] { nameof(FechaFin) });
            }
        }
    }
}
