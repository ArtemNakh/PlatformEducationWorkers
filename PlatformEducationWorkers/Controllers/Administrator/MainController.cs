using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Route("Admin")]
    [Area("Administrator")]
    public class MainController : Controller
    {
        [Route("Main")]
        [UserExists]
        public IActionResult Index()
        {
            return View("~/Views/Administrator/Main/Index.cshtml");
        }
    }
}
