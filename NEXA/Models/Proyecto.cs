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

        [Required(ErrorMessage = "El campo 'Nombre' es obligatorio.")]
        public string Nombre { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Inicio")]
        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime FechaInicio { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Fin")]
        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        public DateTime FechaFin { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; }
        [Required(ErrorMessage = "La foto no es valida")]
        public string Fotos { get; set; }

        [ForeignKey("Usuario")]
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public int UsuarioID { get; set; }

        public string estado { get; set; } = "Pendiente";

        public Usuario? Usuario { get; set; }

        public bool RequiereDocumentos { get; set; } = false;

        public ICollection<PermisoInstalacion> PermisosInstalacion { get; set; } = new List<PermisoInstalacion>();

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
