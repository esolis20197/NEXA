using System.ComponentModel.DataAnnotations;

namespace NEXA.Models
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public DateTime FechaPedido { get; set; } = DateTime.Now;

        public string Estado { get; set; } = "Pendiente";

        public ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
    }
}
