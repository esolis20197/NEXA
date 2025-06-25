using System.ComponentModel.DataAnnotations;

namespace NEXA.Models
{
    public class CarritoDetalle
    {
        [Key]
        public int Id { get; set; }

        public int CarritoId { get; set; }
        public Carrito Carrito { get; set; }

        public int InventarioId { get; set; }
        public Inventario Inventario { get; set; }

        public int Cantidad { get; set; }

        public int PrecioUnitario { get; set; }
    }
}
