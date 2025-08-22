using System.Data;
using Dapper;
using System.Data.SqlClient;
using TiendaWed.Models;
using System.Text.Json;

namespace TiendaWed.Repositorio
{
    public interface ICarritoServicio
    {
        void agregarItemCarro(CarritoModel producto, int cantidad);
        List<CarritoItem> listarItemsCarro();
        void eliminarItemCarro(int productoId);
        void actualizarItemsCarro(int productoId, int cantidad);
        decimal obtenerTotal();
        void RealizarCompra(int usuarioId);
        void LimpiarCarro(); // 👈 nuevo
    }

    public class carritoServicio : ICarritoServicio
    {
        private readonly string cnx;
        private readonly productoSelecionados _productoSelecionados;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly List<CarritoItem> itemsCarrito = new List<CarritoItem>();

        public carritoServicio(
            productoSelecionados productoSelecionados,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _productoSelecionados = productoSelecionados;
            _configuration = configuration;
            cnx = _configuration.GetConnectionString("DefaultConnection");
        }

        private productoSelecionados obtenerItemsSesion()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var cartJson = session.GetString("carrito");
            var jh = string.IsNullOrEmpty(cartJson)
                ? new productoSelecionados()
                : JsonSerializer.Deserialize<productoSelecionados>(cartJson);
            return jh;
        }

        private void guardarItemsSesion(productoSelecionados cart)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            session.SetString("carrito", JsonSerializer.Serialize(cart));
        }

        public void agregarItemCarro(CarritoModel Product, int cantidad)
        {
            var cart = obtenerItemsSesion();
            var existingItem = cart.Items.FirstOrDefault(i => i.Producto.Id == Product.Id);

            if (existingItem != null)
            {
                existingItem.Cantidad += cantidad;
            }
            else
            {
                cart.Items.Add(new CarritoItem { Producto = Product, Cantidad = cantidad });
            }
            guardarItemsSesion(cart);
        }

        public void eliminarItemCarro(int productoId)
        {
            var cart = obtenerItemsSesion();
            var item = cart.Items.FirstOrDefault(i => i.Producto.Id == productoId);

            if (item != null)
            {
                cart.Items.Remove(item);
                guardarItemsSesion(cart);
            }
        }

        public decimal obtenerTotal()
        {
            var cart = obtenerItemsSesion();
            return cart.Items.Sum(i => i.Producto.Precio * i.Cantidad);
        }

        public void actualizarItemsCarro(int productoId, int cantidad)
        {
            var cart = obtenerItemsSesion();
            var existeItem = cart.Items.FirstOrDefault(i => i.Producto.Id == productoId);

            if (existeItem != null)
            {
                existeItem.Cantidad = cantidad;
            }
            guardarItemsSesion(cart);
        }

        public List<CarritoItem> listarItemsCarro()
        {
            return obtenerItemsSesion().Items;
        }

        public void RealizarCompra(int usuarioId)
        {
            var cart = obtenerItemsSesion();
            if (cart.Items.Count == 0)
                throw new Exception("El carrito está vacío");

            using (IDbConnection db = new SqlConnection(cnx))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        // 1️⃣ Insertar en tabla Orden
                        string sqlOrden = @"INSERT INTO Pedido  (UsuarioId, Fecha, Total)
                                            VALUES (@UsuarioId, GETDATE(), @Total);
                                            SELECT CAST(SCOPE_IDENTITY() as int);";

                        int ordenId = db.QuerySingle<int>(sqlOrden, new
                        {
                            UsuarioId = usuarioId,
                            Total = obtenerTotal()
                        }, transaction);

                        // 2️⃣ Insertar detalle de orden
                        string sqlDetalle = @"INSERT INTO PedidoDetalle  (OrdenId, ProductoId, Cantidad, PrecioUnitario)
                                              VALUES (@OrdenId, @ProductoId, @Cantidad, @PrecioUnitario);";

                        foreach (var item in cart.Items)
                        {
                            db.Execute(sqlDetalle, new
                            {
                                OrdenId = ordenId,
                                ProductoId = item.Producto.Id,
                                Cantidad = item.Cantidad,
                                PrecioUnitario = item.Producto.Precio
                            }, transaction);

                            // 3️⃣ Descontar stock del producto
                            string sqlUpdateStock = @"UPDATE Producto 
                                                      SET Unidades = Unidades - @Cantidad 
                                                      WHERE Id = @Id";

                            db.Execute(sqlUpdateStock, new
                            {
                                Cantidad = item.Cantidad,
                                Id = item.Producto.Id
                            }, transaction);
                        }

                        transaction.Commit();

                        // 4️⃣ Vaciar carrito
                        guardarItemsSesion(new productoSelecionados());
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        //public int cantidadTotalCarrito()
        //{
        //    var cart = obtenerItemsSesion();
        //    return cart.Items.Sum(x => x.Cantidad);
        //}

        public int cantidadTotalCarrito()
        {
            return obtenerItemsSesion().Items.Sum(x => x.Cantidad);
        }


        // 🔹 Método para vaciar todo el carrito
        public void LimpiarCarro()
        {
            guardarItemsSesion(new productoSelecionados());
        }
    }
}
