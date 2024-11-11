using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Request;
using PlatformEducationWorkers.Request.JobTitlesRequest;

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
        public async Task<IActionResult> CreateJobTitle(PlatformEducationWorkers.Request.CreateJobTitleRequest request)
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

            ViewBag.JobTitles = _jobTitleService.GetAllRoles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"))).Result;
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

            return View("~/Views/Administrator/Workers/CreateWorker.cshtml");
        }

        [UserExists]
        [Route("JobTitles")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var enterpriseId = Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"));
            if (enterpriseId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var jobTitles = await _jobTitleService.GetAllRoles(enterpriseId);
            ViewBag.JobTitles = jobTitles;

            return View("~/Views/Administrator/JobTitles/Index.cshtml", jobTitles);
        }

        [UserExists]
        [Route("DetailJobTitle")]
        public async Task<IActionResult> DetailJobTitle(int id)
        {
            var jobTitle = await _jobTitleService.GetRole(id);
            if (jobTitle == null)
            {
                return RedirectToAction("Index");
            }
            return View("~/Views/Administrator/JobTitles/DetailJobTitle.cshtml",jobTitle);
        }

        [UserExists]
        [Route("EditJobTitle")]
        [HttpGet]
        public async Task<IActionResult> EditJobTitle(int id)
        {
            var jobTitle = await _jobTitleService.GetRole(id);
            if (jobTitle == null)
            {
                return RedirectToAction("Index");
            }

            var request = new EditJobTitleRequest
            {
                Id = jobTitle.Id,
                Name = jobTitle.Name
            };

            return View("~/Views/Administrator/JobTitles/EditJobTitle.cshtml", request);
        }

        [HttpPost]
        [UserExists]
        [Route("EditJobTitle")]
        public async Task<IActionResult> EditJobTitle(EditJobTitleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var jobTitle = new JobTitle
            {
                Id = request.Id,
                Name = request.Name
            };

            await _jobTitleService.UpdateRole(jobTitle);
            return RedirectToAction("Index");
        }




        [UserExists]
        [HttpGet]
        [Route("AddJobTitle")]
        public IActionResult AddJobTitle()
        {
            return View("~/Views/Administrator/JobTitles/AddJobTitle.cshtml");
        }

        [HttpPost]
        [UserExists]
        [Route("AddJobTitle")]
        public async Task<IActionResult> AddJobTitle(PlatformEducationWorkers.Request.JobTitlesRequest.CreateJobTitleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var enterpriseId = Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"));
            var enterprise = await _enterpriseService.GetEnterprice(enterpriseId);

            if (enterprise == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var jobTitle = new JobTitle
            {
                Name = request.Name,
                Enterprise = enterprise
            };

            await _jobTitleService.AddingRole(jobTitle);

            return RedirectToAction("Index");
        }
        [HttpPost]
        [UserExists]
        public async Task<IActionResult> DeleteJobTitle(int id)
        {
            await _jobTitleService.DeleteRole(id);
            return RedirectToAction("Index");
        }
    }


 

}
