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
            // Mostrar mensaje si viene desde TempData
            if (TempData["Success"] != null)
                ViewData["Success"] = TempData["Success"];

            if (TempData["ErrorLogin"] != null)
                ViewData["Error"] = TempData["ErrorLogin"];

            return View(model);
        }

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Sesión cerrada correctamente";
            return RedirectToAction("Logins");
        }

        [HttpPost]
        //public async Task<IActionResult> inicio(LoginsModel informacion)
        //{
        //    ErrorViewModel errorViewModel = new ErrorViewModel();

        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return RedirectToAction("Logins", "Logins");
        //        }

        //        Encriptar clave = new Encriptar();
        //        informacion.contraseña = clave.Encrypt(informacion.contraseña);

        //        var usuario = await repositorioUsuario.ValidarUsuario(informacion);

        //        if (usuario != null)
        //        {
        //            // ✅ Aquí guardas el ID del usuario en la sesión
        //            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);

        //            // Luego lo rediriges a la vista deseada
        //            return View("~/Views/Home/Menu.cshtml");
        //        }

        //        TempData["ErrorLogin"] = "Correo o contraseña incorrecta";
        //        return RedirectToAction("Logins", "Logins");
        //    }
        //    catch (Exception ex)
        //    {
        //        errorViewModel.RequestId = ex.HResult.ToString();
        //        errorViewModel.message = ex.Message;
        //        return View("Error", errorViewModel);
        //    }
        //}
        public async Task<IActionResult> Inicio(LoginsModel informacion)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorLogin"] = "Datos inválidos";
                return RedirectToAction("Logins");
            }

            try
            {
                var usuario = await repositorioUsuario.ValidarUsuario(
                    informacion.correo,
                    informacion.contraseña // se envía sin encriptar
                );

                if (usuario != null)
                {
                    // 🔹 Guardar en Session (persiste mientras dure la sesión)
                    HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                    HttpContext.Session.SetString("NombreUsuario", usuario.Nombre);
                    HttpContext.Session.SetString("RolUsuario", usuario.Rol.ToString());

                    // 🔹 Mensaje de bienvenida (solo para una redirección)
                    TempData["Success"] = $"¡Bienvenido, {usuario.Nombre}!";

                    return RedirectToAction("Index", "Home");
                }

                TempData["ErrorLogin"] = "Correo o contraseña incorrecta";
                return RedirectToAction("Logins");
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel
                {
                    RequestId = ex.HResult.ToString(),
                    message = ex.Message
                });
            }
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
