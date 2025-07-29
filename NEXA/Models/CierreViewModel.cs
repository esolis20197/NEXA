using System;

namespace NEXA.ViewModels
{
    public class CierreViewModel
    {
        public DateTime FechaDesde { get; set; } = DateTime.Today;
        public DateTime FechaHasta { get; set; } = DateTime.Today;

        public string TipoCierre { get; set; }

        public decimal TotalGanancias { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal TotalCuentasPorPagarPendientes { get; set; }
        public decimal Balance => TotalGanancias - TotalGastos;

        public string RangoDescripcion { get; set; }
        public List<NEXA.Models.CuentaPorPagar> CuentasPendientesDetalle { get; set; } = new();
    }
}
