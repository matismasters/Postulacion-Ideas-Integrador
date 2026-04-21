using System.Text;
using System.Text.RegularExpressions;
using IntegradorIdeas.Models;

namespace IntegradorIdeas.Services
{
    public class IdeaSimilarityService : IIdeaSimilarityService
    {
        private const double SimilarityThreshold = 0.25;

        private readonly HashSet<string> _stopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "a", "ante", "bajo", "cabe", "con", "contra", "de", "desde", "durante", 
            "en", "entre", "hacia", "hasta", "mediante", "para", "por", "según", "sin", 
            "so", "sobre", "tras", "versus", "vía", "el", "la", "los", "las", "un", "una", 
            "unos", "unas", "y", "e", "ni", "que", "o", "u", "pero", "aunque", "mas", 
            "sino", "porque", "pues", "si", "como", "cuando", "donde", "quien", "cual", 
            "este", "esta", "estos", "estas", "ese", "esa", "esos", "esas", "aquel", "aquella", 
            "aquellos", "aquellas", "es", "son", "fue", "fueron", "ser", "estar", "al", "del", "lo", "se", "su", "sus", "mis", "mi", "tu", "tus", "su", "sus", "sean", "de", "las", "los"
        };

        private readonly Dictionary<string, string> _synonyms = new(StringComparer.OrdinalIgnoreCase)
        {
            { "tema", "cancion" },
            { "temas", "cancion" },
            { "guardar", "crear" },
            { "ver", "mostrar" },
            { "reproduccion", "escuchar" },
            { "reproducidas", "escuchar" }
        };

        public SimilarityResult CompareIdeas(string idea1, string idea2)
        {
            if (string.IsNullOrWhiteSpace(idea1) || string.IsNullOrWhiteSpace(idea2))
                return new SimilarityResult();

            var words1 = ExtractValidWords(idea1);
            var words2 = ExtractValidWords(idea2);

            if (words1.Count == 0 || words2.Count == 0)
                return new SimilarityResult();

            var similarWords = words1.Intersect(words2).ToList();
            var unique1 = words1.Except(words2).ToList();
            var unique2 = words2.Except(words1).ToList();

            int matchCount = similarWords.Count;
            int minWordsCount = Math.Min(words1.Count, words2.Count);
            
            double similarityPercentage = (double)matchCount / minWordsCount;
            bool areSimilar = similarityPercentage >= SimilarityThreshold;

            return new SimilarityResult
            {
                AreSimilar = areSimilar,
                MatchCount = matchCount,
                SimilarityPercentage = Math.Round(similarityPercentage * 100, 2),
                SimilarWords = similarWords,
                UniqueWordsIdea1 = unique1,
                UniqueWordsIdea2 = unique2
            };
        }

        private HashSet<string> ExtractValidWords(string text)
        {
            var noAccents = RemoveDiacritics(text);
            var cleanText = Regex.Replace(noAccents, @"[^\w\s]", "");
            
            var words = cleanText
                .Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.ToLowerInvariant())
                .Where(w => !_stopWords.Contains(w))
                .Select(ApplySynonymsAndPronouns)
                .Select(GetWordRoot); 

            return new HashSet<string>(words);
        }

        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private string ApplySynonymsAndPronouns(string word)
        {
            if (word.Length > 4 && (word.EndsWith("la") || word.EndsWith("lo") || word.EndsWith("las") || word.EndsWith("los")))
            {
                string rootCheck = word.Substring(0, word.Length - 2);
                if (word.EndsWith("las") || word.EndsWith("los"))
                    rootCheck = word.Substring(0, word.Length - 3);

                if (rootCheck.EndsWith("r") || rootCheck.EndsWith("d"))
                {
                    word = rootCheck;
                }
            }

            if (_synonyms.TryGetValue(word, out string? synonym))
            {
                return synonym;
            }

            return word;
        }

        private string GetWordRoot(string word)
        {
            if (word.Length <= 4) return word;

            if (word.EndsWith("es") && word.Length > 5)
                word = word.Substring(0, word.Length - 2);
            else if (word.EndsWith("s") && word.Length > 4)
                word = word.Substring(0, word.Length - 1);

            if (word.EndsWith("a") || word.EndsWith("o") || word.EndsWith("e"))
                word = word.Substring(0, word.Length - 1);

            return word;
        }
    }
}
