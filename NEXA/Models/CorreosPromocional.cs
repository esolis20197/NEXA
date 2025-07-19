using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NEXA.Models
{
    public class CorreosPromocional
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El asunto es obligatorio")]
        [StringLength(100, ErrorMessage = "El asunto no puede exceder 100 caracteres")]
        public string Asunto { get; set; }

        [Required(ErrorMessage = "El contenido es obligatorio")]
        public string Contenido { get; set; }

        [Required(ErrorMessage = "La fecha de envío es obligatoria")]
        [DataType(DataType.DateTime)]
        public DateTime FechaEnvio { get; set; }

        [Required]
        public string Estado { get; set; } = "Programado"; // Programado o Enviado

        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [ForeignKey("GrupoCliente")]
        [Required(ErrorMessage = "Debe seleccionar un grupo de clientes")]
        public int GrupoId { get; set; }
        public GrupoCliente? GrupoCliente { get; set; }

    }
}
