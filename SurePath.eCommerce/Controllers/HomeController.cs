using Microsoft.AspNetCore.Mvc;
using SurePath.eCommerce.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace SurePath.eCommerce.Controllers
{
    public class InquiryViewModel
    {
        [Required(ErrorMessage = "Company name is required.")]
        public string CompanyName { get; set; } = "";

        [Required(ErrorMessage = "Contact name is required.")]
        public string ContactName { get; set; } = "";

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please select an inquiry type.")]
        public string InquiryType { get; set; } = "";

        [Required(ErrorMessage = "Message is required.")]
        [MinLength(10, ErrorMessage = "Please provide a bit more detail.")]
        public string Message { get; set; } = "";
    }

    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new InquiryViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Index(InquiryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            ViewData["InquirySuccess"] = true;
            return View(new InquiryViewModel());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
