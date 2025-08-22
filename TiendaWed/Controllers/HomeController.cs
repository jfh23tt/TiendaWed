using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TiendaWed.Models;
using TiendaWed.Repositorio;

namespace TiendaWed.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepositorioProducto repositorioProducto;

        public HomeController(ILogger<HomeController> logger, IRepositorioProducto repositorioProducto)
        {
            _logger = logger;
            this.repositorioProducto = repositorioProducto;
        }

        public IActionResult Index()
        {
            IEnumerable<ProductoModel> productos = repositorioProducto.ListarProductos();
            return View(productos);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
