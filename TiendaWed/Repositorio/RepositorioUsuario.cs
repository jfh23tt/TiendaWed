using System.Data;
using TiendaWed.Models;
using Microsoft.Data.SqlClient;
using Dapper;

namespace TiendaWed.Repositorio
{
    public interface IRepositorioUsuario
    {
        Task<bool> RegistroUsuario(Registrarse usuario);
        Task<Registrarse> ValidarUsuario(string correo, string contrasena);
        Task<Registrarse> ObtenerPorId(int id);
        Task<IEnumerable<Registrarse>> ObtenerTodos(); // 👈 nuevo
        Task<bool> ActualizarUsuario(Registrarse usuario); // 👈 nuevo
        Task<bool> EliminarUsuario(int id); // 👈 nuevo
        Task<bool> ActualizarContrasena(int id, string nuevaContrasena);
        // 🔹 Nuevo método para descontar stock de forma segura
        

    }
    public class RepositorioUsuario : IRepositorioUsuario
    {
        private readonly string cnx;
        public RepositorioUsuario(IConfiguration configuration)
        {
            cnx = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<Registrarse> ObtenerPorId(int id)
        {
            using var connection = new SqlConnection(cnx);
            string query = "SELECT * FROM Usuario WHERE Id = @Id";
            var usuario = await connection.QueryFirstOrDefaultAsync<Registrarse>(query, new { Id = id });
            return usuario;
        }


        //public async Task<Registrarse> ValidarUsuario(LoginsModel informacion)
        //{
        //    using var connection = new SqlConnection(cnx);
        //    string query = @"SELECT * FROM Registrarse WHERE correo = @correo AND contraseña = @contraseña";
        //    var usuario = await connection.QueryFirstOrDefaultAsync<Registrarse>(query, new { informacion.correo, informacion.contraseña });
        //    return usuario;
        //}
        public async Task<Registrarse> ValidarUsuario(string correo, string contrasena)
        {
            try
            {
                using IDbConnection db = new SqlConnection(cnx);

                // Encriptar contraseña antes de comparar
                Encriptar enc = new Encriptar();
                string contrasenaEncriptada = enc.Encrypt(contrasena);

                const string sql = @"
            SELECT TOP 1 *
            FROM Usuario
            WHERE Correo = @Correo
              AND Contraseña = @Contraseña;
        ";

                return await db.QueryFirstOrDefaultAsync<Registrarse>(sql, new
                {
                    Correo = correo,
                    Contraseña = contrasenaEncriptada
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en ValidarUsuario: " + ex.Message);
                return null;
            }
        }

        // 🔹 Nuevo método: descontar stock de forma segura
      
        public async Task<bool> RegistroUsuario(Registrarse usuario)
        {
            try
            {
                using IDbConnection db = new SqlConnection(cnx);

                // Validar si el correo ya existe
                const string sqlExiste = "SELECT COUNT(*) FROM Usuario WHERE Correo = @Correo;";
                var existe = await db.ExecuteScalarAsync<int>(sqlExiste, new { usuario.Correo });

                if (existe > 0)
                {
                    throw new Exception("El correo ya está registrado.");
                }

                // 🔒 Forzar siempre rol Cliente en registros públicos
                usuario.Rol = Rol.Cliente;

                // Insert (columna Rol en SQL debe ser INT)
                const string sql = @"
            INSERT INTO Usuario (Nombre, Apellido, Telefono, Rol, Correo, Contraseña)
            VALUES (@Nombre, @Apellido, @Telefono, @Rol, @Correo, @Contraseña);
        ";

                var rows = await db.ExecuteAsync(sql, new
                {
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Telefono,
                    Rol = (int)usuario.Rol, // 👈 se guarda como INT
                    usuario.Correo,
                    usuario.Contraseña
                });

                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en RegistroUsuario: " + ex.Message);
                return false;
            }
        }


        // 👇 Nuevo método para actualizar contraseña
        public async Task<bool> ActualizarContrasena(int id, string nuevaContrasena)
        {
            try
            {
                using IDbConnection db = new SqlConnection(cnx);

                // Encriptar antes de guardar
                Encriptar enc = new Encriptar();
                string contrasenaEncriptada = enc.Encrypt(nuevaContrasena);

                const string sql = @"
                    UPDATE Usuario
                    SET Contraseña = @Contrasena
                    WHERE Id = @Id;
                ";

                var rows = await db.ExecuteAsync(sql, new
                {
                    Id = id,
                    Contrasena = contrasenaEncriptada
                });

                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en ActualizarContrasena: " + ex.Message);
                return false;
            }
        }
        public async Task<IEnumerable<Registrarse>> ObtenerTodos()
        {
            using IDbConnection db = new SqlConnection(cnx);
            const string sql = "SELECT * FROM Usuario ORDER BY FechaCreacion DESC;";
            return await db.QueryAsync<Registrarse>(sql);
        }

        public async Task<bool> ActualizarUsuario(Registrarse usuario)
        {
            try
            {
                using IDbConnection db = new SqlConnection(cnx);

                const string sql = @"
            UPDATE Usuario
            SET Nombre = @Nombre,
                Apellido = @Apellido,
                Telefono = @Telefono,
                Rol = @Rol,
                Correo = @Correo
            WHERE Id = @Id;
        ";

                var rows = await db.ExecuteAsync(sql, new
                {
                    usuario.Id,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Telefono,
                    Rol = (int)usuario.Rol,
                    usuario.Correo
                });

                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en ActualizarUsuario: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> EliminarUsuario(int id)
        {
            try
            {
                using IDbConnection db = new SqlConnection(cnx);
                const string sql = "DELETE FROM Usuario WHERE Id = @Id;";
                var rows = await db.ExecuteAsync(sql, new { Id = id });
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en EliminarUsuario: " + ex.Message);
                return false;
            }
        }

    }

}
