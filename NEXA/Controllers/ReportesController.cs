using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXA.Models;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;
using System.Linq;

namespace NEXA.Controllers
{
    public class ReportesController : Controller
    {
        private readonly NEXAContext _context;

        public ReportesController(NEXAContext context)
        {
            _context = context;
        }

        // Vista principal de reportes con filtros básicos
        public IActionResult Todos(string desdePedido = "", string hastaPedido = "", string desdeCita = "", string hastaCita = "", string desdeProyecto = "", string hastaProyecto = "")
        {
            ViewData["desdePedido"] = desdePedido;
            ViewData["hastaPedido"] = hastaPedido;
            ViewData["desdeCita"] = desdeCita;
            ViewData["hastaCita"] = hastaCita;
            ViewData["desdeProyecto"] = desdeProyecto;
            ViewData["hastaProyecto"] = hastaProyecto;

            var historial = _context.ReporteHistoriales
                .OrderByDescending(h => h.FechaGeneracion)
                .Take(20)
                .ToList();

            return View(historial);
        }

        // Registra en la base el historial de generación de reportes
        private void RegistrarHistorial(string tipoReporte, string filtros)
        {
            _context.ReporteHistoriales.Add(new ReporteHistorial
            {
                TipoReporte = tipoReporte,
                FiltrosAplicados = filtros,
                FechaGeneracion = DateTime.Now
            });
            _context.SaveChanges();
        }

        #region Helpers para Excel

        private void FormatearCabeceraExcel(IXLWorksheet ws, int filaCabecera, int columnas)
        {
            var headerRange = ws.Range(filaCabecera, 1, filaCabecera, columnas);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        #endregion

        #region Helpers para PDF

        private void AplicarEstiloCeldaHeader(PdfPCell cell)
        {
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 5;
            cell.BorderWidth = 1;
            cell.Phrase.Font.SetStyle(Font.BOLD);
        }

        private void AplicarEstiloCeldaNormal(PdfPCell cell)
        {
            cell.Padding = 5;
            cell.BorderWidth = 1;
        }

        private PdfPTable CrearTablaPdf(string[] headers, float[] widths = null)
        {
            var table = new PdfPTable(headers.Length)
            {
                WidthPercentage = 100
            };
            if (widths != null && widths.Length == headers.Length)
                table.SetWidths(widths);

            foreach (var h in headers)
            {
                var cell = new PdfPCell(new Phrase(h));
                AplicarEstiloCeldaHeader(cell);
                table.AddCell(cell);
            }
            return table;
        }

        #endregion

        #region Inventario

        public IActionResult ExcelInventario()
        {
            RegistrarHistorial("Excel Inventario", "Sin filtros");
            var datos = _context.Inventario.ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Inventario");

            // ACA PUSE EL TITULO NEXA EN GRANDE
            var rangoTitulo = ws.Range(1, 1, 1, 5);
            rangoTitulo.Merge();
            rangoTitulo.Value = "NEXA";
            rangoTitulo.Style.Font.Bold = true;
            rangoTitulo.Style.Font.FontSize = 28;
            rangoTitulo.Style.Font.FontColor = XLColor.White;
            rangoTitulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rangoTitulo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            rangoTitulo.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 70, 127); // Azul oscuro
            rangoTitulo.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
            rangoTitulo.Style.Border.OutsideBorderColor = XLColor.Black;
            ws.Row(1).Height = 40;

            // Cabecera con estilo
            ws.Cell(2, 1).Value = "Nombre";
            ws.Cell(2, 2).Value = "Descripción";
            ws.Cell(2, 3).Value = "Precio";
            ws.Cell(2, 4).Value = "Stock";
            ws.Cell(2, 5).Value = "Tipo";

            var rangoCabecera = ws.Range(2, 1, 2, 5);
            rangoCabecera.Style.Font.Bold = true;
            rangoCabecera.Style.Font.FontColor = XLColor.White;
            rangoCabecera.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            rangoCabecera.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rangoCabecera.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rangoCabecera.Style.Border.OutsideBorderColor = XLColor.Black;

            int row = 3;
            foreach (var item in datos)
            {
                ws.Cell(row, 1).Value = item.Nombre;
                ws.Cell(row, 2).Value = item.Descripcion;
                ws.Cell(row, 3).Value = item.Precio;
                ws.Cell(row, 4).Value = item.Stock;
                ws.Cell(row, 5).Value = item.tipo;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Inventario.xlsx");
        }

        public IActionResult PdfInventario()
        {
            RegistrarHistorial("PDF Inventario", "Sin filtros");
            var datos = _context.Inventario.ToList();

            using var stream = new MemoryStream();
            var doc = new Document(PageSize.A4, 25, 25, 30, 30);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            // Título NEXA grande con fondo azul y texto blanco
            var tablaTitulo = new PdfPTable(1)
            {
                WidthPercentage = 100,
                SpacingAfter = 15
            };
            var celdaTitulo = new PdfPCell(new Phrase("NEXA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 36, BaseColor.WHITE)))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                BackgroundColor = new BaseColor(0, 70, 127),
                Border = PdfPCell.NO_BORDER,
                FixedHeight = 50
            };
            tablaTitulo.AddCell(celdaTitulo);
            doc.Add(tablaTitulo);

            // Subtítulo con color gris oscuro
            var titulo = new Paragraph("Reporte de Inventario")
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15,
                Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY)
            };
            doc.Add(titulo);

            // Fecha en cursiva y gris
            var fecha = new Paragraph($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm}")
            {
                Alignment = Element.ALIGN_RIGHT,
                SpacingAfter = 15,
                Font = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10, BaseColor.GRAY)
            };
            doc.Add(fecha);

            var table = CrearTablaPdf(new[] { "Nombre", "Descripción", "Precio", "Stock", "Tipo" });

            //Estilo en la cabecera 
            for (int i = 0; i < 5; i++)
            {
                var cell = table.GetRow(0).GetCells()[i];
                cell.BackgroundColor = new BaseColor(0, 70, 127);
                cell.Phrase.Font.Color = BaseColor.WHITE;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Padding = 5f;
                cell.BorderWidth = 1.5f;
            }

            foreach (var item in datos)
            {
                var cells = new[]
                {
            new PdfPCell(new Phrase(item.Nombre)),
            new PdfPCell(new Phrase(item.Descripcion)),
            new PdfPCell(new Phrase(item.Precio.ToString("C2"))),
            new PdfPCell(new Phrase(item.Stock.ToString())),
            new PdfPCell(new Phrase(item.tipo))
        };
                foreach (var cell in cells) AplicarEstiloCeldaNormal(cell);
                foreach (var cell in cells) table.AddCell(cell);
            }

            doc.Add(table);
            doc.Close();

            return File(stream.ToArray(), "application/pdf", "Inventario.pdf");
        }

        #endregion

        #region Usuarios Empleados

        public IActionResult ExcelUsuarios()
        {
            RegistrarHistorial("Excel Usuarios Empleados", "Rol = empleado");
            var datos = _context.Usuario.Where(u => u.Rol.ToLower() == "empleado").ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Usuarios Empleados");

            // Título "NEXA" en grande con estilo
            var rangoTitulo = ws.Range(1, 1, 1, 5);
            rangoTitulo.Merge();
            rangoTitulo.Value = "NEXA";
            rangoTitulo.Style.Font.Bold = true;
            rangoTitulo.Style.Font.FontSize = 28;
            rangoTitulo.Style.Font.FontColor = XLColor.White;
            rangoTitulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rangoTitulo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            rangoTitulo.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 70, 127); // Azul oscuro
            rangoTitulo.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
            rangoTitulo.Style.Border.OutsideBorderColor = XLColor.Black;
            ws.Row(1).Height = 40;

            // Cabecera con estilo
            ws.Cell(2, 1).Value = "Nombre de Usuario";
            ws.Cell(2, 2).Value = "Nombre Completo";
            ws.Cell(2, 3).Value = "Correo";
            ws.Cell(2, 4).Value = "Cédula";
            ws.Cell(2, 5).Value = "Teléfono";

            var rangoCabecera = ws.Range(2, 1, 2, 5);
            rangoCabecera.Style.Font.Bold = true;
            rangoCabecera.Style.Font.FontColor = XLColor.White;
            rangoCabecera.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            rangoCabecera.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rangoCabecera.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rangoCabecera.Style.Border.OutsideBorderColor = XLColor.Black;

            int row = 3;
            foreach (var u in datos)
            {
                ws.Cell(row, 1).Value = u.NombreUsuario;
                ws.Cell(row, 2).Value = u.NombreCompleto;
                ws.Cell(row, 3).Value = u.Correo;
                ws.Cell(row, 4).Value = u.Cedula;
                ws.Cell(row, 5).Value = u.Telefono;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "UsuariosEmpleados.xlsx");
        }

        public IActionResult PdfUsuarios()
        {
            RegistrarHistorial("PDF Usuarios Empleados", "Rol = empleado");
            var datos = _context.Usuario.Where(u => u.Rol.ToLower() == "empleado").ToList();

            using var stream = new MemoryStream();
            var doc = new Document(PageSize.A4, 25, 25, 30, 30);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            // Título NEXA grande con fondo azul y texto blanco
            var tablaTitulo = new PdfPTable(1)
            {
                WidthPercentage = 100,
                SpacingAfter = 15
            };
            var celdaTitulo = new PdfPCell(new Phrase("NEXA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 36, BaseColor.WHITE)))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                BackgroundColor = new BaseColor(0, 70, 127),
                Border = PdfPCell.NO_BORDER,
                FixedHeight = 50
            };
            tablaTitulo.AddCell(celdaTitulo);
            doc.Add(tablaTitulo);

            // Subtítulo con color gris oscuro
            var titulo = new Paragraph("Reporte de Usuarios Empleados")
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15,
                Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY)
            };
            doc.Add(titulo);

            // Fecha en cursiva y gris
            var fecha = new Paragraph($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm}")
            {
                Alignment = Element.ALIGN_RIGHT,
                SpacingAfter = 15,
                Font = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10, BaseColor.GRAY)
            };
            doc.Add(fecha);

            var table = CrearTablaPdf(new[] { "Nombre de Usuario", "Nombre Completo", "Correo", "Cédula", "Teléfono" });

            // Aplicar estilo cabecera tabla (fondo azul oscuro, texto blanco)
            for (int i = 0; i < 5; i++)
            {
                var cell = table.GetRow(0).GetCells()[i];
                cell.BackgroundColor = new BaseColor(0, 70, 127);
                cell.Phrase.Font.Color = BaseColor.WHITE;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Padding = 5f;
                cell.BorderWidth = 1.5f;
            }

            foreach (var u in datos)
            {
                var cells = new[]
                {
            new PdfPCell(new Phrase(u.NombreUsuario)),
            new PdfPCell(new Phrase(u.NombreCompleto)),
            new PdfPCell(new Phrase(u.Correo)),
            new PdfPCell(new Phrase(u.Cedula)),
            new PdfPCell(new Phrase(u.Telefono))
        };
                foreach (var cell in cells) AplicarEstiloCeldaNormal(cell);
                foreach (var cell in cells) table.AddCell(cell);
            }

            doc.Add(table);
            doc.Close();

            return File(stream.ToArray(), "application/pdf", "UsuariosEmpleados.pdf");
        }

        #endregion

        #region Pedidos

        public IActionResult ExcelPedidos(DateTime? desde, DateTime? hasta)
        {
            RegistrarHistorial("Excel Pedidos", $"Desde: {desde?.ToString("yyyy-MM-dd") ?? "N/A"}, Hasta: {hasta?.ToString("yyyy-MM-dd") ?? "N/A"}");

            var datos = _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Inventario)
                .AsQueryable();

            if (desde.HasValue)
                datos = datos.Where(p => p.FechaPedido >= desde.Value);
            if (hasta.HasValue)
                datos = datos.Where(p => p.FechaPedido <= hasta.Value);

            var lista = datos.ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Pedidos");

            // Título "NEXA" en grande con estilo
            var rangoTitulo = ws.Range(1, 1, 1, 7);
            rangoTitulo.Merge();
            rangoTitulo.Value = "NEXA";
            rangoTitulo.Style.Font.Bold = true;
            rangoTitulo.Style.Font.FontSize = 28;
            rangoTitulo.Style.Font.FontColor = XLColor.White;
            rangoTitulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rangoTitulo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            rangoTitulo.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 70, 127); // Azul oscuro
            rangoTitulo.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
            rangoTitulo.Style.Border.OutsideBorderColor = XLColor.Black;
            ws.Row(1).Height = 40;

            // Cabecera con estilo
            ws.Cell(2, 1).Value = "ID Pedido";
            ws.Cell(2, 2).Value = "Usuario";
            ws.Cell(2, 3).Value = "Fecha Pedido";
            ws.Cell(2, 4).Value = "Estado";
            ws.Cell(2, 5).Value = "Producto";
            ws.Cell(2, 6).Value = "Cantidad";
            ws.Cell(2, 7).Value = "Precio Unitario";

            var rangoCabecera = ws.Range(2, 1, 2, 7);
            rangoCabecera.Style.Font.Bold = true;
            rangoCabecera.Style.Font.FontColor = XLColor.White;
            rangoCabecera.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            rangoCabecera.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rangoCabecera.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rangoCabecera.Style.Border.OutsideBorderColor = XLColor.Black;

            int row = 3;
            foreach (var p in lista)
            {
                if (p.Detalles != null && p.Detalles.Any())
                {
                    foreach (var d in p.Detalles)
                    {
                        ws.Cell(row, 1).Value = p.Id;
                        ws.Cell(row, 2).Value = p.Usuario?.NombreCompleto ?? "N/A";
                        ws.Cell(row, 3).Value = p.FechaPedido.ToString("yyyy-MM-dd");
                        ws.Cell(row, 4).Value = p.Estado;
                        ws.Cell(row, 5).Value = d.Inventario?.Nombre ?? "N/A";
                        ws.Cell(row, 6).Value = d.Cantidad;
                        ws.Cell(row, 7).Value = d.PrecioUnitario;
                        row++;
                    }
                }
                else
                {
                    ws.Cell(row, 1).Value = p.Id;
                    ws.Cell(row, 2).Value = p.Usuario?.NombreCompleto ?? "N/A";
                    ws.Cell(row, 3).Value = p.FechaPedido.ToString("yyyy-MM-dd");
                    ws.Cell(row, 4).Value = p.Estado;
                    ws.Cell(row, 5).Value = "-";
                    ws.Cell(row, 6).Value = "-";
                    ws.Cell(row, 7).Value = "-";
                    row++;
                }
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Pedidos.xlsx");
        }

        public IActionResult PdfPedidos(DateTime? desde, DateTime? hasta)
        {
            RegistrarHistorial("PDF Pedidos", $"Desde: {desde?.ToString("yyyy-MM-dd") ?? "N/A"}, Hasta: {hasta?.ToString("yyyy-MM-dd") ?? "N/A"}");

            var datos = _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Inventario)
                .AsQueryable();

            if (desde.HasValue)
                datos = datos.Where(p => p.FechaPedido >= desde.Value);
            if (hasta.HasValue)
                datos = datos.Where(p => p.FechaPedido <= hasta.Value);

            var lista = datos.ToList();

            using var stream = new MemoryStream();
            var doc = new Document(PageSize.A4.Rotate(), 25, 25, 30, 30);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            // Título NEXA grande con texto blanco y fondo azul (tabla con 1 celda)
            var tablaTitulo = new PdfPTable(1)
            {
                WidthPercentage = 100,
                SpacingAfter = 15
            };
            var celdaTitulo = new PdfPCell(new Phrase("NEXA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 36, BaseColor.WHITE)))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                BackgroundColor = new BaseColor(0, 70, 127),
                Border = PdfPCell.NO_BORDER,
                FixedHeight = 50
            };
            tablaTitulo.AddCell(celdaTitulo);
            doc.Add(tablaTitulo);

            // Subtítulo con color gris oscuro
            var titulo = new Paragraph("Reporte de Pedidos con Detalles")
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15,
                Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY)
            };
            doc.Add(titulo);

            // Fecha en cursiva y gris
            var fecha = new Paragraph($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm}")
            {
                Alignment = Element.ALIGN_RIGHT,
                SpacingAfter = 15,
                Font = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10, BaseColor.GRAY)
            };
            doc.Add(fecha);

            var table = CrearTablaPdf(
                new[] { "ID Pedido", "Usuario", "Fecha Pedido", "Estado", "Producto", "Cantidad", "Precio Unitario" },
                new float[] { 7f, 20f, 12f, 12f, 25f, 8f, 12f });

            // Aplicar estilo cabecera tabla (fondo azul oscuro, texto blanco)
            for (int i = 0; i < 7; i++)
            {
                var cell = table.GetRow(0).GetCells()[i];
                cell.BackgroundColor = new BaseColor(0, 70, 127);
                cell.Phrase.Font.Color = BaseColor.WHITE;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Padding = 5f;
                cell.BorderWidth = 1.5f;
            }

            foreach (var p in lista)
            {
                if (p.Detalles != null && p.Detalles.Any())
                {
                    foreach (var d in p.Detalles)
                    {
                        var cells = new[]
                        {
                    new PdfPCell(new Phrase(p.Id.ToString())),
                    new PdfPCell(new Phrase(p.Usuario?.NombreCompleto ?? "N/A")),
                    new PdfPCell(new Phrase(p.FechaPedido.ToString("yyyy-MM-dd"))),
                    new PdfPCell(new Phrase(p.Estado)),
                    new PdfPCell(new Phrase(d.Inventario?.Nombre ?? "N/A")),
                    new PdfPCell(new Phrase(d.Cantidad.ToString())),
                    new PdfPCell(new Phrase(d.PrecioUnitario.ToString("C2")))
                };
                        foreach (var cell in cells) AplicarEstiloCeldaNormal(cell);
                        foreach (var cell in cells) table.AddCell(cell);
                    }
                }
                else
                {
                    var cells = new[]
                    {
                new PdfPCell(new Phrase(p.Id.ToString())),
                new PdfPCell(new Phrase(p.Usuario?.NombreCompleto ?? "N/A")),
                new PdfPCell(new Phrase(p.FechaPedido.ToString("yyyy-MM-dd"))),
                new PdfPCell(new Phrase(p.Estado)),
                new PdfPCell(new Phrase("-")),
                new PdfPCell(new Phrase("-")),
                new PdfPCell(new Phrase("-"))
            };
                    foreach (var cell in cells) AplicarEstiloCeldaNormal(cell);
                    foreach (var cell in cells) table.AddCell(cell);
                }
            }

            doc.Add(table);
            doc.Close();

            return File(stream.ToArray(), "application/pdf", "Pedidos.pdf");
        }

        #endregion

        #region Citas

        public IActionResult ExcelCitas(DateTime? desde, DateTime? hasta)
        {
            RegistrarHistorial("Excel Citas", $"Desde: {desde?.ToString("yyyy-MM-dd") ?? "N/A"}, Hasta: {hasta?.ToString("yyyy-MM-dd") ?? "N/A"}");

            var datos = _context.Citas.Include(c => c.Usuario).AsQueryable();

            if (desde.HasValue)
                datos = datos.Where(c => c.FechaCita >= desde.Value);
            if (hasta.HasValue)
                datos = datos.Where(c => c.FechaCita <= hasta.Value);

            var lista = datos.ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Citas");

            // Título "NEXA" en grande con estilo
            var rangoTitulo = ws.Range(1, 1, 1, 5);
            rangoTitulo.Merge();
            rangoTitulo.Value = "NEXA";
            rangoTitulo.Style.Font.Bold = true;
            rangoTitulo.Style.Font.FontSize = 28;
            rangoTitulo.Style.Font.FontColor = XLColor.White;
            rangoTitulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rangoTitulo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            rangoTitulo.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 70, 127); // Azul oscuro
            rangoTitulo.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
            rangoTitulo.Style.Border.OutsideBorderColor = XLColor.Black;
            ws.Row(1).Height = 40;

            // Cabecera con estilo
            ws.Cell(2, 1).Value = "Nombre Cita";
            ws.Cell(2, 2).Value = "Tipo Cita";
            ws.Cell(2, 3).Value = "Fecha Cita";
            ws.Cell(2, 4).Value = "Encargado";
            ws.Cell(2, 5).Value = "Estado";

            var rangoCabecera = ws.Range(2, 1, 2, 5);
            rangoCabecera.Style.Font.Bold = true;
            rangoCabecera.Style.Font.FontColor = XLColor.White;
            rangoCabecera.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            rangoCabecera.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rangoCabecera.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rangoCabecera.Style.Border.OutsideBorderColor = XLColor.Black;

            int row = 3;
            foreach (var c in lista)
            {
                ws.Cell(row, 1).Value = c.NombreCita;
                ws.Cell(row, 2).Value = c.TipoCita;
                ws.Cell(row, 3).Value = c.FechaCita.ToString("yyyy-MM-dd");
                ws.Cell(row, 4).Value = c.Encargado;
                ws.Cell(row, 5).Value = c.Estado;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Citas.xlsx");
        }

        public IActionResult PdfCitas(DateTime? desde, DateTime? hasta)
        {
            RegistrarHistorial("PDF Citas", $"Desde: {desde?.ToString("yyyy-MM-dd") ?? "N/A"}, Hasta: {hasta?.ToString("yyyy-MM-dd") ?? "N/A"}");

            var datos = _context.Citas.Include(c => c.Usuario).AsQueryable();

            if (desde.HasValue)
                datos = datos.Where(c => c.FechaCita >= desde.Value);
            if (hasta.HasValue)
                datos = datos.Where(c => c.FechaCita <= hasta.Value);

            var lista = datos.ToList();

            using var stream = new MemoryStream();
            var doc = new Document(PageSize.A4, 25, 25, 30, 30);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            // Título NEXA grande con texto blanco y fondo azul (tabla con 1 celda)
            var tablaTitulo = new PdfPTable(1)
            {
                WidthPercentage = 100,
                SpacingAfter = 15
            };
            var celdaTitulo = new PdfPCell(new Phrase("NEXA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 36, BaseColor.WHITE)))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                BackgroundColor = new BaseColor(0, 70, 127),
                Border = PdfPCell.NO_BORDER,
                FixedHeight = 50
            };
            tablaTitulo.AddCell(celdaTitulo);
            doc.Add(tablaTitulo);

            // Subtítulo con color gris oscuro
            var titulo = new Paragraph("Reporte de Citas")
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15,
                Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY)
            };
            doc.Add(titulo);

            // Fecha en cursiva y gris
            var fecha = new Paragraph($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm}")
            {
                Alignment = Element.ALIGN_RIGHT,
                SpacingAfter = 15,
                Font = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10, BaseColor.GRAY)
            };
            doc.Add(fecha);

            var table = CrearTablaPdf(new[] { "Nombre Cita", "Tipo Cita", "Fecha Cita", "Encargado", "Estado" });

            // Aplicar estilo cabecera tabla (fondo azul oscuro, texto blanco)
            for (int i = 0; i < 5; i++)
            {
                var cell = table.GetRow(0).GetCells()[i];
                cell.BackgroundColor = new BaseColor(0, 70, 127);
                cell.Phrase.Font.Color = BaseColor.WHITE;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Padding = 5f;
                cell.BorderWidth = 1.5f;
            }

            foreach (var c in lista)
            {
                var cells = new[]
                {
            new PdfPCell(new Phrase(c.NombreCita)),
            new PdfPCell(new Phrase(c.TipoCita)),
            new PdfPCell(new Phrase(c.FechaCita.ToString("yyyy-MM-dd"))),
            new PdfPCell(new Phrase(c.Encargado)),
            new PdfPCell(new Phrase(c.Estado))
        };
                foreach (var cell in cells) AplicarEstiloCeldaNormal(cell);
                foreach (var cell in cells) table.AddCell(cell);
            }

            doc.Add(table);
            doc.Close();

            return File(stream.ToArray(), "application/pdf", "Citas.pdf");
        }

        #endregion

        #region Proyectos

        public IActionResult ExcelProyectos(DateTime? desde, DateTime? hasta)
        {
            RegistrarHistorial("Excel Proyectos", $"Desde: {desde?.ToString("yyyy-MM-dd") ?? "N/A"}, Hasta: {hasta?.ToString("yyyy-MM-dd") ?? "N/A"}");

            var datos = _context.Proyecto.Include(p => p.Usuario).AsQueryable();

            if (desde.HasValue)
                datos = datos.Where(p => p.FechaInicio >= desde.Value);
            if (hasta.HasValue)
                datos = datos.Where(p => p.FechaInicio <= hasta.Value);

            var lista = datos.ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Proyectos");

            // Título "NEXA" en grande con estilo
            var rangoTitulo = ws.Range(1, 1, 1, 6);
            rangoTitulo.Merge();
            rangoTitulo.Value = "NEXA";
            rangoTitulo.Style.Font.Bold = true;
            rangoTitulo.Style.Font.FontSize = 28;
            rangoTitulo.Style.Font.FontColor = XLColor.White;
            rangoTitulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rangoTitulo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            rangoTitulo.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 70, 127); // Azul oscuro
            rangoTitulo.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
            rangoTitulo.Style.Border.OutsideBorderColor = XLColor.Black;
            ws.Row(1).Height = 40;

            // Cabecera con estilo
            ws.Cell(2, 1).Value = "Nombre";
            ws.Cell(2, 2).Value = "Descripción";
            ws.Cell(2, 3).Value = "Fecha Inicio";
            ws.Cell(2, 4).Value = "Fecha Fin";
            ws.Cell(2, 5).Value = "Encargado";
            ws.Cell(2, 6).Value = "Estado";

            var rangoCabecera = ws.Range(2, 1, 2, 6);
            rangoCabecera.Style.Font.Bold = true;
            rangoCabecera.Style.Font.FontColor = XLColor.White;
            rangoCabecera.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            rangoCabecera.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rangoCabecera.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rangoCabecera.Style.Border.OutsideBorderColor = XLColor.Black;

            int row = 3;
            foreach (var p in lista)
            {
                ws.Cell(row, 1).Value = p.Nombre;
                ws.Cell(row, 2).Value = p.Descripcion;
                ws.Cell(row, 3).Value = p.FechaInicio.ToString("yyyy-MM-dd");
                ws.Cell(row, 4).Value = p.FechaFin.ToString("yyyy-MM-dd");
                ws.Cell(row, 5).Value = p.Usuario?.NombreCompleto ?? "N/A";
                ws.Cell(row, 6).Value = p.estado;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Proyectos.xlsx");
        }

        public IActionResult PdfProyectos(DateTime? desde, DateTime? hasta)
        {
            RegistrarHistorial("PDF Proyectos", $"Desde: {desde?.ToString("yyyy-MM-dd") ?? "N/A"}, Hasta: {hasta?.ToString("yyyy-MM-dd") ?? "N/A"}");

            var datos = _context.Proyecto.Include(p => p.Usuario).AsQueryable();

            if (desde.HasValue)
                datos = datos.Where(p => p.FechaInicio >= desde.Value);
            if (hasta.HasValue)
                datos = datos.Where(p => p.FechaInicio <= hasta.Value);

            var lista = datos.ToList();

            using var stream = new MemoryStream();
            var doc = new Document(PageSize.A4, 25, 25, 30, 30);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            // Título NEXA grande con fondo azul y texto blanco en tabla
            var tablaTitulo = new PdfPTable(1)
            {
                WidthPercentage = 100,
                SpacingAfter = 15
            };
            var celdaTitulo = new PdfPCell(new Phrase("NEXA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 36, BaseColor.WHITE)))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                BackgroundColor = new BaseColor(0, 70, 127),
                Border = PdfPCell.NO_BORDER,
                FixedHeight = 50
            };
            tablaTitulo.AddCell(celdaTitulo);
            doc.Add(tablaTitulo);

            // Subtítulo con color gris oscuro
            var titulo = new Paragraph("Reporte de Proyectos")
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15,
                Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY)
            };
            doc.Add(titulo);

            // Fecha en cursiva y gris
            var fecha = new Paragraph($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm}")
            {
                Alignment = Element.ALIGN_RIGHT,
                SpacingAfter = 15,
                Font = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10, BaseColor.GRAY)
            };
            doc.Add(fecha);

            var table = CrearTablaPdf(new[] { "Nombre", "Descripción", "Fecha Inicio", "Fecha Fin", "Encargado", "Estado" });

            // Aplicar estilo cabecera tabla (fondo azul oscuro, texto blanco)
            for (int i = 0; i < 6; i++)
            {
                var cell = table.GetRow(0).GetCells()[i];
                cell.BackgroundColor = new BaseColor(0, 70, 127);
                cell.Phrase.Font.Color = BaseColor.WHITE;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Padding = 5f;
                cell.BorderWidth = 1.5f;
            }

            foreach (var p in lista)
            {
                var cells = new[]
                {
            new PdfPCell(new Phrase(p.Nombre)),
            new PdfPCell(new Phrase(p.Descripcion)),
            new PdfPCell(new Phrase(p.FechaInicio.ToString("yyyy-MM-dd"))),
            new PdfPCell(new Phrase(p.FechaFin.ToString("yyyy-MM-dd"))),
            new PdfPCell(new Phrase(p.Usuario?.NombreCompleto ?? "N/A")),
            new PdfPCell(new Phrase(p.estado))
        };

                foreach (var cell in cells) AplicarEstiloCeldaNormal(cell);
                foreach (var cell in cells) table.AddCell(cell);
            }

            doc.Add(table);
            doc.Close();

            return File(stream.ToArray(), "application/pdf", "Proyectos.pdf");
        }

        #endregion

    }
}
