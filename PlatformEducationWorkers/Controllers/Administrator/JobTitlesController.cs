using Amazon.Runtime.Internal;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Request;
using PlatformEducationWorkers.Request.JobTitlesRequest;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Area("Administrator")]

    public class JobTitlesController : Controller
    {
        private readonly IJobTitleService _jobTitleService;
        private readonly IEnterpriseService _enterpriseService;
        private readonly ILoggerService _loggerService;


        public JobTitlesController(IJobTitleService jobTitleService, IEnterpriseService enterpriseService,  ILoggerService loggerService)
        {
            _jobTitleService = jobTitleService;
            _enterpriseService = enterpriseService;
            _loggerService = loggerService;
        }

        [HttpPost]
        [Route("CreateJobTitle")]
        [UserExists]
        public async Task<IActionResult> CreateJobTitle(CreateJobTitleRequest request)
        {

            //await _loggerService.LogAsync(Logger.LogType.Info, $"Start creating job title", HttpContext.Session.GetInt32("UserId").Value);

            if (!ModelState.IsValid)
            {
              //  await _loggerService.LogAsync(Logger.LogType.Warning, $"Invalid model state for creating job title", HttpContext.Session.GetInt32("UserId").Value);



                ViewBag.JobTitles = _jobTitleService.GetAllJobTitles(HttpContext.Session.GetInt32("EnterpriseId").Value).Result;
                ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

                return View("~/Views/Administrator/Workers/CreateWorker.cshtml");
            }

            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;

            if (enterpriseId == null)
            {
                //await _loggerService.LogAsync(Logger.LogType.Warning, $"Enterprise ID is null. Redirecting to login.", HttpContext.Session.GetInt32("UserId").Value);

                return RedirectToAction("Login", "Login");
            }

            var enterprise = await _enterpriseService.GetEnterprise(enterpriseId);
            if (enterprise == null)
            {
                //await _loggerService.LogAsync(Logger.LogType.Warning, $"Enterprise not found. Redirecting to login.", HttpContext.Session.GetInt32("UserId").Value);

                return RedirectToAction("Login", "Login");
            }

            var jobTitle = new JobTitle
            {
                Name = request.Name,
                Enterprise = enterprise
            };

            await _jobTitleService.AddingRole(jobTitle);

            //  await _loggerService.LogAsync(Logger.LogType.Info, $"Job title created successfully: {jobTitle.Name}", HttpContext.Session.GetInt32("UserId").Value);
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
            ViewBag.JobTitles = await _jobTitleService.GetAllJobTitles(HttpContext.Session.GetInt32("EnterpriseId").Value);
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

            return View("~/Views/Administrator/Workers/CreateWorker.cshtml");
        }

        [UserExists]
        [Route("JobTitles")]
        [HttpGet]
        public async Task<IActionResult> JobTitles()
        {
            // await _loggerService.LogAsync(Logger.LogType.Info, $"Fetching job titles", HttpContext.Session.GetInt32("UserId").Value);

            var enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            
            var jobTitles = await _jobTitleService.GetAllJobTitles(enterpriseId);

            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
            ViewData["CompanyName"] = companyName;
            ViewBag.JobTitles = jobTitles;

           // await _loggerService.LogAsync(Logger.LogType.Info, $"Job titles fetched successfully", HttpContext.Session.GetInt32("UserId").Value);

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
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
            return View("~/Views/Administrator/JobTitles/JobTitles.cshtml", jobTitles.ToList());
        }


        [HttpGet]
        [UserExists]
        [Route("DetailJobTitle")]
        public async Task<IActionResult> DetailJobTitle(int id)
        {

           // await _loggerService.LogAsync(Logger.LogType.Info, $"Fetching details for job title ID: {id}", HttpContext.Session.GetInt32("UserId").Value);

            var jobTitle = await _jobTitleService.GetJobTitle(id);
            if (jobTitle == null)
            {
                await _loggerService.LogAsync(Logger.LogType.Warning, $"Job title with ID {id} not found", HttpContext.Session.GetInt32("UserId").Value);

                return RedirectToAction("JobTitles");
            }
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
            ViewData["CompanyName"] = companyName;

          //  await _loggerService.LogAsync(Logger.LogType.Info, $"Job title details fetched successfully for ID: {id}", HttpContext.Session.GetInt32("UserId").Value);

            return View("~/Views/Administrator/JobTitles/DetailJobTitle.cshtml",jobTitle);
        }

        [UserExists]
        [Route("EditJobTitle")]
        [HttpGet]
        public async Task<IActionResult> EditJobTitle(int id)
        {

           // await _loggerService.LogAsync(Logger.LogType.Info, $"Fetching job title for editing. ID: {id}", HttpContext.Session.GetInt32("UserId").Value);

            var jobTitle = await _jobTitleService.GetJobTitle(id);
            if (jobTitle == null)
            {

                await _loggerService.LogAsync(Logger.LogType.Warning, $"Job title with ID {id} not found for editing", HttpContext.Session.GetInt32("UserId").Value);

                return RedirectToAction("JobTitles");
            }

            var request = new EditJobTitleRequest
            {
                Id = jobTitle.Id,
                Name = jobTitle.Name
            }; 
            
           // await _loggerService.LogAsync(Logger.LogType.Info, $"Job title fetched successfully for editing. ID: {id}", HttpContext.Session.GetInt32("UserId").Value);


            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
            ViewData["CompanyName"] = companyName;
            return View("~/Views/Administrator/JobTitles/EditJobTitle.cshtml", request);
        }

        [HttpPost]
        [UserExists]
        [Route("EditJobTitle")]
        public async Task<IActionResult> EditJobTitle(EditJobTitleRequest request)
        {

          //  await _loggerService.LogAsync(Logger.LogType.Info, $"Editing job title with ID: {request.Id}", HttpContext.Session.GetInt32("UserId").Value);

            if (!ModelState.IsValid)
            {
                await _loggerService.LogAsync(Logger.LogType.Warning, $"Invalid model state for editing job title with ID: {request.Id}", HttpContext.Session.GetInt32("UserId").Value);



                return View(request);
            }

            var jobTitle = new JobTitle
            {
                Id = request.Id,
                Name = request.Name
            };

            await _jobTitleService.UpdateJobTitle(jobTitle);
            await _loggerService.LogAsync(Logger.LogType.Info, $"Job title updated successfully with ID: {request.Id}", HttpContext.Session.GetInt32("UserId").Value);



            return RedirectToAction("JobTitles");
        }   




        [UserExists]
        [HttpGet]
        [Route("AddJobTitle")]
        public async Task<IActionResult> AddJobTitle()
        {
          //  await _loggerService.LogAsync(Logger.LogType.Info, $"Open page for adding jobTitle", HttpContext.Session.GetInt32("UserId").Value);


            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
            ViewData["CompanyName"] = companyName;
            return View("~/Views/Administrator/JobTitles/AddJobTitle.cshtml");
        }

        [HttpPost]
        [UserExists]
        [Route("AddJobTitle")]
        public async Task<IActionResult> AddJobTitle(PlatformEducationWorkers.Request.JobTitlesRequest.CreateJobTitleRequest request)
        {

           // await _loggerService.LogAsync(Logger.LogType.Info, $"adding job title ", HttpContext.Session.GetInt32("UserId").Value);

            if (!ModelState.IsValid)
            {

                await _loggerService.LogAsync(Logger.LogType.Warning, $"Invalid model state for adding job title", HttpContext.Session.GetInt32("UserId").Value);

                return View(request);
            }

            var enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var enterprise = await _enterpriseService.GetEnterprise(enterpriseId);

            if (enterprise == null)
            {

                await _loggerService.LogAsync(Logger.LogType.Warning, $"Enterprise was not found", HttpContext.Session.GetInt32("UserId").Value);

                return RedirectToAction("Login", "Login");
            }

            var jobTitle = new JobTitle
            {
                Name = request.Name,
                Enterprise = enterprise
            };

            await _jobTitleService.AddingRole(jobTitle);

            await _loggerService.LogAsync(Logger.LogType.Info, $"Job title was adding successfully", HttpContext.Session.GetInt32("UserId").Value);

            return RedirectToAction("JobTitles");
        }
            
        [HttpPost]
        [UserExists]
        public async Task<IActionResult> DeleteJobTitle(int id)
        {
         //   await _loggerService.LogAsync(Logger.LogType.Info, $"Deleting job title with ID: {id}", HttpContext.Session.GetInt32("UserId").Value);

            await _jobTitleService.DeleteJobTitle(id);

            await _loggerService.LogAsync(Logger.LogType.Info, $"Job title deleted successfully with ID: {id}", HttpContext.Session.GetInt32("UserId").Value);

            return RedirectToAction("JobTitles");
        }
    }


 

}
