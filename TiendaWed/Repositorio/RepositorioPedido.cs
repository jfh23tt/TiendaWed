using TiendaWed.Models;
using Dapper;
using System.Data;
using System.Data.SqlClient;

namespace TiendaWed.Repositorio
{
    public interface IRepositorioPedido
    {
        Task<int> CrearPedido(PedidoModel pedido); // Crear nuevo pedido
        Task<PedidoModel?> ObtenerPedidoConDetalles(int pedidoId);
        Task<IEnumerable<PedidoModel>> ObtenerPedidosPorUsuario(int usuarioId);
        Task<PedidoModel?> ObtenerPedidoPorId(int pedidoId);

    }

    public class RepositorioPedido : IRepositorioPedido
    {
        private readonly string connectionString;

        public RepositorioPedido(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<IEnumerable<PedidoModel>> ObtenerPedidosPorUsuario(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            // 1. Traer pedidos del usuario
            const string sqlPedidos = @"
        SELECT Id, UsuarioId, Fecha, Total 
        FROM Pedidos 
        WHERE UsuarioId = @UsuarioId
        ORDER BY Fecha DESC";

            var pedidos = (await connection.QueryAsync<PedidoModel>(sqlPedidos, new { UsuarioId = usuarioId }))
                          .ToList();

            if (pedidos.Count == 0)
                return pedidos;

            // 2. Traer todos los detalles de los pedidos en un solo query
            const string sqlDetalles = @"
        SELECT pd.Id, pd.PedidoId, pd.ProductoId, p.Nombre, 
               pd.Cantidad, pd.PrecioUnitario, pd.Subtotal, p.UrlImagen
        FROM PedidoDetalles pd
        INNER JOIN Producto p ON p.Id = pd.ProductoId
        WHERE pd.PedidoId IN @IdsPedidos";

            var detalles = (await connection.QueryAsync<PedidoDetalle>(
                sqlDetalles,
                new { IdsPedidos = pedidos.Select(p => p.Id).ToArray() }
            )).ToList();

            // 3. Asignar detalles a cada pedido
            foreach (var pedido in pedidos)
            {
                pedido.Detalles = detalles.Where(d => d.PedidoId == pedido.Id).ToList();
            }

            return pedidos;
        }
        public async Task<PedidoModel?> ObtenerPedidoPorId(int pedidoId)
        {
            using var connection = new SqlConnection(connectionString);

            const string sql = @"SELECT Id, UsuarioId, Fecha, Total
                         FROM Pedidos
                         WHERE Id = @PedidoId";

            return await connection.QueryFirstOrDefaultAsync<PedidoModel>(
                sql,
                new { PedidoId = pedidoId }
            );
        }


        public async Task<int> CrearPedido(PedidoModel pedido)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        // 1️⃣ Insertar en tabla Pedidos
                        string sqlPedido = @"INSERT INTO Pedidos (UsuarioId, Fecha, Total)
                                     VALUES (@UsuarioId, GETDATE(), @Total);
                                     SELECT CAST(SCOPE_IDENTITY() as int);";

                        int pedidoId = await db.QuerySingleAsync<int>(sqlPedido, new
                        {
                            UsuarioId = pedido.UsuarioId,
                            Total = pedido.Total
                        }, transaction);

                        // 2️⃣ Insertar detalle de pedido (con Nombre y Subtotal)
                        string sqlDetalle = @"INSERT INTO PedidoDetalles 
                                      (PedidoId, ProductoId, Nombre, Cantidad, PrecioUnitario, Subtotal)
                                      VALUES (@PedidoId, @ProductoId, @Nombre, @Cantidad, @PrecioUnitario, @Subtotal);";

                        // 3️⃣ Actualizar stock validando que no quede negativo
                        string sqlUpdateStock = @"UPDATE Producto 
                                          SET Unidades = Unidades - @Cantidad 
                                          WHERE Id = @Id AND Unidades >= @Cantidad;";

                        foreach (var item in pedido.Detalles)
                        {
                            // Insertar detalle con Nombre y Subtotal
                            await db.ExecuteAsync(sqlDetalle, new
                            {
                                PedidoId = pedidoId,
                                ProductoId = item.ProductoId,
                                Nombre = item.Nombre, // 👈 Aquí agregamos el nombre del producto
                                Cantidad = item.Cantidad,
                                PrecioUnitario = item.PrecioUnitario,
                                Subtotal = item.Cantidad * item.PrecioUnitario
                            }, transaction);

                            // Descontar stock y validar resultado
                            var filasAfectadas = await db.ExecuteAsync(sqlUpdateStock, new
                            {
                                Cantidad = item.Cantidad,
                                Id = item.ProductoId
                            }, transaction);

                            if (filasAfectadas == 0)
                            {
                                throw new Exception($"Stock insuficiente para el producto {item.ProductoId}");
                            }
                        }

                        transaction.Commit();
                        return pedidoId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }












        // 🔹 3️⃣ Obtener Pedido con sus Detalles
        public async Task<PedidoModel?> ObtenerPedidoConDetalles(int pedidoId)
        {
            using var connection = new SqlConnection(connectionString);

            var sql = @"SELECT p.Id, p.UsuarioId, p.Fecha, p.Total,
                   d.Id, d.PedidoId, d.ProductoId, d.Nombre, d.Cantidad, d.PrecioUnitario, d.Subtotal,
                   pr.UrlImagen
            FROM Pedidos p
            INNER JOIN PedidoDetalles d ON p.Id = d.PedidoId
            INNER JOIN Producto pr ON d.ProductoId = pr.Id   -- 👈 usa el nombre real de la tabla
            WHERE p.Id = @PedidoId";


            var pedidoDict = new Dictionary<int, PedidoModel>();

            var pedido = await connection.QueryAsync<PedidoModel, PedidoDetalle, PedidoModel>(
                sql,
                (p, d) =>
                {
                    if (!pedidoDict.TryGetValue(p.Id, out var pedidoEntry))
                    {
                        pedidoEntry = p;
                        pedidoEntry.Detalles = new List<PedidoDetalle>();
                        pedidoDict.Add(p.Id, pedidoEntry);
                    }
                    pedidoEntry.Detalles.Add(d);
                    return pedidoEntry;
                },
                new { PedidoId = pedidoId }
            );

            return pedido.FirstOrDefault();
        }

    }

}

