using System.ComponentModel.DataAnnotations;

namespace NEXA.Models
{
    public class Gasto
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Motivo del gasto")]
        public string NombreGasto { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; }
        public decimal Monto { get; set; }
        public string Categoria { get; set; }
    }
}
