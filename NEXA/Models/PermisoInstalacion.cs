using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NEXA.Models
{
    public class PermisoInstalacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NombreArchivo { get; set; }

        public string RutaArchivo { get; set; }

        [Required]
        public string Estado { get; set; } = "Pendiente";

        [DataType(DataType.DateTime)]
        public DateTime FechaSubida { get; set; } = DateTime.Now;

        [ForeignKey("Proyecto")]
        public int ProyectoID { get; set; }

        public Proyecto Proyecto { get; set; }
    }

}
