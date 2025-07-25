using System.ComponentModel.DataAnnotations;

namespace NEXA.Models
{
    public class CuentaPorPagar
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Motivo de la deuda")]

        public string NombreDeuda { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; }
        public decimal Monto { get; set; }
        public string Proveedor { get; set; }
        public DateTime PlazaParaPagar { get; set; }
    }
}
