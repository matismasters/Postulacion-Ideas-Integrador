using IntegradorIdeas.Data;
using IntegradorIdeas.Models;
using IntegradorIdeas.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IntegradorIdeas.Controllers
{
    public class TeamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeamController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate if team already exists
                var existingTeam = await _context.Teams.FirstOrDefaultAsync(t => t.Name == model.Name);
                if (existingTeam != null)
                {
                    ModelState.AddModelError("Name", "ya hay un equipo con ese nombre, ingrese otro nombre");
                    return View(model);
                }

                var team = new Team
                {
                    Name = model.Name,
                    Password = model.Password,
                    MemberCount = model.MemberCount,
                    Member1Name = model.Member1Name,
                    Member2Name = model.Member2Name
                };

                _context.Teams.Add(team);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ingresado con éxito";
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
    }
}
