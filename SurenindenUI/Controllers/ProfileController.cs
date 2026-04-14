using Microsoft.AspNetCore.Mvc;

namespace SurenindenUI.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
