using Microsoft.AspNetCore.Mvc;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Route("Admin")]
    [Area("Administrator")]
    public class CourcesController : Controller
    {
        [Route("Cources")]
        public IActionResult Index()
        {
            return View("~/Views/Administrator/Cources/Index.cshtml");
        }
    }
}
