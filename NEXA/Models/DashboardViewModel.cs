using DocumentFormat.OpenXml.Bibliography;

namespace NEXA.Models
{
    public class DashboardViewModel
    {
        public int TotalUsuarios { get; set; }
        public int TotalInventario { get; set; }
        public int TotalPedidos { get; set; }
        public int TotalCitasPendientes { get; set; }
        public int TotalPedidosPendientes { get; set; }
        public int TotalProyectosPendientes { get; set; }
        public string Rol { get; set; }
    }

}
