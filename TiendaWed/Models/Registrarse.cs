using System.ComponentModel.DataAnnotations;

namespace TiendaWed.Models
{
    public class Registrarse
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellido { get; set; }

        public string Telefono { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        public Rol Rol { get; set; } = Rol.Cliente; // 👈 Por defecto será Cliente

         [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Contraseña { get; set; }

       

        [DataType(DataType.Date)]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

    }

}
