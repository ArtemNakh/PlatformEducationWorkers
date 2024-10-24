using Microsoft.AspNetCore.Mvc;

namespace PlatformEducationWorkers.Controllers.Worker
{
    [Route("Worker")]
    [Area("Worker")]
    public class MainController : Controller
    {
        [Route("Main")]
        public IActionResult Index()
        {
           return View("~/Views/Worker/Main/Index.cshtml");
        }
    }
}
