using Microsoft.AspNetCore.Mvc;

namespace PlatformEducationWorkers.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
