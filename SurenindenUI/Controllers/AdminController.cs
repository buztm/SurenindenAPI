using Microsoft.AspNetCore.Mvc;

namespace SurenindenUI.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Cars()
        {
            return View();
        }

        public IActionResult Categories()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View();
        }

        public IActionResult Rentals()
        {
            return View();
        }
    }
}
