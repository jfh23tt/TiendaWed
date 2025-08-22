using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        return View(pedido); // 👈 Renderiza DetalleCompra.cshtml
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
    public IActionResult Agregar(int codigo, int cantidad)
    {
        var detalle = repositorioProducto.selectcarro(codigo);
        if (detalle != null)
        {
            carritoServicio.agregarItemCarro(detalle, cantidad);
        }

        var carroitem = carritoServicio.listarItemsCarro();
        return View("~/Views/Home/Carrito.cshtml", carroitem);
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
            return RedirectToAction("Login", "Logins");
        }

        var pedido = new PedidoModel
        {
            UsuarioId = usuarioId,
            ClienteNombre = TempData["NombreUsuario"]?.ToString() ?? "Cliente",
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
            var pedidoId = await repositorioPedido.CrearPedido(pedido);

            carritoServicio.LimpiarCarro();

            TempData["MensajeExito"] = $"Compra realizada con éxito. Número de pedido: {pedidoId}";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception)
        {
            TempData["MensajeError"] = "Hubo un error al procesar la compra. Intente nuevamente.";
            return RedirectToAction("Carrito");
        }
    }

    // Comprar directamente un solo producto
    [HttpPost]
    public async Task<IActionResult> ComprarAhora(int productoId, int cantidad = 1)
    {
        var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
        if (usuarioId == 0)
        {
            TempData["MensajeError"] = "Debe iniciar sesión para comprar.";
            return RedirectToAction("Login", "Logins");
        }

        var producto = await repositorioProducto.ObtenerProductoPorId(productoId);
        if (producto == null)
        {
            TempData["MensajeError"] = "El producto no existe.";
            return RedirectToAction("Index", "Home");
        }

        var pedido = new PedidoModel
        {
            UsuarioId = usuarioId,
            ClienteNombre = TempData["NombreUsuario"]?.ToString() ?? "Cliente",
            Fecha = DateTime.Now,
            Total = producto.Precio * cantidad,
            Detalles = new List<PedidoDetalle>
            {
                new PedidoDetalle
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.Precio
                }
            }
        };

        try
        {
            var pedidoId = await repositorioPedido.CrearPedido(pedido);
            TempData["MensajeExito"] = $"Compra realizada con éxito. Pedido N° {pedidoId}";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            TempData["MensajeError"] = $"Ocurrió un error al procesar la compra: {ex.Message}";
            return RedirectToAction("Index", "Home");
        }
    }
}
