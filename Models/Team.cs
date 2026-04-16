using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntegradorIdeas.Models
{
    public class Team
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del equipo es obligatorio.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = string.Empty; // In a production app, use PasswordHash instead

        [Required(ErrorMessage = "Debe especificar la cantidad de integrantes.")]
        [Range(1, 2, ErrorMessage = "El numero de integrantes no esta permitido")]
        public int MemberCount { get; set; }

        [Required(ErrorMessage = "El nombre del primer integrante es obligatorio.")]
        public string Member1Name { get; set; } = string.Empty;

        public string? Member2Name { get; set; }

        // Navegation property
        public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
    }
}
