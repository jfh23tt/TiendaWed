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
        public async Task<IActionResult> GuardarProducto(ProductoModel ccc)
        {
            try
            {
                if (ccc == null)
                {
                    TempData["ErrorMessage"] = "⚠️ El modelo llegó vacío.";
                    return RedirectToAction("Producto");
                }

                if (ccc.ImageFile != null && ccc.ImageFile.Length > 0)
                {
                    var extension = Path.GetExtension(ccc.ImageFile.FileName);
                    var NuevoNombre = Guid.NewGuid().ToString() + extension;

                    var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Produimage");

                    // ✅ Crear carpeta si no existe
                    if (!Directory.Exists(carpeta))
                        Directory.CreateDirectory(carpeta);

                    var filePath = Path.Combine(carpeta, NuevoNombre);

                    // ✅ Sobrescribir en caso de existir (evita bloqueo por archivo previo)
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    // ✅ Copiar imagen y cerrar stream automáticamente
                    using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await ccc.ImageFile.CopyToAsync(stream);
                    }

                    // Guardar ruta relativa
                    ccc.Urlimagen = "/Produimage/" + NuevoNombre;

                    // 👇 Setear fecha de creación
                    ccc.FechaCreacion = DateTime.UtcNow;

                    // ✅ Insertar en BD
                    bool insertado = await repositorioProducto.InsertarProducto(ccc);

                    if (insertado)
                        TempData["SuccessMessage"] = "✅ El producto se guardó exitosamente.";
                    else
                        TempData["ErrorMessage"] = "❌ No se pudo guardar el producto.";
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
