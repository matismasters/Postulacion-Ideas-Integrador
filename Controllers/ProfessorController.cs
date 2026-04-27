using IntegradorIdeas.Data;
using IntegradorIdeas.Models;
using IntegradorIdeas.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IntegradorIdeas.Controllers
{
    public class ProfessorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string AdminUser = "admin";
        private const string AdminPass = "admin";

        public ProfessorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(ProfessorLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Username == AdminUser && model.Password == AdminPass)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Username),
                        new Claim(ClaimTypes.Role, "Professor")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Contraseña incorrecta o usuario inexistente");
                }
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Professor")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var ideas = await _context.Ideas
                .Include(i => i.Team)
                .OrderByDescending(i => i.PostDate) // Más nueva primero
                .ToListAsync();

            return View(ideas);
        }

        [Authorize(Roles = "Professor")]
        [HttpGet]
        public async Task<IActionResult> Evaluate(int id)
        {
            var idea = await _context.Ideas.Include(i => i.Team).FirstOrDefaultAsync(i => i.Id == id);
            if (idea == null) return NotFound();

            var model = new ProfessorEvaluateViewModel
            {
                IdeaId = idea.Id,
                Text = idea.Text,
                TeamName = idea.Team?.Name ?? "Sin Equipo",
                IsCreative = idea.IsCreative,
                IsWellFormulated = idea.IsWellFormulated,
                AprovedCheckbox = idea.Status == IdeaStatus.Aprobada,
                ProfessorObservation = idea.ProfessorObservation
            };

            return View(model);
        }

        [Authorize(Roles = "Professor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Evaluate(ProfessorEvaluateViewModel model)
        {
            var idea = await _context.Ideas.FindAsync(model.IdeaId);
            if (idea == null) return NotFound();

            idea.IsCreative = model.IsCreative;
            idea.IsWellFormulated = model.IsWellFormulated;
            idea.ProfessorObservation = model.ProfessorObservation;

            if (model.IsCreative && model.IsWellFormulated && model.AprovedCheckbox)
            {
                idea.Status = IdeaStatus.Aprobada;
                TempData["SuccessMessage"] = "idea aprobada";
            }
            else
            {
                idea.Status = IdeaStatus.NoAprobada;
                TempData["ErrorMessage"] = "idea no aprobada";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home"); // The AC says "me lleva a la pagina principal"
        }
    }
}
