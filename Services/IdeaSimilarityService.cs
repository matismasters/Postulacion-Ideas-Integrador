using System.Text;
using System.Text.RegularExpressions;
using IntegradorIdeas.Models;

namespace IntegradorIdeas.Services
{
    /// <summary>
    /// Compara ideas usando un score combinado:
    ///   - 50% Jaccard sobre palabras normalizadas (raíces + sinónimos)
    ///   - 50% Similitud de coseno sobre trigramas de caracteres
    /// Esto permite detectar paráfrasis donde casi todas las palabras clave cambian.
    /// Umbral: 0.28 (equivale a ~28% de similitud combinada)
    /// </summary>
    public class IdeaSimilarityService : IIdeaSimilarityService
    {
        private const double SimilarityThreshold = 0.28;
        private const double WordWeight = 0.50;
        private const double CharNgramWeight = 0.50;
        private const int NgramSize = 3;

        // ─── Stopwords en español ──────────────────────────────────────────────
        private readonly HashSet<string> _stopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "a","al","ante","bajo","cabe","con","contra","de","del","desde","durante",
            "en","entre","hacia","hasta","mediante","para","por","segun","sin","so",
            "sobre","tras","versus","via","el","la","los","las","un","una","unos","unas",
            "y","e","ni","que","o","u","pero","aunque","mas","sino","porque","pues",
            "si","como","cuando","donde","quien","cual","cuales","quienes",
            "este","esta","estos","estas","ese","esa","esos","esas",
            "aquel","aquella","aquellos","aquellas",
            "es","son","fue","fueron","ser","estar","ha","han","habia","habian",
            "lo","se","su","sus","mi","mis","tu","tus","nos","les","le","te","me",
            "ya","no","tambien","muy","mas","menos","solo","bien","mal","asi","tan",
            "todo","toda","todos","todas","cada","otro","otra","otros","otras",
            "puede","pueden","tiene","tienen","tiene","habia","hay","hubo",
            "unica","unico","cual","cuya","cuyo","cuyas","cuyos","con","sin",
            "the","of","and","a","in","is","it","its","for","to","that","this",
            "are","was","were","be","been","being","have","has","had","do","does","did"
        };

        // ─── Diccionario de sinónimos generales en español ────────────────────
        // Cada par mapea una forma → forma canónica (raíz semántica compartida)
        private readonly Dictionary<string, string> _synonyms = new(StringComparer.OrdinalIgnoreCase)
        {
            // Vida / existencia
            { "vivir",     "vida" }, { "vive",      "vida" }, { "vida",      "vida" },
            { "util",      "vida" }, { "existir",   "vida" }, { "existencia","vida" },
            { "longevidad","vida" }, { "duracion",  "vida" },

            // Tiempo / período
            { "anos",      "tiempo" }, { "ano",       "tiempo" }, { "years",     "tiempo" },
            { "periodo",   "tiempo" }, { "lapso",     "tiempo" }, { "plazo",     "tiempo" },
            { "ciclo",     "tiempo" }, { "lustro",    "tiempo" },

            // Colmena / enjambre
            { "colmena",   "colmena" }, { "enjambre",  "colmena" }, { "panal",     "colmena" },
            { "colmenas",  "colmena" }, { "enjambres", "colmena" },

            // Abeja / insecto
            { "abeja",     "abeja" }, { "abejas",    "abeja" }, { "insecto",   "abeja" },
            { "insectos",  "abeja" }, { "apicultura","abeja" },

            // Reina / jefa / líder
            { "reina",     "lider" }, { "jefa",      "lider" }, { "lider",     "lider" },
            { "maestra",   "lider" }, { "principal", "lider" }, { "cabeza",    "lider" },

            // Hembra / reproductora
            { "hembra",     "reproductora" }, { "fertil",     "reproductora" },
            { "fertiles",   "reproductora" }, { "reproductiva","reproductora" },
            { "reproductivo","reproductora"},  { "reproductoras","reproductora" },
            { "capacidad",  "reproductora" }, { "integrante", "reproductora" },
            { "miembro",    "reproductora" },

            // Único / solo / exclusivo
            { "unico",     "exclusivo" }, { "unica",     "exclusivo" },
            { "solo",      "exclusivo" }, { "sola",      "exclusivo" },
            { "exclusivo",  "exclusivo" }, { "exclusiva", "exclusivo" },

            // Tecnología / informática
            { "tema",      "cancion" }, { "temas",     "cancion" },
            { "guardar",   "crear"   }, { "ver",       "mostrar" },
            { "reproduccion","escuchar" }, { "reproducidas","escuchar" },
            { "cancion",   "cancion" }, { "canciones", "cancion" },
            { "musica",    "cancion" },

            // Mostrar / visualizar
            { "mostrar",   "mostrar" }, { "visualizar","mostrar" },
            { "exhibir",   "mostrar" }, { "presentar", "mostrar" },
            { "desplegar", "mostrar" }, { "listar",    "mostrar" },

            // Crear / agregar / añadir
            { "crear",     "crear" }, { "agregar",   "crear" },
            { "anadir",    "crear" }, { "registrar", "crear" },
            { "nuevo",     "crear" }, { "nueva",     "crear" },
            { "insertar",  "crear" }, { "generar",   "crear" },

            // Eliminar / borrar
            { "eliminar",  "borrar" }, { "borrar",    "borrar" },
            { "quitar",    "borrar" }, { "remover",   "borrar" },
            { "suprimir",  "borrar" }, { "descartar", "borrar" },

            // Usuario / persona
            { "usuario",   "persona" }, { "usuarios",  "persona" },
            { "persona",   "persona" }, { "personas",  "persona" },
            { "gente",     "persona" }, { "individuo", "persona" },
            { "alumno",    "persona" }, { "alumnos",   "persona" },
            { "estudiante","persona" }, { "estudiantes","persona" },

            // Sistema / aplicación / plataforma
            { "sistema",   "app" }, { "sistemas",  "app" },
            { "aplicacion","app" }, { "app",       "app" },
            { "plataforma","app" }, { "herramienta","app" },
            { "software",  "app" }, { "programa",  "app" },

            // Buscar / encontrar
            { "buscar",    "buscar" }, { "encontrar", "buscar" },
            { "busqueda",  "buscar" }, { "hallar",    "buscar" },
            { "filtrar",   "buscar" }, { "localizar", "buscar" },

            // Notificar / avisar / alertar
            { "notificar", "notificar" }, { "avisar",    "notificar" },
            { "alertar",   "notificar" }, { "informar",  "notificar" },
            { "comunicar", "notificar" },

            // Compartir / publicar
            { "compartir", "publicar" }, { "publicar",  "publicar" },
            { "difundir",  "publicar" }, { "publicacion","publicar" },
            { "postear",   "publicar" }, { "post",      "publicar" },

            // Rápido / eficiente / veloz
            { "rapido",    "rapido" }, { "rapida",    "rapido" },
            { "veloz",     "rapido" }, { "eficiente", "rapido" },
            { "agil",      "rapido" }, { "eficaz",    "rapido" },

            // Seguro / protegido
            { "seguro",    "seguro" }, { "segura",    "seguro" },
            { "protegido", "seguro" }, { "protegida", "seguro" },
            { "privado",   "seguro" }, { "privada",   "seguro" },
        };

        // ─── API pública ───────────────────────────────────────────────────────
        public SimilarityResult CompareIdeas(string idea1, string idea2)
        {
            if (string.IsNullOrWhiteSpace(idea1) || string.IsNullOrWhiteSpace(idea2))
                return new SimilarityResult();

            var words1 = ExtractValidWords(idea1);
            var words2 = ExtractValidWords(idea2);

            if (words1.Count == 0 || words2.Count == 0)
                return new SimilarityResult();

            // ── Score 1: Jaccard sobre palabras normalizadas ──
            var similarWords = words1.Intersect(words2).ToList();
            int matchCount = similarWords.Count;
            int unionCount = words1.Count + words2.Count - matchCount;
            double jaccardScore = unionCount > 0 ? (double)matchCount / unionCount : 0;

            // ── Score 2: Coseno sobre trigramas de caracteres ──
            string norm1 = NormalizeForNgram(idea1);
            string norm2 = NormalizeForNgram(idea2);
            double ngramScore = CosineSimilarityNgrams(norm1, norm2, NgramSize);

            // ── Score combinado ──
            double combined = WordWeight * jaccardScore + CharNgramWeight * ngramScore;
            bool areSimilar = combined >= SimilarityThreshold;

            return new SimilarityResult
            {
                AreSimilar = areSimilar,
                MatchCount = matchCount,
                SimilarityPercentage = Math.Round(combined * 100, 2),
                SimilarWords = similarWords,
                UniqueWordsIdea1 = words1.Except(words2).ToList(),
                UniqueWordsIdea2 = words2.Except(words1).ToList()
            };
        }

        // ─── Normalización de palabras ─────────────────────────────────────────
        private HashSet<string> ExtractValidWords(string text)
        {
            var clean = RemoveDiacritics(text);
            clean = Regex.Replace(clean, @"[^\w\s]", "");

            var words = clean
                .Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.ToLowerInvariant())
                .Select(NormalizeRepeatedCharacters)
                .Where(w => !_stopWords.Contains(w) && w.Length > 2)
                .Select(ApplySynonyms)
                .Select(GetWordRoot);

            return new HashSet<string>(words);
        }

        private string NormalizeRepeatedCharacters(string word)
            => Regex.Replace(word, @"(.)\1{2,}", "$1");

        private string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (char c in normalized)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                    != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private string ApplySynonyms(string word)
        {
            // Intentar primero la forma exacta
            if (_synonyms.TryGetValue(word, out string? syn)) return syn;

            // Intentar quitar sufijos flexivos antes de buscar en sinónimos
            foreach (var suffix in new[] { "mente", "cion", "sion", "idad", "ismo", "ista", "ando", "iendo", "ado", "ido", "ar", "er", "ir" })
            {
                if (word.Length > suffix.Length + 3 && word.EndsWith(suffix))
                {
                    string stem = word[..^suffix.Length];
                    if (_synonyms.TryGetValue(stem, out string? stemSyn)) return stemSyn;
                }
            }

            return word;
        }

        private string GetWordRoot(string word)
        {
            if (word.Length <= 4) return word;

            if (word.EndsWith("es") && word.Length > 5)
                word = word[..^2];
            else if (word.EndsWith("s") && word.Length > 4)
                word = word[..^1];

            if (word.EndsWith("a") || word.EndsWith("o") || word.EndsWith("e"))
                word = word[..^1];

            return word;
        }

        // ─── Similitud de coseno sobre n-gramas de caracteres ─────────────────
        private string NormalizeForNgram(string text)
        {
            var s = RemoveDiacritics(text.ToLowerInvariant());
            return Regex.Replace(s, @"[^a-z0-9 ]", " ");
        }

        private static Dictionary<string, int> BuildNgramFrequency(string text, int n)
        {
            var freq = new Dictionary<string, int>();
            // Operamos sobre palabras para no mezclar trigramas entre palabras
            foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (word.Length < n) { freq[word] = freq.GetValueOrDefault(word) + 1; continue; }
                for (int i = 0; i <= word.Length - n; i++)
                {
                    string ng = word.Substring(i, n);
                    freq[ng] = freq.GetValueOrDefault(ng) + 1;
                }
            }
            return freq;
        }

        private static double CosineSimilarityNgrams(string text1, string text2, int n)
        {
            var freq1 = BuildNgramFrequency(text1, n);
            var freq2 = BuildNgramFrequency(text2, n);

            double dot = 0, mag1 = 0, mag2 = 0;

            foreach (var (ng, count) in freq1)
            {
                mag1 += count * count;
                if (freq2.TryGetValue(ng, out int c2)) dot += count * c2;
            }
            foreach (var (_, count) in freq2) mag2 += count * count;

            if (mag1 == 0 || mag2 == 0) return 0;
            return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2));
        }
    }
}
