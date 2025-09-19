using System.ComponentModel.DataAnnotations;

namespace NEXA.Models
{
    public class Inventario
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Foto (URL)")]
        public string? FotoUrl { get; set; }

        [Display(Name = "Nombre del producto")]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; }

        [Display(Name = "Precio")]
        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El precio debe ser un valor positivo.")]
        public int Precio { get; set; }

        [Display(Name = "Stock disponible")]
        [Required(ErrorMessage = "El stock es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser un valor positivo.")]
        public int Stock { get; set; }

        [Display(Name = "Tipo de inventario")]
        [Required(ErrorMessage = "El tipo es obligatorio.")]
        public string tipo { get; set; } // Producto, Material, etc.
    }
}
