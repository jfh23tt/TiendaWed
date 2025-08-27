using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using TiendaWed.Models;
using TiendaWed.Repositorio;


public class CarritoController : Controller
{
    private readonly IRepositorioProducto repositorioProducto;
    private readonly ICarritoServicio carritoServicio;
    private readonly IRepositorioPedido repositorioPedido;

    public CarritoController(
        ICarritoServicio carritoServicio,
        IRepositorioProducto repositorioProducto,
        IRepositorioPedido repositorioPedido)
    {
        this.repositorioProducto = repositorioProducto;
        this.carritoServicio = carritoServicio;
        this.repositorioPedido = repositorioPedido;
    }

    // Listar productos en el carrito
    public IActionResult Carrito()
    {
        var items = carritoServicio.listarItemsCarro();
        return View("~/Views/Home/Carrito.cshtml", items);
    }
    // Acción para ver el detalle de una compra
    public async Task<IActionResult> DetalleCompra(int id)
    {
        if (id <= 0)
        {
            TempData["MensajeError"] = "Compra no válida.";
            return RedirectToAction("Compras"); // 👈 vuelve al historial
        }

        // Buscar el pedido con sus detalles
        var pedido = await repositorioPedido.ObtenerPedidoConDetalles(id);

        if (pedido == null)
        {
            TempData["MensajeError"] = "No se encontró la compra.";
            return RedirectToAction("Compras");
        }

        return View("~/Views/Compra/DetalleCompra.cshtml", pedido); // 👈 Renderiza DetalleCompra.cshtml
    }


    // Vista de compras
    // Vista de compras (historial de pedidos)
    public async Task<IActionResult> Compras()
    {
        var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

        if (usuarioId == null || usuarioId == 0)
        {
            TempData["MensajeError"] = "Debe iniciar sesión para ver sus compras.";
            return RedirectToAction("Login", "Logins");
        }

        // Traemos todas las compras del usuario
        var compras = await repositorioPedido.ObtenerPedidosPorUsuario(usuarioId.Value);

        if (compras == null || !compras.Any())
        {
            ViewBag.Mensaje = "Aún no tienes compras registradas.";
            return View("~/Views/Compra/Compras.cshtml", new List<PedidoModel>());
        }

        return View("~/Views/Compra/Compras.cshtml", compras);
    }


    // Buscar un producto por ID
    public async Task<IActionResult> ObtenerProductoPorCodigo(int id)
    {
        var producto = await repositorioProducto.ObtenerProductoPorId(id);
        if (producto == null)
        {
            return NotFound();
        }
        return Json(producto);
    }

    // Agregar un producto al carrito
    [HttpPost]
    public IActionResult Agregar(int codigo, int cantidad)
    {
        var detalle = repositorioProducto.selectcarro(codigo);
        if (detalle != null)
        {
            carritoServicio.agregarItemCarro(detalle, cantidad);
            var totalItems = carritoServicio.listarItemsCarro().Sum(x => x.Cantidad);

            HttpContext.Session.SetInt32("CarritoCantidad", totalItems);

            return Json(new { success = true, mensaje = "Producto agregado al carrito", totalItems });
        }
        return Json(new { success = false, mensaje = "El producto no existe" });
    }

    // Actualizar cantidad de un producto
    public IActionResult Actualizar(int productoId, int cantidad)
    {
        if (cantidad < 1)
        {
            return BadRequest("La cantidad debe ser al menos de 1");
        }

        carritoServicio.actualizarItemsCarro(productoId, cantidad);
        var carroitem = carritoServicio.listarItemsCarro();
        return View("~/Views/Home/Carrito.cshtml", carroitem);
    }

    // Eliminar producto del carrito
    public IActionResult Eliminar(int productoId)
    {
        carritoServicio.eliminarItemCarro(productoId);
        var carroitem = carritoServicio.listarItemsCarro();
        return View("~/Views/Home/Carrito.cshtml", carroitem);
    }

    // Limpiar todo el carrito
    public IActionResult LimpiarCarro()
    {
        carritoServicio.LimpiarCarro();
        TempData["MensajeExito"] = "El carrito se vació correctamente.";
        return RedirectToAction("Carrito");
    }

    // Realizar compra del carrito
    [HttpPost]
    public async Task<IActionResult> RealizarCompra()
    {
        var itemsCarrito = carritoServicio.listarItemsCarro();

        if (!itemsCarrito.Any())
        {
            TempData["MensajeError"] = "El carrito está vacío, no se puede realizar la compra.";
            return RedirectToAction("Carrito");
        }

        var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
        if (usuarioId == 0)
        {
            TempData["MensajeError"] = "Debe iniciar sesión para realizar una compra.";
            return RedirectToAction("Logins", "Logins");
        }

        var clienteNombre = HttpContext.Session.GetString("NombreUsuario") ?? "Cliente";

        var pedido = new PedidoModel
        {
            UsuarioId = usuarioId,
            ClienteNombre = clienteNombre,
            Fecha = DateTime.Now,
            Total = itemsCarrito.Sum(x => x.Producto.Precio * x.Cantidad),
            Detalles = itemsCarrito.Select(x => new PedidoDetalle
            {
                ProductoId = x.Producto.Id,
                Nombre = x.Producto.Nombre,
                Cantidad = x.Cantidad,
                PrecioUnitario = x.Producto.Precio
            }).ToList()
        };

        try
        {
            // Guardar pedido en BD
            int pedidoId = await repositorioPedido.CrearPedido(pedido);

            // Vaciar carrito
            carritoServicio.LimpiarCarro();

            TempData["MensajeExito"] = "Compra realizada con éxito.";

            // 👇 Redirige al detalle de la compra en lugar de descargar PDF
            return RedirectToAction("DetalleCompra", "Carrito", new { id = pedidoId });
        }
        catch (Exception ex)
        {
            TempData["MensajeError"] = ex.Message;
            return RedirectToAction("Carrito");
        }
    }

    public async Task<IActionResult> GenerarFactura(int id)
    {
        var pedido = await repositorioPedido.ObtenerPedidoPorId(id);

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





}
