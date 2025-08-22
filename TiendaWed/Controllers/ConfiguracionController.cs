using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaWed.Models;
using TiendaWed.Repositorio;

namespace TiendaWed.Controllers
{
    public class ConfiguracionController : Controller
    {
        private readonly IRepositorioUsuario repositorioUsuario;
        public ConfiguracionController(IRepositorioUsuario repositorioUsuario)
        {
            this.repositorioUsuario = repositorioUsuario;
        }
        // GET: ConfiguracionController
        public async Task<IActionResult> InformacionPersonal()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                // Redirigir al login si no hay sesión
                TempData["ErrorLogin"] = "Debe iniciar sesión para acceder a esta sección.";
                return RedirectToAction("Login", "Logins");
            }

            var usuario = await repositorioUsuario.ObtenerPorId(usuarioId.Value);

            if (usuario == null)
            {
                return View("Error", new ErrorViewModel
                {
                    message = "Usuario no encontrado",
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            return View("~/Views/Configuracion/InformacionPersonal.cshtml", usuario);
        }

        public IActionResult Seguridad()
        {
            return View("~/Views/Configuracion/Seguridad.cshtml");
        }
        // POST: Seguridad (actualizar contraseña)
        // POST: Seguridad (actualizar contraseña)
        [HttpPost]
        public async Task<IActionResult> Seguridads(string ContrasenaActual, string NuevaContrasena, string ConfirmarContrasena)
        {
            // 1️⃣ Validar sesión de usuario
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                TempData["MensajeError"] = "Debes iniciar sesión para cambiar la contraseña.";
                return RedirectToAction("Seguridad");
            }

            // 2️⃣ Validar que las contraseñas coincidan
            if (NuevaContrasena != ConfirmarContrasena)
            {
                TempData["MensajeError"] = "Las contraseñas no coinciden.";
                return RedirectToAction("Seguridad");
            }

            // 3️⃣ Obtener el usuario desde la BD
            var usuario = await repositorioUsuario.ObtenerPorId(usuarioId.Value);
            if (usuario == null)
            {
                TempData["MensajeError"] = "Usuario no encontrado.";
                return RedirectToAction("Seguridad");
            }

            Encriptar encriptar = new Encriptar();
            usuario.Contraseña = encriptar.Decrypt(usuario.Contraseña);
            // 4️⃣ Verificar la contraseña actual (normal, sin hash)
            if (usuario.Contraseña != ContrasenaActual)
            {
                TempData["MensajeError"] = "La contraseña actual no es correcta.";
                return RedirectToAction("Seguridad");
            }

            // 5️⃣ Actualizar la nueva contraseña
            usuario.Contraseña = NuevaContrasena;
            await repositorioUsuario.ActualizarContrasena(usuario.Id, NuevaContrasena);

            // 6️⃣ Confirmación
            TempData["MensajeExito"] = "Contraseña actualizada correctamente ✅";
            return RedirectToAction("Seguridad");
        }


        public IActionResult Configuracion()
        {
            return View("~/Views/Configuracion/Configuracion.cshtml");
        }


        // GET: ConfiguracionController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ConfiguracionController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ConfiguracionController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ConfiguracionController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ConfiguracionController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ConfiguracionController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ConfiguracionController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
