using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SurePath.Core.Models;

namespace SurePath.Core.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;

        public HomeController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            ViewData["TradingUrl"]  = _config["DivisionUrls:Trading"]  ?? "#";
            ViewData["CommerceUrl"] = _config["DivisionUrls:Commerce"] ?? "#";
            ViewData["SecurityUrl"] = _config["DivisionUrls:Security"] ?? "#";
            return View();
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
