using System.ComponentModel.DataAnnotations;

namespace TiendaWed.Models
{
    public class LoginsModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string contraseña { get; set; }
    }

}
