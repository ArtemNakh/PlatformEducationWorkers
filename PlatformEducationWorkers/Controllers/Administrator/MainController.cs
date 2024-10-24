using Microsoft.AspNetCore.Mvc;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Route("Admin")]
    [Area("Administrator")]
    public class MainController : Controller
    {
        [Route("Main")]
        public IActionResult Index()
        {
            return View("~/Views/Administrator/Main/Index.cshtml");
            //return View();
        }
    }
}
