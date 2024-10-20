using Microsoft.AspNetCore.Mvc;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    public class MainController : Controller
    {
        [Route("Admin/Main")]
        public IActionResult Index()
        {
            return View("~/Views/Administrator/Main/Index.cshtml");
            //return View();
        }
    }
}
