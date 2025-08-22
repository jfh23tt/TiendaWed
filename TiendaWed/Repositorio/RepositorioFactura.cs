using Dapper;
using System.Data.SqlClient;
using TiendaWed.Models;

namespace TiendaWed.Repositorio
{
    public interface IRepositorioFactura
    {
        Task<FacturaModel?> ObtenerFacturaConDetalles(int facturaId);
        Task<IEnumerable<FacturaModel>> ListarFacturas();
        Task<int> CrearFactura(FacturaModel factura);
    }
    public class RepositorioFactura : IRepositorioFactura
    {
        private readonly string _connectionString;

        public RepositorioFactura(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /// 🔹 Obtener una factura con todos sus detalles
        public async Task<FacturaModel?> ObtenerFacturaConDetalles(int facturaId)
        {
            using var connection = new SqlConnection(_connectionString);

            // Traer la factura
            var factura = await connection.QueryFirstOrDefaultAsync<FacturaModel>(
                "SELECT * FROM Factura WHERE Id = @Id",
                new { Id = facturaId });

            if (factura == null) return null;

            // Traer los detalles
            var detalles = await connection.QueryAsync<DetalleFacturaModel>(
                "SELECT * FROM FacturaDetalles WHERE FacturaId = @Id",
                new { Id = facturaId });

            factura.Detalles = detalles.ToList();
            return factura;
        }

        /// 🔹 Listar todas las facturas
        public async Task<IEnumerable<FacturaModel>> ListarFacturas()
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<FacturaModel>(
                "SELECT * FROM Factura ORDER BY Fecha DESC");
        }

        /// 🔹 Crear factura con sus detalles
        public async Task<int> CrearFactura(FacturaModel factura)
        {
            using var connection = new SqlConnection(_connectionString);

            // Insertar Factura
            var facturaId = await connection.ExecuteScalarAsync<int>(@"
                INSERT INTO Factura (NumeroFactura, Fecha, Total)
                OUTPUT INSERTED.Id
                VALUES (@NumeroFactura, @Fecha, @Total)",
                factura);

            // Insertar Detalles
            foreach (var d in factura.Detalles)
            {
                d.FacturaId = facturaId;
                d.Subtotal = d.Cantidad * d.PrecioUnitario;

                await connection.ExecuteAsync(@"
                    INSERT INTO FacturaDetalles (FacturaId, Nombre, Cantidad, PrecioUnitario, Subtotal)
                    VALUES (@FacturaId, @Nombre, @Cantidad, @PrecioUnitario, @Subtotal)", d);
            }

            return facturaId;
        }
    }




}
