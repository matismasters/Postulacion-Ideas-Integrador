using IntegradorIdeas.Data;
using IntegradorIdeas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace IntegradorIdeas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ideas = await _context.Ideas
                .Include(i => i.Team)
                .OrderByDescending(i => i.PostDate) // Ordered by date
                .OrderBy(i => i.Status == IdeaStatus.Aprobada ? 1 : (i.Status == IdeaStatus.NoAprobada ? 2 : 0)) // "y aprobadas no" ordered by status
                .ToListAsync();

            return View(ideas);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
