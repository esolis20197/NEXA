using System.ComponentModel.DataAnnotations;

namespace NEXA.Models
{
    public class PedidoDetalle
    {
        [Key]
        public int Id { get; set; }

        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; }

        public int InventarioId { get; set; }
        public Inventario Inventario { get; set; }

        public int Cantidad { get; set; }

        public int PrecioUnitario { get; set; }
    }
}
