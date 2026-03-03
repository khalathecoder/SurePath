using Microsoft.AspNetCore.Mvc;
using SurePath.Security.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace SurePath.Security.Controllers
{
    public class HomeController : Controller
    {
        // ── GET / ──────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Index()
        {
            return View(new ContactViewModel());
        }

        // ── POST /home/contact ─────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            // TODO: wire up email / notification (Phase B)
            ViewData["ContactSuccess"] = true;
            return View("Index", new ContactViewModel());
        }

        // ── GET /portfolio ─────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Portfolio()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

// ── View model ────────────────────────────────────────────────────────────────
public class ContactViewModel
{
    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Service { get; set; } = string.Empty;

    [Required, MinLength(10), MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
}
