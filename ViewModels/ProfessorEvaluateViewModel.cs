namespace IntegradorIdeas.ViewModels
{
    public class ProfessorEvaluateViewModel
    {
        public int IdeaId { get; set; }

        public string Text { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;

        public bool IsCreative { get; set; }
        public bool IsWellFormulated { get; set; }
        public bool AprovedCheckbox { get; set; }

        public string? ProfessorObservation { get; set; }
    }
}
