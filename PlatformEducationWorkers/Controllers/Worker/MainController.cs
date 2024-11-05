using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;

namespace PlatformEducationWorkers.Controllers.Worker
{
    [Route("Worker")]
    [Area("Worker")]
    public class MainController : Controller
    {
        [Route("Main")]
        [UserExists]
        public IActionResult Index()
        {
           return View("~/Views/Worker/Main/Index.cshtml");
        }
    }
}
