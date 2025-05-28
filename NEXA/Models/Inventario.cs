
using System.ComponentModel.DataAnnotations;

namespace NEXA.Models
{
    public class Inventario
    {
        [Key]
        public int Id { get; set; }

        public string? FotoUrl { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public int Precio { get; set; }

        public int Stock { get; set; }

        public string tipo { get; set; } //Tipo, Producto, Material.



    }
}
