using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NEXA.Models
{
    public class ClientesPorGrupo
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("GrupoCliente")]
        public int GrupoId { get; set; }
        public GrupoCliente GrupoCliente { get; set; }

        [ForeignKey("Usuario")]
        public int ClienteId { get; set; }
        public Usuario Usuario { get; set; }
    }
}
