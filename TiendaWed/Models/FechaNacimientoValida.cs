using System.ComponentModel.DataAnnotations;

namespace TiendaWed.Models
{
    public class FechaNacimientoValida : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime fechaNacimiento)
            {
                // ❌ No permitir fechas futuras
                if (fechaNacimiento > DateTime.Today)
                {
                    return new ValidationResult("La fecha de nacimiento no puede ser futura.");
                }

                // ✅ Calcular edad
                int edad = DateTime.Today.Year - fechaNacimiento.Year;
                if (fechaNacimiento.Date > DateTime.Today.AddYears(-edad))
                    edad--;

                if (edad < 18)
                {
                    return new ValidationResult("Debe tener al menos 18 años.");
                }
            }

            return ValidationResult.Success;
        }
    }

}
