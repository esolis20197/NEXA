using System;
using System.ComponentModel.DataAnnotations;

namespace NEXA.Models
{
    public class ReporteHistorial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TipoReporte { get; set; }  // Ej: "Pedidos", "Inventario"

        public string FiltrosAplicados { get; set; }  // JSON o texto plano con filtros usados

        public DateTime FechaGeneracion { get; set; } = DateTime.Now;
    }
}

