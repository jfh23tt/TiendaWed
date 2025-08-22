using System.ComponentModel.DataAnnotations;

namespace TiendaWed.Models
{
    public class Registrarse
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El correo es obligatorio")]

        public TipoC TipoC { get; set; }
        [Required(ErrorMessage = "El correo es obligatorio")]

        public string Identificacion { get; set; }
        [Required(ErrorMessage = "El correo es obligatorio")]

        public string Nombre { get; set; }
        [Required(ErrorMessage = "El correo es obligatorio")]

        public string Apellido { get; set; }


        public string Telefono { get; set; }
        [Required(ErrorMessage = "El correo es obligatorio")]

        public Rol Rol { get; set; }
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Correo { get; set; }
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Contraseña { get; set; } // con tilde

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        [DataType(DataType.Date)]
        [FechaNacimientoValida] // 👈 Validación personalizada
        public DateTime Fechadenacimiento { get; set; }



        [Required(ErrorMessage = "El correo es obligatorio")]
        public Tiposexo Tiposexo { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

    }

}
