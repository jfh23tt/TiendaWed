using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TiendaWed.Models;
using TiendaWed.Repositorio;

namespace TiendaWed.Controllers
{
    public class AdminController : Controller
    {
        private readonly IRepositorioUsuario repositorioUsuario;

        public AdminController(IRepositorioUsuario repositorioUsuario)
        {
            this.repositorioUsuario = repositorioUsuario;
        }

        // 📌 Vista principal de gestión de usuarios
        public async Task<IActionResult> Usuario()
        {
            var usuarios = await repositorioUsuario.ObtenerTodos();
            return View("~/Views/Registrarse/Usuario.cshtml", usuarios);
        }

        // 📌 Crear usuario (Admin puede asignar rol)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuario(Registrarse usuario)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Datos inválidos.";
                return RedirectToAction(nameof(Usuario));
            }

            usuario.FechaCreacion = DateTime.Now;

            if (usuario.Rol == Rol.None)
                usuario.Rol = Rol.Cliente;

            var creado = await repositorioUsuario.RegistroUsuario(usuario);
            TempData[creado ? "Success" : "Error"] = creado
                ? "Usuario creado correctamente."
                : "No se pudo crear el usuario.";

            return RedirectToAction(nameof(Usuario));
        }

        // 📌 Editar usuario (desde modal)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(Registrarse usuario)
        {
            ModelState.Remove("Contraseña");
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Datos inválidos.";
                return RedirectToAction(nameof(Usuario));
            }

            // 🔹 Recuperamos el usuario original de la BD
            var usuarioDb = await repositorioUsuario.ObtenerPorId(usuario.Id);
            if (usuarioDb == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Usuario));
            }

            // 🔹 Solo actualizamos lo editable desde el modal
            usuarioDb.Nombre = usuario.Nombre;
            usuarioDb.Apellido = usuario.Apellido;
            usuarioDb.Correo = usuario.Correo;
            usuarioDb.Telefono = usuario.Telefono;
            usuarioDb.Rol = usuario.Rol;

            // La contraseña y otros campos no enviados quedan iguales

            var actualizado = await repositorioUsuario.ActualizarUsuario(usuarioDb);

            TempData[actualizado ? "Success" : "Error"] = actualizado
                ? "Usuario actualizado correctamente ✅"
                : "No se pudo actualizar ❌";

            return RedirectToAction(nameof(Usuario));
        }


        // 📌 Eliminar usuario (desde modal)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var eliminado = await repositorioUsuario.EliminarUsuario(id);
            TempData[eliminado ? "Success" : "Error"] = eliminado
                ? "Usuario eliminado correctamente."
                : "No se pudo eliminar.";

            return RedirectToAction(nameof(Usuario));
        }
    }
}
