using Microsoft.AspNetCore.Mvc;

namespace PlatformEducationWorkers.Controllers.Worker
{
    [Route("Worker")]
    [Area("Worker")]
    public class CourcesController : Controller
    {
        // Цей метод буде доступний за маршрутом Worker/Cources
        [Route("Cources")]
        public IActionResult Index()
        {
            return View("~/Views/Worker/Cources/Index.cshtml");
        }

        // Цей метод буде доступний за маршрутом Worker/Cources/StatisticCources
        [Route("Statistics")]
        public IActionResult StatisticCources()
        {
            return View("~/Views/Worker/Cources/Statistics.cshtml");
        }
    }
}
