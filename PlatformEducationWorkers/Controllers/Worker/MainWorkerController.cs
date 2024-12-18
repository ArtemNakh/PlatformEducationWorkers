using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;

namespace PlatformEducationWorkers.Controllers.Worker
{
    [Area("Worker")]
    public class MainWorkerController : Controller
    {
        private readonly ILogger<MainWorkerController> _logger;
        private readonly IEnterpriseService _enterpriseService;
        private readonly ICoursesService _courseService;


        public MainWorkerController(ILogger<MainWorkerController> logger, IEnterpriseService enterpriseService, ICoursesService courceService)
        {
            _logger = logger;
            _enterpriseService = enterpriseService;
            _courseService = courceService;
        }

        [HttpGet]
        [Route("MainWorker")]
        [Area("Worker")]
        [UserExists]
        public async Task<IActionResult> MainWorker()
        {
           // _logger.LogInformation("User accessed the Main page of the Worker area.");

            try
            {
                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                int workerId = HttpContext.Session.GetInt32("UserId").Value; 

                var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
                var unfinishedCourses = (await _courseService.GetUncompletedCoursesForUser( workerId,enterpriseId));
                // Отримуємо аватарку з сесії (якщо вона є)
                byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
                if (avatarBytes != null && avatarBytes.Length > 0)
                {
                    string base64Avatar = Convert.ToBase64String(avatarBytes);
                    ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
                }
                ViewData["CompanyName"] = companyName;
                ViewData["UnfinishedCourses"] = unfinishedCourses.OrderBy(c=>c.DateCreate);
                return View("~/Views/Worker/Main/Main.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to render the Main page.");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
