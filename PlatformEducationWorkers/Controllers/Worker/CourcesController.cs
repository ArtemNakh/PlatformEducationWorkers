using Microsoft.AspNetCore.Mvc;

namespace PlatformEducationWorkers.Controllers.Worker
{
    public class CourcesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
