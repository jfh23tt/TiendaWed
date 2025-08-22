using Dapper;
using TiendaWed.Models;
using System.Data.SqlClient;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace TiendaWed.Repositorio
{
    public interface IRepositorioPDF
    {
        List<ProductoModel> Invetariopdf(ProductoModel pdfinventario);
        byte[] GenerarFacturaPDF(PedidoModel pedido, string numeroFactura);
    }

    public class RepositorioPDF : IRepositorioPDF
    {
        private readonly string _cnx;

        public RepositorioPDF(IConfiguration configuration)
        {
            _cnx = configuration.GetConnectionString("DefaultConnection");
        }

        public List<ProductoModel> Invetariopdf(ProductoModel pdfinventario)
        {
            using var connection = new SqlConnection(_cnx); // ✅ Cambiado a SqlConnection
            string query = "SELECT * FROM Inventario";
            return connection.Query<ProductoModel>(query).ToList();
        }
        public byte[] GenerarFacturaPDF(PedidoModel pedido, string numeroFactura)
        {
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            doc.Add(new Paragraph($"Factura N°: {numeroFactura}")
                .SetFontSize(16)
                .SetFont(boldFont));

            doc.Add(new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy}"));
            doc.Add(new Paragraph($"Cliente: {pedido.ClienteNombre}"));
            doc.Add(new Paragraph(" "));

            Table table = new Table(4).UseAllAvailableWidth();
            table.AddHeaderCell("Producto");
            table.AddHeaderCell("Cantidad");
            table.AddHeaderCell("Precio Unitario");
            table.AddHeaderCell("Subtotal");

            decimal total = 0;
            foreach (var d in pedido.Detalles)
            {
                table.AddCell(d.Nombre);
                table.AddCell(d.Cantidad.ToString());
                table.AddCell(d.PrecioUnitario.ToString("C"));
                var subtotal = d.Cantidad * d.PrecioUnitario;
                total += subtotal;
                table.AddCell(subtotal.ToString("C"));
            }

            doc.Add(table);
            doc.Add(new Paragraph($"TOTAL: {total:C}")
                .SetFontSize(14)
                .SetFont(boldFont)
                .SetTextAlignment(TextAlignment.RIGHT));

            doc.Close();
            return ms.ToArray();
        }
    }

}
