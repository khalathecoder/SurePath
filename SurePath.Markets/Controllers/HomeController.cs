using Microsoft.AspNetCore.Mvc;
using SurePath.Markets.Models;
using System.Diagnostics;

namespace SurePath.Markets.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Corporate homepage now lives in SurePath.Core.
            // Anyone hitting the Markets root gets redirected to the Trading Desk.
            return RedirectToAction("Analytics", "Trade");
        }

        public IActionResult Privacy()
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
