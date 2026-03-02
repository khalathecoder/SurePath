using Microsoft.AspNetCore.Mvc;

namespace SurePath.Markets.Controllers
{
    public class TradeController : Controller
    {
        public IActionResult Markets()
        {
            return View();
        }

        public IActionResult Journal()
        {
            return View();
        }

        public IActionResult RiskCalc()
        {
            return View();
        }
    }
}