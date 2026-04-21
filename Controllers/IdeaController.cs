using IntegradorIdeas.Data;
using IntegradorIdeas.Models;
using IntegradorIdeas.Services;
using IntegradorIdeas.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IntegradorIdeas.Controllers
{
    public class IdeaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IIdeaSimilarityService _similarityService;

        public IdeaController(ApplicationDbContext context, IIdeaSimilarityService similarityService)
        {
            _context = context;
            _similarityService = similarityService;
        }

        [HttpGet]
        public IActionResult Post()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Post(PostIdeaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var team = await _context.Teams.FirstOrDefaultAsync(t => t.Name == model.TeamName);
                if (team == null)
                {
                    ModelState.AddModelError("TeamName", "el equipo no existe, verifique el nombre");
                    return View(model);
                }

                if (team.Password != model.TeamPassword)
                {
                    ModelState.AddModelError("TeamPassword", "el password es incorrecto");
                    return View(model);
                }

                // VALIDACIÓN ANTI-DUPLICADOS CORE - Ahora permite publicar pero registra similitud
                var existingIdeas = await _context.Ideas.ToListAsync();
                Idea? mostSimilarIdea = null;
                double maxSimilarity = 0;

                foreach (var existing in existingIdeas)
                {
                    // No comparamos con ideas que ya son marcadas como similares a otra para evitar encadenamientos complejos (opcional)
                    // O simplemente comparamos con todas y buscamos la mejor coincidencia.
                    var match = _similarityService.CompareIdeas(model.Text, existing.Text);
                    if (match.AreSimilar && match.SimilarityPercentage > maxSimilarity)
                    {
                        maxSimilarity = match.SimilarityPercentage;
                        mostSimilarIdea = existing;
                    }
                }

                var idea = new Idea
                {
                    TeamId = team.Id,
                    Text = model.Text,
                    PostDate = DateTime.Now,
                    Status = IdeaStatus.Pendiente,
                    SimilarToIdeaId = mostSimilarIdea?.Id,
                    SimilarityPercentage = mostSimilarIdea != null ? maxSimilarity : (double?)null
                };

                _context.Ideas.Add(idea);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "la idea fue postulada con éxito, espere por aprobación";
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var idea = await _context.Ideas
                .Include(i => i.Team)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (idea == null)
            {
                return NotFound();
            }

            return View(idea);
        }
    }
}
