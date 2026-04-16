using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IntegradorIdeas.ViewModels
{
    public class RegisterViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "El nombre del equipo es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre de Equipo")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "los passwords no coinciden")]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe especificar la cantidad de integrantes.")]
        [Range(1, 2, ErrorMessage = "el numero de integrantes no esta permitido")]
        [Display(Name = "Número de integrantes")]
        public int MemberCount { get; set; }

        [Required(ErrorMessage = "El nombre del primer integrante es obligatorio.")]
        [Display(Name = "Nombre del integrante 1")]
        public string Member1Name { get; set; } = string.Empty;

        [Display(Name = "Nombre del integrante 2 (Opcional)")]
        public string? Member2Name { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MemberCount == 2 && string.IsNullOrWhiteSpace(Member2Name))
            {
                yield return new ValidationResult(
                    "El nombre del segundo integrante es obligatorio.", 
                    new[] { nameof(Member2Name) }
                );
            }
        }
    }
}
