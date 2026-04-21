using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntegradorIdeas.Models
{
    public class Idea
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TeamId { get; set; }

        [Required(ErrorMessage = "El texto de la idea no puede estar vacio.")]
        [Display(Name = "Idea Creativa")]
        public string Text { get; set; } = string.Empty;

        [Required]
        public DateTime PostDate { get; set; }

        public IdeaStatus Status { get; set; } = IdeaStatus.Pendiente;

        // Validacion/Calificacion del profesor
        public bool IsCreative { get; set; }
        public bool IsWellFormulated { get; set; }
        
        public string? ProfessorObservation { get; set; }
        
        // Similitud detectada
        public int? SimilarToIdeaId { get; set; }
        public double? SimilarityPercentage { get; set; }

        // Navegation property
        [ForeignKey("TeamId")]
        public Team? Team { get; set; }

        [ForeignKey("SimilarToIdeaId")]
        public Idea? SimilarToIdea { get; set; }
    }
}
