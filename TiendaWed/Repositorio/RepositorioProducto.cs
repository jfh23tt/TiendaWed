﻿using System.Data;
using Dapper;
using System.Data.SqlClient;
using TiendaWed.Models;
using System.Transactions;


namespace TiendaWed.Repositorio
{
    public interface IRepositorioProducto
    {
        Task<ProductoModel> Detalleproducto(int id);   // Obtener detalle de un producto por id
        IEnumerable<ProductoModel> ListarProductos();  // Listar todos los productos
        Task<bool> InsertarProducto(ProductoModel producto); // Insertar un producto nuevo
        /*carritoModel selectcarro(int id);*/              // Seleccionar producto para carrito
        Task<ProductoModel> ObtenerProductoPorId(int id); // Obtener producto por id
        CarritoModel selectcarro(int Codigo);
        Task<bool> DescontarStock(int productoId, int cantidad);

        Task<bool> Actualizar(ProductoModel producto);
        }
    public class RepositorioProducto : IRepositorioProducto
    {
        private readonly string cnx;

        public RepositorioProducto(IConfiguration configuration)
        {
            cnx = configuration.GetConnectionString("DefaultConnection");
        }
       

        /// <summary>
        /// Obtener un producto por su Id (Primary Key)
        /// </summary>
        public async Task<ProductoModel> ObtenerProductoPorId(int id)
        {
            using (IDbConnection db = new SqlConnection(cnx))
            {
                string query = @"SELECT Id, Codigo, Nombre, Categoria, Descripcion, Precio, Unidades, Estado, urlimagen 
                                 FROM Producto 
                                 WHERE Id = @Id";
                return await db.QuerySingleOrDefaultAsync<ProductoModel>(query, new { Id = id });
            }
        }
        public CarritoModel selectcarro(int Codigo)
        {
            using (IDbConnection db = new SqlConnection(cnx))
            {
                string sqlQuery = "SELECT * FROM Producto WHERE Codigo=@Codigo";
                CarritoModel producto = db.QuerySingleOrDefault<CarritoModel>(sqlQuery, new { Codigo  });
                return producto;
            }
        }
        public async Task<bool> DescontarStock(int productoId, int cantidad)
        {
            using (var connection = new SqlConnection(cnx))
            {
                string sql = @"
            UPDATE Producto 
            SET Unidades = Unidades - @Cantidad
            WHERE Id = @IdProducto AND Unidades >= @Cantidad";

                var filasAfectadas = await connection.ExecuteAsync(
                    sql,
                    new { IdProducto = productoId, Cantidad = cantidad }
                );

                return filasAfectadas > 0; // ✅ True si actualizó, False si no había stock suficiente
            }
        }


        /// <summary>
        /// Insertar un producto nuevo
        /// </summary>
        public async Task<bool> InsertarProducto(ProductoModel producto)
        {
            try
            {
                using (var connection = new SqlConnection(cnx))
                {
                    var result = await connection.ExecuteAsync(
                         @"INSERT INTO Producto(Codigo, Nombre, Categoria, Descripcion, Precio, Unidades, Estado, urlimagen) 
                       VALUES (@Codigo, @Nombre, @Categoria, @Descripcion, @Precio, @Unidades, @Estado, @urlimagen)", producto
                       );


                    return result > 0;
                }
            }
             catch (Exception ex)
            {
                // Aquí podrías loguear el error
                string msg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Listar todos los productos
        /// </summary>
        public IEnumerable<ProductoModel> ListarProductos()
        {
            using (IDbConnection db = new SqlConnection(cnx))
            {
                string sqlQuery = "SELECT * FROM Producto";
                return db.Query<ProductoModel>(sqlQuery).ToList();
            }
        }

        /// <summary>
        /// Obtener detalle de un producto por Id
        /// </summary>
        public async Task<ProductoModel> Detalleproducto(int id)
        {
            using (IDbConnection db = new SqlConnection(cnx))
            {
                string sqlQuery = "SELECT * FROM Producto WHERE Id = @Id";
                var result = await db.QueryFirstOrDefaultAsync<ProductoModel>(sqlQuery, new { Id = id });

                Console.WriteLine($"➡️ Query ejecutada con Id={id}, resultado {(result == null ? "null" : "ok")}");
                return result;
            }
        }
        public async Task<bool> Actualizar(ProductoModel producto)
        {
            using (var connection = new SqlConnection(cnx))
            {
                string sql = @"
        UPDATE Producto
        SET Codigo = @Codigo,
            Nombre = @Nombre,
            Categoria = @Categoria,
            Descripcion = @Descripcion,
            Precio = @Precio,
            Unidades = @Unidades,
            Estado = @Estado,
            urlimagen = @urlimagen
        WHERE Id = @Id";

                var filasAfectadas = await connection.ExecuteAsync(sql, producto);
                return filasAfectadas > 0; // 👈 devuelve true si se actualizó algo
            }
        }





        /// <summary>
        /// Seleccionar producto para carrito usando Id
        /// </summary>
        //public carritoModel selectcarro(int id)
        //{
        //    using (IDbConnection db = new SqlConnection(cnx))
        //    {
        //        string sqlQuery = @"SELECT Id, Codigo, Nombre, Marca, Descripcion, Precio, Unidades, Estado, urlimagen 
        //                            FROM Producto 
        //                            WHERE Id = @Id";
        //        return db.QuerySingleOrDefault<carritoModel>(sqlQuery, new { Id = id });
        //    }
        //}
    }

}
