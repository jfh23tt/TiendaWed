﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TiendaWed.Models;
using TiendaWed.Repositorio;

namespace TiendaWed.Controllers
{
    public class DatosController : Controller
    {

        private readonly IRepositorioUsuario repositorioUsuario;
        public DatosController(IRepositorioUsuario repositorioUsuario)
        {
            this.repositorioUsuario = repositorioUsuario;
        }


        // GET: DatosController
        public IActionResult Registrarse()
        {
            return View("~/Views/Registrarse/Registrarse.cshtml");
        }


        public async Task<IActionResult> Registrase(Registrarse usuario)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorRegistro"] = "Datos inválidos, revisa el formulario.";
                return RedirectToAction("Registrarse");
            }

            try
            {
                // 🔒 Asignar fecha de creación automáticamente
                usuario.FechaCreacion = DateTime.Now;

                // Encriptar la contraseña antes de guardar
                Encriptar encriptar = new Encriptar();
                usuario.Contraseña = encriptar.Encrypt(usuario.Contraseña);

                bool creado = await repositorioUsuario.RegistroUsuario(usuario);

                if (creado)
                {
                    TempData["MensajeExito"] = "Cuenta creada correctamente. Ahora puedes iniciar sesión.";
                    return RedirectToAction("Logins", "Logins");
                }
                else
                {
                    TempData["ErrorRegistro"] = "No se pudo registrar el usuario (puede que el correo ya exista).";
                    return RedirectToAction("Registrarse");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorRegistro"] = $"Ocurrió un error: {ex.Message}";
                return RedirectToAction("Registrarse");
            }
        }


        // GET: DatosController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DatosController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DatosController/Create
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

        // GET: DatosController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DatosController/Edit/5
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

        // GET: DatosController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DatosController/Delete/5
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
