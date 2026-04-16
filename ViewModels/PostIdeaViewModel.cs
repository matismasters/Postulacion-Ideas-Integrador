using System.ComponentModel.DataAnnotations;

namespace IntegradorIdeas.ViewModels
{
    public class PostIdeaViewModel
    {
        [Required(ErrorMessage = "los campos no pueden estar vacíos")]
        [Display(Name = "Nombre de Equipo")]
        public string TeamName { get; set; } = string.Empty;

        [Required(ErrorMessage = "los campos no pueden estar vacíos")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string TeamPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "los campos no pueden estar vacíos")]
        [Display(Name = "Descripción de la Idea")]
        public string Text { get; set; } = string.Empty;
    }
}
