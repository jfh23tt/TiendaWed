using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TiendaWed.Repositorio;
using TiendaWed.Models;

namespace TiendaWed.Controllers
{
    public class ProductoController : Controller
    {
        private readonly IRepositorioProducto repositorioProducto;

        public ProductoController(IRepositorioProducto repoProducto)
        {
            this.repositorioProducto = repoProducto;
        }
        [HttpGet]
        public IActionResult Buscar(string query)
        {
            var productos = repositorioProducto.ListarProductos();

            if (!string.IsNullOrEmpty(query))
            {
                productos = productos
                    .Where(p => p.Nombre.Contains(query, StringComparison.OrdinalIgnoreCase)
                             || p.Descripcion.Contains(query, StringComparison.OrdinalIgnoreCase)
                             || p.Categoria.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            ViewBag.Query = query;
            return View("~/Views/Home/Index.cshtml", productos);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(ProductoModel producto)
        {
            if (producto == null)
            {
                TempData["ErrorMessage"] = "⚠️ El modelo llegó vacío.";
                return RedirectToAction("Inventario");
            }

            try
            {
                // 📌 Si el usuario subió una nueva imagen
                if (producto.ImageFile != null && producto.ImageFile.Length > 0)
                {
                    var extension = Path.GetExtension(producto.ImageFile.FileName).ToLower();

                    // Extensiones permitidas
                    var permitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                    if (!permitidas.Contains(extension))
                    {
                        TempData["ErrorMessage"] = "⚠️ Solo se permiten imágenes (JPG, JPEG, PNG, GIF, WEBP).";
                        return RedirectToAction("Inventario");
                    }

                    var nuevoNombre = Guid.NewGuid().ToString() + extension;

                    var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Produimage");

                    if (!Directory.Exists(carpeta))
                        Directory.CreateDirectory(carpeta);

                    var filePath = Path.Combine(carpeta, nuevoNombre);

                    // ✅ Guardar nueva imagen
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await producto.ImageFile.CopyToAsync(stream);
                    }

                    // Guardar la nueva URL
                    producto.Urlimagen = "/Produimage/" + nuevoNombre;
                }
                else
                {
                    // 📌 Mantener la imagen anterior (sacada de BD)
                    var prodAnterior = await repositorioProducto.ObtenerProductoPorId(producto.Id);
                    if (prodAnterior != null)
                        producto.Urlimagen = prodAnterior.Urlimagen;
                }

                // ✅ Actualizar en BD
                bool actualizado = await repositorioProducto.Actualizar(producto);

                if (actualizado)
                {
                    TempData["SuccessMessage"] = "✅ Producto actualizado correctamente.";
                    return Ok();
                }
                else
                {
                    TempData["ErrorMessage"] = "❌ No se pudo actualizar el producto.";
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error: " + ex.Message;
                return StatusCode(500, ex.Message);
            }
        }


        public async Task<IActionResult> Eliminar(int id)
        {
            var producto = await repositorioProducto.ObtenerProductoPorId(id);
            if (producto == null)
            {
                return NotFound();
            }

            return View("~/Views/Inventario/Inventario.cshtml", producto);
        }

        //[HttpPost, ActionName("Eliminar")]
        //public async Task<IActionResult> ConfirmarEliminar(int id)
        //{
        //    await repositorioInventario.EliminarProducto(id);
        //    return RedirectToAction("Index");
        //}
    






        public IActionResult Inventario()
        {
            // Traer todos los productos
            var productos = repositorioProducto.ListarProductos();

            // Pasarlos a la vista
            return View("~/Views/Inventario/Inventario.cshtml", productos);
        }

        // GET: Producto/Producto
        [HttpGet]
        public IActionResult Producto()
        {
            // Traer todos los productos para mostrar en la vista
            var productos = repositorioProducto.ListarProductos();
            return View("~/Views/Inventario/Producto.cshtml", productos);
        }

        // POST: Guardar producto
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> GuardarProducto(ProductoModel ccc)
        {
            try
            {
                if (ccc == null)
                {
                    TempData["ErrorMessage"] = "⚠️ El modelo llegó vacío.";
                    return RedirectToAction("Inventario");
                }

                if (ccc.ImageFile != null && ccc.ImageFile.Length > 0)
                {
                    var extension = Path.GetExtension(ccc.ImageFile.FileName).ToLower();

                    // Extensiones permitidas
                    var permitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                    if (!permitidas.Contains(extension))
                    {
                        TempData["ErrorMessage"] = "⚠️ Solo se permiten imágenes (JPG, JPEG, PNG, GIF, WEBP).";
                        return RedirectToAction("Inventario");
                    }

                    var nuevoNombre = Guid.NewGuid().ToString() + extension;

                    var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Produimage");

                    // ✅ Crear carpeta si no existe
                    if (!Directory.Exists(carpeta))
                        Directory.CreateDirectory(carpeta);

                    var filePath = Path.Combine(carpeta, nuevoNombre);

                    // ✅ Guardar imagen
                    using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await ccc.ImageFile.CopyToAsync(stream);
                    }

                    // Guardar ruta relativa
                    ccc.Urlimagen = "/Produimage/" + nuevoNombre;

                    // Setear fecha creación
                    ccc.FechaCreacion = DateTime.Now;

                    // ✅ Insertar en BD
                    bool insertado = await repositorioProducto.InsertarProducto(ccc);

                    TempData["SuccessMessage"] = insertado
                        ? "✅ El producto se guardó exitosamente."
                        : "❌ No se pudo guardar el producto.";
                }
                else
                {
                    TempData["ErrorMessage"] = "⚠️ Debes seleccionar una imagen.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error: " + ex.Message;
            }

            return RedirectToAction("Inventario");
        }



        // GET: Detalle de producto por Id
        [HttpGet]
        public async Task<JsonResult> detalleproducto(int id)
        {
            Console.WriteLine($"➡️ ID recibido en el controlador: {id}");
            var detalle = await repositorioProducto.Detalleproducto(id);

            if (detalle == null)
            {
                return Json(new { error = $"Producto con id {id} no encontrado" });
            }

            return Json(detalle);
        }



        [HttpGet]
        public string Mensaje()
        {
            return "✅ Backend funcionando correctamente.";
        }

        // Métodos CRUD vacíos (si quieres implementar después)
        public ActionResult Edit(int id) => View();
        public ActionResult Delete(int id) => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try { return RedirectToAction(nameof(Producto)); }
            catch { return View(); }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try { return RedirectToAction(nameof(Producto)); }
            catch { return View(); }
        }
    }

}
