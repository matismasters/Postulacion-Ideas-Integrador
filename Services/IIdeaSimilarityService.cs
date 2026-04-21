using IntegradorIdeas.Models;

namespace IntegradorIdeas.Services
{
    public interface IIdeaSimilarityService
    {
        SimilarityResult CompareIdeas(string idea1, string idea2);
    }
}
