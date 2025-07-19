using System.ComponentModel.DataAnnotations;

namespace NEXA.Models
{
    public class GrupoCliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NombreGrupo { get; set; }

        public string Descripcion { get; set; }

        public ICollection<ClientesPorGrupo> ClientesPorGrupo { get; set; }

        public ICollection<CorreosPromocional> CorreosPromocionales { get; set; }
    }
}
