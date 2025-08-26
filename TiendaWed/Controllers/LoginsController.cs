using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TiendaWed.Models;
using TiendaWed.Repositorio;
using System.Reflection;


namespace TiendaWed.Controllers
{
    public class LoginsController : Controller
    {
        private readonly IRepositorioUsuario repositorioUsuario;
        public LoginsController(IRepositorioUsuario repositorioUsuario)
        {
            this.repositorioUsuario = repositorioUsuario;
        }
        // GET: LoginsController
        // GET: Logins
        [HttpGet]
        public IActionResult Logins(LoginsModel model)
        {
            // ✅ Mensajes de éxito (ej: registro correcto)
            if (TempData["Success"] != null)
            {
                ViewData["Success"] = TempData["Success"];
            }

            // ❌ Mensajes de error (ej: login fallido)
            if (TempData["ErrorLogin"] != null)
            {
                ViewData["Error"] = TempData["ErrorLogin"];
            }

            return View(model);
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Inicio(LoginsModel informacion)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Error"] = "Datos inválidos. Verifique la información.";
                return View("Logins", informacion);
            }

            try
            {
                var usuario = await repositorioUsuario.ValidarUsuario(
                    informacion.correo,
                    informacion.contraseña
                );

                if (usuario != null)
                {
                    // 🔹 Guardar en sesión
                    HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                    HttpContext.Session.SetString("NombreUsuario", usuario.Nombre);

                    // Guardar el rol como string (Admin, Cliente, etc.)
                    HttpContext.Session.SetString("RolUsuario", usuario.Rol.ToString());

                    TempData["Success"] = $"¡Bienvenido, {usuario.Nombre}!";

                    return RedirectToAction("Index", "Home");
                }

                ViewData["Error"] = "Correo o contraseña incorrecta.";
                return View("Logins", informacion);
            }
            catch (Exception)
            {
                ViewData["Error"] = "Ha ocurrido un error inesperado. Intente nuevamente.";
                return View("Logins", informacion);
            }
        }


        // ✅ Cerrar sesión
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear(); // Elimina toda la sesión
            TempData["Success"] = "Has cerrado sesión correctamente.";
            return RedirectToAction("Logins", "Logins");
        }
    









        // GET: LoginsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: LoginsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LoginsController/Create
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

        // GET: LoginsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LoginsController/Edit/5
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

        // GET: LoginsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LoginsController/Delete/5
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
