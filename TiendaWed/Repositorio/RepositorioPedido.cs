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


        public async Task<int> CrearPedido(PedidoModel pedido)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // 1️⃣ Insertar Pedido
                var sql = @"INSERT INTO Pedidos (UsuarioId, Fecha, Total)
                            VALUES (@UsuarioId, @Fecha, @Total);
                            SELECT CAST(SCOPE_IDENTITY() as int);";

                var pedidoId = await connection.ExecuteScalarAsync<int>(sql, new
                {
                    pedido.UsuarioId,
                    pedido.Fecha,
                    pedido.Total
                }, transaction);

                // 2️⃣ Insertar Detalles
                var sqlDetalle = @"INSERT INTO PedidoDetalles 
                                  (PedidoId, ProductoId, Nombre, Cantidad, PrecioUnitario, Subtotal)
                                  VALUES (@PedidoId, @ProductoId, @Nombre, @Cantidad, @PrecioUnitario, @Subtotal)";

                foreach (var detalle in pedido.Detalles)
                {
                    await connection.ExecuteAsync(sqlDetalle, new
                    {
                        PedidoId = pedidoId,
                        detalle.ProductoId,
                        detalle.Nombre,
                        detalle.Cantidad,
                        detalle.PrecioUnitario,
                        Subtotal = detalle.Subtotal
                    }, transaction);
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

        // 🔹 3️⃣ Obtener Pedido con sus Detalles
        public async Task<PedidoModel?> ObtenerPedidoConDetalles(int pedidoId)
        {
            using var connection = new SqlConnection(connectionString);

            // Pedido + detalles
            var sql = @"SELECT p.Id, p.UsuarioId, p.Fecha, p.Total,
                               d.Id, d.PedidoId, d.ProductoId, d.Nombre, d.Cantidad, d.PrecioUnitario, d.Subtotal
                        FROM Pedidos p
                        INNER JOIN PedidoDetalles d ON p.Id = d.PedidoId
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

