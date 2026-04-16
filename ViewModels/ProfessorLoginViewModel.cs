using System.ComponentModel.DataAnnotations;

namespace IntegradorIdeas.ViewModels
{
    public class ProfessorLoginViewModel
    {
        [Required(ErrorMessage = "Contraseña incorrecta o usuario inexistente")]
        [Display(Name = "Usuario")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contraseña incorrecta o usuario inexistente")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;
    }
}
