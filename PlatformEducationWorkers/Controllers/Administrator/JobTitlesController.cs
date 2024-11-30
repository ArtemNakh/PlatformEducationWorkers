using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IEnterpriseService _enterpriseService;
        private readonly ILogger<JobTitlesController> _logger;

        public JobTitlesController(IJobTitleService jobTitleService, IEnterpriseService enterpriseService, ILogger<JobTitlesController> logger)
        {
            _jobTitleService = jobTitleService;
            _enterpriseService = enterpriseService;
            _logger = logger;
        }

        [HttpPost]
        [Route("CreateJobTitle")]
        [UserExists]
        public async Task<IActionResult> CreateJobTitle(PlatformEducationWorkers.Request.JobTitlesRequest.CreateJobTitleRequest request)
        {
            _logger.LogInformation("Start creating job title");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for creating job title");

                ViewBag.JobTitles = _jobTitleService.GetAllJobTitles(HttpContext.Session.GetInt32("EnterpriseId").Value).Result;
                ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

                return View("~/Views/Administrator/Workers/CreateWorker.cshtml");
            }

            var enterpriseId =Convert.ToInt32( HttpContext.Session.GetString("EnterpriseId"));

            if (enterpriseId == null)
            {
                _logger.LogWarning("Enterprise ID is null. Redirecting to login.");

                //переадресація на сторінку логін
                return RedirectToAction("Login", "Login");
            }

            var enterprise = await _enterpriseService.GetEnterprice(enterpriseId);
            if (enterprise == null)
            {
                _logger.LogWarning("Enterprise not found. Redirecting to login.");

                //переадресація на сторінку логін
                return RedirectToAction("Login", "Login");
            }

            var jobTitle = new JobTitle
            {
                Name = request.Name,
                Enterprise = enterprise
            };

            await _jobTitleService.AddingRole(jobTitle);
            _logger.LogInformation("Job title created successfully: {JobTitleName}", jobTitle.Name);

            ViewBag.JobTitles = await _jobTitleService.GetAllJobTitles(HttpContext.Session.GetInt32("EnterpriseId").Value);
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

            return View("~/Views/Administrator/Workers/CreateWorker.cshtml");
        }

        [UserExists]
        [Route("JobTitles")]
        [HttpGet]
        public async Task<IActionResult> JobTitles()
        {
            _logger.LogInformation("Fetching job titles");

            var enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            
            var jobTitles = await _jobTitleService.GetAllJobTitles(enterpriseId);

            var companyName = (await _enterpriseService.GetEnterprice(enterpriseId)).Title;
            ViewData["CompanyName"] = companyName;
            ViewBag.JobTitles = jobTitles;
            _logger.LogInformation("Job titles fetched successfully");

            return View("~/Views/Administrator/JobTitles/JobTitles.cshtml", jobTitles);
        }


        [UserExists]
        [Route("FindJobTitles")]
        [HttpGet]
        public async Task<ActionResult> FindJobTitles(string searchTerm)
        {
            IEnumerable<JobTitle> jobTitles = await _jobTitleService.GetAllJobTitles(HttpContext.Session.GetInt32("EnterpriseId").Value);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                jobTitles = jobTitles.Where(j => j.Name.Contains(searchTerm));
            }

            return View("~/Views/Administrator/JobTitles/JobTitles.cshtml", jobTitles.ToList());
        }


        [HttpGet]
        [UserExists]
        [Route("DetailJobTitle")]
        public async Task<IActionResult> DetailJobTitle(int id)
        {
            _logger.LogInformation("Fetching details for job title ID: {Id}", id);

            var jobTitle = await _jobTitleService.GetJobTitle(id);
            if (jobTitle == null)
            {
                _logger.LogWarning("Job title with ID {Id} not found", id);

                return RedirectToAction("Index");
            }
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprice(enterpriseId)).Title;
            ViewData["CompanyName"] = companyName;
            _logger.LogInformation("Job title details fetched successfully for ID: {Id}", id);

            return View("~/Views/Administrator/JobTitles/DetailJobTitle.cshtml",jobTitle);
        }

        [UserExists]
        [Route("EditJobTitle")]
        [HttpGet]
        public async Task<IActionResult> EditJobTitle(int id)
        {
            _logger.LogInformation("Fetching job title for editing. ID: {Id}", id);

            var jobTitle = await _jobTitleService.GetJobTitle(id);
            if (jobTitle == null)
            {
                _logger.LogWarning("Job title with ID {Id} not found for editing", id);

                return RedirectToAction("Index");
            }

            var request = new EditJobTitleRequest
            {
                Id = jobTitle.Id,
                Name = jobTitle.Name
            };
            _logger.LogInformation("Job title fetched successfully for editing. ID: {Id}", id);
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprice(enterpriseId)).Title;
            ViewData["CompanyName"] = companyName;
            return View("~/Views/Administrator/JobTitles/EditJobTitle.cshtml", request);
        }

        [HttpPost]
        [UserExists]
        [Route("EditJobTitle")]
        public async Task<IActionResult> EditJobTitle(EditJobTitleRequest request)
        {
            _logger.LogInformation("Editing job title with ID: {Id}", request.Id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for editing job title with ID: {Id}", request.Id);

                return View(request);
            }

            var jobTitle = new JobTitle
            {
                Id = request.Id,
                Name = request.Name
            };

            await _jobTitleService.UpdateJobTitle(jobTitle);
            _logger.LogInformation("Job title updated successfully with ID: {Id}", request.Id);

            return RedirectToAction("Index");
        }




        [UserExists]
        [HttpGet]
        [Route("AddJobTitle")]
        public async Task<IActionResult> AddJobTitle()
        {
            _logger.LogInformation("Open page for adding jobTitle");
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprice(enterpriseId)).Title;
            ViewData["CompanyName"] = companyName;
            return View("~/Views/Administrator/JobTitles/AddJobTitle.cshtml");
        }

        [HttpPost]
        [UserExists]
        [Route("AddJobTitle")]
        public async Task<IActionResult> AddJobTitle(PlatformEducationWorkers.Request.JobTitlesRequest.CreateJobTitleRequest request)
        {
            _logger.LogInformation("adding job title ");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for adding job title");

                return View(request);
            }

            var enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var enterprise = await _enterpriseService.GetEnterprice(enterpriseId);

            if (enterprise == null)
            {
                _logger.LogWarning("Enterprise was not found");

                return RedirectToAction("Login", "Login");
            }

            var jobTitle = new JobTitle
            {
                Name = request.Name,
                Enterprise = enterprise
            };

            await _jobTitleService.AddingRole(jobTitle);
            _logger.LogInformation("Job title was adding successfully");

            return RedirectToAction("Index");
        }
        [HttpPost]
        [UserExists]
        public async Task<IActionResult> DeleteJobTitle(int id)
        {
            _logger.LogInformation("Deleting job title with ID: {Id}", id);

            await _jobTitleService.DeleteJobTitle(id);
            _logger.LogInformation("Job title deleted successfully with ID: {Id}", id);

            return RedirectToAction("Index");
        }
    }


 

}
