using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TiendaWed.Repositorio;

namespace TiendaWed.Controllers
{
    public class PDFController : Controller
    {
        //private readonly Irepositoriopdf repositoriopdf;
        private readonly IRepositorioPedido repositorioPedido;
        private readonly IRepositorioFactura repositorioFactura;
        private readonly IRepositorioPDF repositorioPDF;

        public PDFController(/*Irepositoriopdf repositoriopdf,*/
                             IRepositorioPedido repositorioPedido,
                             IRepositorioFactura repositorioFactura,
                             IRepositorioPDF repositorioPDF)
        {
            //this.repositoriopdf = repositoriopdf;
            this.repositorioPedido = repositorioPedido;
            this.repositorioFactura = repositorioFactura;
            this.repositorioPDF = repositorioPDF;
        }

        // ✅ Vista inicial
        public IActionResult PDF()
        {
            return View();
        }
        public async Task<IActionResult> GenerarFactura(int id)
        {
            var pedido = await repositorioPedido.ObtenerPedidoConDetalles(id);

            if (pedido == null)
            {
                TempData["MensajeError"] = "No se encontró el pedido.";
                return RedirectToAction("Compras");
            }

            using (var ms = new MemoryStream())
            {
                var writer = new iText.Kernel.Pdf.PdfWriter(ms);
                var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                var doc = new iText.Layout.Document(pdf);

                // Encabezado
                doc.Add(new iText.Layout.Element.Paragraph("Factura de Compra")
                    .SetFontSize(18).SetBold());
                doc.Add(new iText.Layout.Element.Paragraph($"Pedido N°: {pedido.Id}"));
                doc.Add(new iText.Layout.Element.Paragraph($"Cliente: {pedido.ClienteNombre}"));
                doc.Add(new iText.Layout.Element.Paragraph($"Fecha: {pedido.Fecha:dd/MM/yyyy HH:mm}"));

                doc.Add(new iText.Layout.Element.Paragraph("\n"));

                // Tabla de productos
                var table = new iText.Layout.Element.Table(4, true);
                table.AddHeaderCell("Producto");
                table.AddHeaderCell("Cantidad");
                table.AddHeaderCell("Precio Unitario");
                table.AddHeaderCell("Subtotal");

                foreach (var item in pedido.Detalles)
                {
                    table.AddCell(item.Nombre);
                    table.AddCell(item.Cantidad.ToString());
                    table.AddCell("$" + item.PrecioUnitario.ToString("N2"));
                    table.AddCell("$" + (item.Cantidad * item.PrecioUnitario).ToString("N2"));
                }

                doc.Add(table);

                doc.Add(new iText.Layout.Element.Paragraph($"\nTOTAL: ${pedido.Total:N2}")
                    .SetBold().SetFontSize(14));

                doc.Close();

                byte[] pdfBytes = ms.ToArray();

                return File(pdfBytes, "application/pdf", $"Factura_{pedido.Id}.pdf");
            }
        }


        // ✅ PDF de Inventario
        //public IActionResult pdfInventario()
        //{
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        PdfWriter writer = new PdfWriter(stream, new WriterProperties().SetCompressionLevel(9));
        //        PdfDocument pdf = new PdfDocument(writer);
        //        Document document = new Document(pdf);

        //        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        //        // Título
        //        document.Add(new Paragraph("Listado de Inventario")
        //            .SetFontSize(18)
        //            .SetFont(boldFont)
        //            .SetTextAlignment(TextAlignment.CENTER));

        //        // Tabla de inventario
        //        Table table = new Table(6);

        //        table.AddHeaderCell(new Cell().Add(new Paragraph("Nombre").SetFont(boldFont)));
        //        table.AddHeaderCell(new Cell().Add(new Paragraph("Categoría").SetFont(boldFont)));
        //        table.AddHeaderCell(new Cell().Add(new Paragraph("Cantidad").SetFont(boldFont)));
        //        table.AddHeaderCell(new Cell().Add(new Paragraph("Precio").SetFont(boldFont)));
        //        //table.AddHeaderCell(new Cell().Add(new Paragraph("Proveedor").SetFont(boldFont)));
        //        table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha Creacion").SetFont(boldFont)));

        //        var inventario = repositorioPDF.Invetariopdf(new ProductoModel());

        //        foreach (var item in inventario)
        //        {
        //            table.AddCell(item.Nombre ?? "");
        //            table.AddCell(item.Categoria ?? "");
        //            table.AddCell(item.Cantidad.ToString());
        //            table.AddCell(item.Precio.ToString("C"));
        //            //table.AddCell(item.Proveedor ?? "");
        //            table.AddCell(item.FechaCreacion.ToString("dd/MM/yyyy"));
        //        }

        //        document.Add(table);
        //        document.Close();

        //        return File(stream.ToArray(), "application/pdf", "ListadoInventario.pdf");
        //    }
        //}

        // ✅ Descargar Factura desde Pedido
        [HttpGet]
        public async Task<IActionResult> DescargarFacturaPedido(int pedidoId)
        {
            var pedido = await repositorioPedido.ObtenerPedidoConDetalles(pedidoId);

            if (pedido == null)
            {
                TempData["MensajeError"] = "No se encontró el pedido.";
                return RedirectToAction("Index", "Home");
            }

            var numeroFactura = "FAC-" + pedido.Id.ToString("D5");

            var pdfBytes = repositorioPDF.GenerarFacturaPDF(pedido, numeroFactura);

            return File(pdfBytes, "application/pdf", $"Factura-{numeroFactura}.pdf");
        }

        // ✅ Descargar Factura desde la tabla Factura
        [HttpGet]
        public async Task<IActionResult> DescargarFacturaDb(int facturaId)
        {
            var factura = await repositorioFactura.ObtenerFacturaConDetalles(facturaId);

            if (factura == null)
            {
                TempData["MensajeError"] = "No se encontró la factura.";
                return RedirectToAction("Index", "Home");
            }

            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);

            doc.Add(new Paragraph($"Factura N°: {factura.NumeroFactura}"));
            doc.Add(new Paragraph($"Fecha: {factura.Fecha}"));
            doc.Add(new Paragraph($"Total: {factura.Total:C}"));

            doc.Add(new Paragraph("Detalles:"));
            foreach (var d in factura.Detalles)
            {
                doc.Add(new Paragraph($"{d.Nombre} - {d.Cantidad} x {d.PrecioUnitario:C} = {d.Subtotal:C}"));
            }

            doc.Close();

            return File(ms.ToArray(), "application/pdf", $"Factura-{factura.NumeroFactura}.pdf");
        }
    }
}
