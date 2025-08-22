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
        Task<bool> ActualizarContrasena(int id, string nuevaContrasena);

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

                //// Encriptar contraseña antes de guardar
                //Encriptar enc = new Encriptar();
                //usuario.Contraseña = enc.Encrypt(usuario.Contraseña);

                const string sql = @"
            INSERT INTO Usuario
            (TipoC, Identificacion, Nombre, Apellido, Telefono, Rol, Tiposexo, Fechadenacimiento, Correo, Contraseña)
            VALUES (@TipoC, @Identificacion, @Nombre, @Apellido, @Telefono, @Rol, @Tiposexo, @Fechadenacimiento, @Correo, @Contraseña);
        ";

                var rows = await db.ExecuteAsync(sql, usuario);
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
    }

}
