using Microsoft.AspNetCore.Mvc;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    public class CourcesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
