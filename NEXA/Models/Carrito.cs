using System.ComponentModel.DataAnnotations;

namespace NEXA.Models
{
    public class Carrito
    {
        [Key]
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public ICollection<CarritoDetalle> Detalles { get; set; } = new List<CarritoDetalle>();
    }
}
