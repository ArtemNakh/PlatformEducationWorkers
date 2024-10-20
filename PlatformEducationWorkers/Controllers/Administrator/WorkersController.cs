using Microsoft.AspNetCore.Mvc;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    public class WorkersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
