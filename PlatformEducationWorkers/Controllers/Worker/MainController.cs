using Microsoft.AspNetCore.Mvc;

namespace PlatformEducationWorkers.Controllers.Worker
{
    //однаковий шлях
   
    public class MainController : Controller
    {
        [Route("/Main")]
        public IActionResult Index()
        {
           return View("~/Views/Worker/Main/Index.cshtml");
            //return View();
        }
    }
}
