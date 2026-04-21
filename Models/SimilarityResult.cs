namespace IntegradorIdeas.Models
{
    public class SimilarityResult
    {
        public bool AreSimilar { get; set; }
        public int MatchCount { get; set; }
        public double SimilarityPercentage { get; set; }
        public List<string> SimilarWords { get; set; } = new List<string>();
        public List<string> UniqueWordsIdea1 { get; set; } = new List<string>();
        public List<string> UniqueWordsIdea2 { get; set; } = new List<string>();
    }
}
