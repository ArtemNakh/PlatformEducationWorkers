using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;

namespace PlatformEducationWorkers.Controllers.Worker
{
    [Route("Worker")]
    [Area("Worker")]
    public class MainController : Controller
    {
        private readonly ILogger<MainController> _logger;

        public MainController(ILogger<MainController> logger)
        {
            _logger = logger;
        }

        [Route("Main")]
        [UserExists]
        public IActionResult Index()
        {
            _logger.LogInformation("User accessed the Main page of the Worker area.");

            try
            {
                return View("~/Views/Worker/Main/Index.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to render the Main page.");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
