using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Request;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Route("Workers")]
    public class JobTitlesController : Controller
    {
        private readonly IJobTitleService _jobTitleService;
        private readonly IEnterpriceService _enterpriseService; // для отримання підприємства за ID

        public JobTitlesController(IJobTitleService jobTitleService, IEnterpriceService enterpriseService)
        {
            _jobTitleService = jobTitleService;
            _enterpriseService = enterpriseService;
        }

        [HttpPost]
        [Route("CreateJobTitle")]
        [UserExists]
        public async Task<IActionResult> CreateJobTitle(CreateJobTitleRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.JobTitles = _jobTitleService.GetAllRoles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"))).Result;
                ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

                return View("~/Views/Administrator/Workers/CreateWorker.cshtml");
            }

            var enterpriseId =Convert.ToInt32( HttpContext.Session.GetString("EnterpriseId"));

            if (enterpriseId == null)
            {
                //переадресація на сторінку логін
                return RedirectToAction("Login", "Login");
            }

            var enterprise = await _enterpriseService.GetEnterprice(enterpriseId);
            if (enterprise == null)
            {
                //переадресація на сторінку логін
                return RedirectToAction("Login", "Login");
            }

            var jobTitle = new JobTitle
            {
                Name = request.Name,
                Enterprise = enterprise
            };

            await _jobTitleService.AddingRole(jobTitle);

            return View("~/Views/Administrator/Workers/CreateWorker.cshtml");
        }

        [UserExists]
        public IActionResult Index()
        {
            return View();
        }

       
    }
}
