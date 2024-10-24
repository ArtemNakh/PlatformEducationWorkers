using Microsoft.AspNetCore.Mvc;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Route("Admin")]
    [Area("Administrator")]
    public class WorkersController : Controller
    {
        [Route("Workers")]
        public IActionResult Index()
        {
            return View("~/Views/Administrator/Workers/Index.cshtml");
        }
    }
}
