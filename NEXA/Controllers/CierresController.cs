using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXA.Filters;
using NEXA.Models;
using NEXA.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NEXA.Controllers
{
    [FiltroSeguridad]
    public class CierresController : BaseController
    {
        private readonly NEXAContext _context;

        public CierresController(NEXAContext context) : base(context)
        {
            _context = context;
        }

        // GET: Cierres
        public IActionResult Index()
        {
            return View(new CierreViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(CierreViewModel model)
        {
            DateTime fechaDesde, fechaHasta;
            var hoy = DateTime.Today;

            switch (model.TipoCierre)
            {
                case "Diario":
                    fechaDesde = hoy;
                    fechaHasta = hoy;
                    model.RangoDescripcion = $"Cierre diario: {hoy:dddd, dd MMMM yyyy}";
                    break;
                case "Semanal":
                    int diff = (7 + (hoy.DayOfWeek - DayOfWeek.Monday)) % 7;
                    fechaDesde = hoy.AddDays(-diff);
                    fechaHasta = fechaDesde.AddDays(6);
                    model.RangoDescripcion = $"Cierre semanal: {fechaDesde:dd MMM yyyy} al {fechaHasta:dd MMM yyyy}";
                    break;
                case "Mensual":
                    fechaDesde = new DateTime(hoy.Year, hoy.Month, 1);
                    fechaHasta = hoy;
                    var mes = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(hoy.Month);
                    model.RangoDescripcion = $"Cierre mensual: {mes} {hoy.Year}";
                    break;
                case "Anual":
                    fechaDesde = new DateTime(hoy.Year, 1, 1);
                    fechaHasta = hoy;
                    model.RangoDescripcion = $"Cierre anual: {hoy.Year}";
                    break;
                case "Rango":
                    fechaDesde = model.FechaDesde.Date;
                    fechaHasta = model.FechaHasta.Date;
                    model.RangoDescripcion = $"Cierre del {fechaDesde:dd/MM/yyyy} al {fechaHasta:dd/MM/yyyy}";
                    break;
                default:
                    fechaDesde = hoy;
                    fechaHasta = hoy;
                    model.RangoDescripcion = $"Cierre diario: {hoy:dddd, dd MMMM yyyy}";
                    break;
            }

            var fechaHastaQuery = fechaHasta.Date.AddDays(1).AddSeconds(-1);

            var ingresos = await _context.Pedidos
                .Include(p => p.Detalles)
                .Where(p => p.FechaPedido >= fechaDesde && p.FechaPedido <= fechaHastaQuery && p.Estado == "Completado")
                .SelectMany(p => p.Detalles)
                .SumAsync(d => d.Cantidad * d.PrecioUnitario);

            var gastos = await _context.Gastos
                .Where(g => g.Fecha >= fechaDesde && g.Fecha <= fechaHastaQuery)
                .SumAsync(g => g.Monto);

            var cuentasPendientes = await _context.CuentasPorPagar
                .Where(c => c.Fecha >= fechaDesde && c.Fecha <= fechaHastaQuery && c.Estado == "Pendiente")
                .SumAsync(c => c.Monto);

            model.FechaDesde = fechaDesde.Date;
            model.FechaHasta = fechaHasta.Date;
            model.TotalGanancias = ingresos;
            model.TotalGastos = gastos;
            model.TotalCuentasPorPagarPendientes = cuentasPendientes;

            model.CuentasPendientesDetalle = await _context.CuentasPorPagar
                .Where(c => c.Fecha >= fechaDesde && c.Fecha <= fechaHastaQuery && c.Estado == "Pendiente")
                .ToListAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ExportarExcel(string TipoCierre, DateTime FechaDesde, DateTime FechaHasta)
        {
            var fechaHastaQuery = FechaHasta.Date.AddDays(1).AddSeconds(-1);

            var ingresos = await _context.Pedidos
                .Include(p => p.Detalles)
                .Where(p => p.FechaPedido >= FechaDesde && p.FechaPedido <= fechaHastaQuery && p.Estado == "Completado")
                .SelectMany(p => p.Detalles)
                .SumAsync(d => d.Cantidad * d.PrecioUnitario);

            var gastos = await _context.Gastos
                .Where(g => g.Fecha >= FechaDesde && g.Fecha <= fechaHastaQuery)
                .SumAsync(g => g.Monto);

            var cuentasPendientes = await _context.CuentasPorPagar
                .Where(c => c.Fecha >= FechaDesde && c.Fecha <= fechaHastaQuery && c.Estado == "Pendiente")
                .ToListAsync();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Cierre");

                // TÍTULO PRINCIPAL
                var titulo = worksheet.Range(1, 1, 1, 5);
                titulo.Merge();
                titulo.Value = "NEXA";
                titulo.Style.Font.Bold = true;
                titulo.Style.Font.FontSize = 28;
                titulo.Style.Font.FontColor = XLColor.White;
                titulo.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 70, 127);
                titulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Row(1).Height = 40;

                // SUBTÍTULO
                worksheet.Cell(2, 1).Value = $"Cierre {TipoCierre}";
                worksheet.Cell(3, 1).Value = $"Desde: {FechaDesde:dd/MM/yyyy}";
                worksheet.Cell(3, 2).Value = $"Hasta: {FechaHasta:dd/MM/yyyy}";

                // RESUMEN
                worksheet.Cell(5, 1).Value = "Ganancias";
                worksheet.Cell(5, 2).Value = ingresos;

                worksheet.Cell(6, 1).Value = "Gastos";
                worksheet.Cell(6, 2).Value = gastos;

                worksheet.Cell(7, 1).Value = "Cuentas por Pagar Pendientes";
                worksheet.Cell(7, 2).Value = cuentasPendientes.Sum(c => c.Monto);

                // DETALLE
                worksheet.Cell(9, 1).Value = "Detalle Cuentas por Pagar";
                worksheet.Cell(9, 1).Style.Font.Bold = true;

                // ENCABEZADOS DE LA TABLA
                string[] headers = { "Motivo", "Descripción", "Proveedor", "Monto", "Plazo para pagar" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(10, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // DATOS
                int row = 11;
                foreach (var item in cuentasPendientes)
                {
                    worksheet.Cell(row, 1).Value = item.NombreDeuda;
                    worksheet.Cell(row, 2).Value = item.Descripcion;
                    worksheet.Cell(row, 3).Value = item.Proveedor;
                    worksheet.Cell(row, 4).Value = item.Monto;
                    worksheet.Cell(row, 5).Value = item.PlazaParaPagar.ToString("dd/MM/yyyy");
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var nombreArchivo = $"Cierre_{TipoCierre}_{FechaDesde:yyyyMMdd}_al_{FechaHasta:yyyyMMdd}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
                }
            }

        }

        [HttpPost]
        public async Task<IActionResult> ExportarPdf(string TipoCierre, DateTime FechaDesde, DateTime FechaHasta)
        {
            var fechaHastaQuery = FechaHasta.Date.AddDays(1).AddSeconds(-1);

            var ingresos = await _context.Pedidos
                .Include(p => p.Detalles)
                .Where(p => p.FechaPedido >= FechaDesde && p.FechaPedido <= fechaHastaQuery && p.Estado == "Completado")
                .SelectMany(p => p.Detalles)
                .SumAsync(d => d.Cantidad * d.PrecioUnitario);

            var gastos = await _context.Gastos
                .Where(g => g.Fecha >= FechaDesde && g.Fecha <= fechaHastaQuery)
                .SumAsync(g => g.Monto);

            var cuentasPendientes = await _context.CuentasPorPagar
                .Where(c => c.Fecha >= FechaDesde && c.Fecha <= fechaHastaQuery && c.Estado == "Pendiente")
                .ToListAsync();

            using (var ms = new System.IO.MemoryStream())
            {
                var doc = new Document(PageSize.A4, 40, 40, 40, 40);
                var writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Colores y fuentes
                var colorPrincipal = new BaseColor(33, 150, 243); // azul
                var fondoAzul = new BaseColor(0, 70, 127);        // azul oscuro nexa
                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, colorPrincipal);
                var fontSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.BLACK);
                var fontTablaHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.WHITE);
                var fondoHeaderTabla = new BaseColor(51, 122, 183);

                // NUEVO: Encabezado "NEXA" centrado con fondo azul oscuro
                var encabezado = new PdfPTable(1)
                {
                    WidthPercentage = 100,
                    SpacingAfter = 20f
                };
                var celdaEncabezado = new PdfPCell(new Phrase("NEXA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 28, BaseColor.WHITE)))
                {
                    BackgroundColor = fondoAzul,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Border = PdfPCell.NO_BORDER,
                    FixedHeight = 45f
                };
                encabezado.AddCell(celdaEncabezado);
                doc.Add(encabezado);

                // Título centrado
                var titulo = new Paragraph($"Cierre {TipoCierre}", fontTitulo)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 12f,
                    SpacingBefore = 8f
                };
                doc.Add(titulo);

                // Fechas
                var fechas = new Paragraph($"Desde: {FechaDesde:dd/MM/yyyy}    Hasta: {FechaHasta:dd/MM/yyyy}", fontNormal)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 18f
                };
                doc.Add(fechas);

                // Sección resumen
                doc.Add(new Paragraph("Resumen del cierre", fontSubtitulo) { SpacingAfter = 7f });
                var resumen = new PdfPTable(2)
                {
                    WidthPercentage = 50,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    SpacingAfter = 18f
                };
                resumen.DefaultCell.Border = PdfPCell.NO_BORDER;
                resumen.AddCell(new Phrase("Ganancias:", fontNormal));
                resumen.AddCell(new Phrase("₡" + ingresos.ToString("N2"), fontNormal));
                resumen.AddCell(new Phrase("Gastos:", fontNormal));
                resumen.AddCell(new Phrase("₡" + gastos.ToString("N2"), fontNormal));
                resumen.AddCell(new Phrase("Cuentas por pagar pendientes:", fontNormal));
                resumen.AddCell(new Phrase("₡" + cuentasPendientes.Sum(c => c.Monto).ToString("N2"), fontNormal));
                resumen.AddCell(new Phrase("Balance Neto:", fontSubtitulo));
                resumen.AddCell(new Phrase("₡" + (ingresos - gastos).ToString("N2"), fontSubtitulo));
                doc.Add(resumen);

                // Línea divisoria
                var separator = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, colorPrincipal, Element.ALIGN_CENTER, 1)));
                doc.Add(separator);
                doc.Add(new Paragraph("\n"));

                // Tabla de detalle de cuentas por pagar
                doc.Add(new Paragraph("Detalle de Cuentas por Pagar Pendientes", fontSubtitulo) { SpacingAfter = 8f });
                PdfPTable table = new PdfPTable(5)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 5f
                };
                float[] widths = { 20f, 28f, 18f, 12f, 18f };
                table.SetWidths(widths);

                string[] headers = { "Motivo", "Descripción", "Proveedor", "Monto", "Plazo para pagar" };
                foreach (var header in headers)
                {
                    var cell = new PdfPCell(new Phrase(header, fontTablaHeader))
                    {
                        BackgroundColor = fondoHeaderTabla,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5f
                    };
                    table.AddCell(cell);
                }

                foreach (var item in cuentasPendientes)
                {
                    table.AddCell(new PdfPCell(new Phrase(item.NombreDeuda ?? "", fontNormal)) { Padding = 5f });
                    table.AddCell(new PdfPCell(new Phrase(item.Descripcion ?? "", fontNormal)) { Padding = 5f });
                    table.AddCell(new PdfPCell(new Phrase(item.Proveedor ?? "", fontNormal)) { Padding = 5f });
                    table.AddCell(new PdfPCell(new Phrase("₡" + item.Monto.ToString("N2"), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5f });
                    table.AddCell(new PdfPCell(new Phrase(item.PlazaParaPagar.ToString("dd/MM/yyyy"), fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5f });
                }

                doc.Add(table);

                doc.Close();
                var nombreArchivo = $"Cierre_{TipoCierre}_{FechaDesde:yyyyMMdd}_al_{FechaHasta:yyyyMMdd}.pdf";
                return File(ms.ToArray(), "application/pdf", nombreArchivo);
            }
        }


    }
}
