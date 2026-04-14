using Microsoft.AspNetCore.Mvc;

namespace SurenindenUI.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("authToken");
            return RedirectToAction("Index", "Home");
        }
    }
}
