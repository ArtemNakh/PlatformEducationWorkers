using Amazon.Runtime.Internal;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Core;
using PlatformEducationWorkers.Request;
using PlatformEducationWorkers.Request.JobTitlesRequest;
using Serilog;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Area("Administrator")]

    public class JobTitlesController : Controller
    {
        private readonly IJobTitleService _jobTitleService;
        private readonly IEnterpriseService _enterpriseService;


        public JobTitlesController(IJobTitleService jobTitleService, IEnterpriseService enterpriseService)
        {
            _jobTitleService = jobTitleService;
            _enterpriseService = enterpriseService;
        }
        private void SetViewData()
        {
            ViewBag.JobTitles = _jobTitleService.GetAllJobTitles(HttpContext.Session.GetInt32("EnterpriseId").Value).Result;
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
        }



        [UserExists]
        [Route("JobTitles")]
        [HttpGet]
        public async Task<IActionResult> JobTitles()
        {

            Log.Information($"open the page job titles");
            var enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;

            var jobTitles = await _jobTitleService.GetAvaliableRoles(enterpriseId);

            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
            else
            {
                ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
            }
            ViewData["CompanyName"] = companyName;
            ViewBag.JobTitles = jobTitles;


            return View("~/Views/Administrator/JobTitles/JobTitles.cshtml", jobTitles);
        }


        [UserExists]
        [Route("FindJobTitles")]
        [HttpGet]
        public async Task<ActionResult> FindJobTitles(string searchTerm)
        {

            Log.Information($"find jobTitle for title");
            IEnumerable<JobTitle> jobTitles = await _jobTitleService.GetAvaliableRoles(HttpContext.Session.GetInt32("EnterpriseId").Value);

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
            else
            {
                ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
            }
            var enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
            ViewData["CompanyName"] = companyName;
            return View("~/Views/Administrator/JobTitles/JobTitles.cshtml", jobTitles.ToList());
        }


        [HttpGet]
        [UserExists]
        [Route("DetailJobTitle")]
        public async Task<IActionResult> DetailJobTitle(int id)
        {

            Log.Information($"open the page detailJobTitle");

            var jobTitle = await _jobTitleService.GetJobTitle(id);
            if (jobTitle == null)
            {
                Log.Error($"detail job title, jobtitle is nill,id jobtitle:{id}");

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
            else
            {
                ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
            }
            ViewData["CompanyName"] = companyName;

            return View("~/Views/Administrator/JobTitles/DetailJobTitle.cshtml", jobTitle);
        }

        [UserExists]
        [Route("EditJobTitle")]
        [HttpGet]
        public async Task<IActionResult> EditJobTitle(int id)
        {

            Log.Information($"open the page edit job title");
            var jobTitle = await _jobTitleService.GetJobTitle(id);
            if (jobTitle == null)
            {

                Log.Error($"edit job title, jobtitle is nill,id jobtitle:{id}");
                return RedirectToAction("JobTitles");
            }

            var request = new EditJobTitleRequest
            {
                Id = jobTitle.Id,
                Name = jobTitle.Name
            };


            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
            else
            {
                ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
            }
            ViewData["CompanyName"] = companyName;
            return View("~/Views/Administrator/JobTitles/EditJobTitle.cshtml", request);
        }

        [HttpPost]
        [UserExists]
        [Route("EditJobTitle")]
        public async Task<IActionResult> EditJobTitle(EditJobTitleRequest request)
        {

            Log.Information($"post edit job title ");
            if (!ModelState.IsValid)
            {

                Log.Warning($"edit job title, request is not valid,request :{request}");

                return View("~/Views/Administrator/JobTitles/EditJobTitle.cshtml", request);
            }
            var enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
            ViewData["CompanyName"] = companyName;

            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }


            // Перевірка на існування посади
            if ((await _jobTitleService.GetAllJobTitles(enterpriseId))
                .Any(n => n.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("Name", "Посада з такою назвою вже існує.");
                Log.Warning($"Create Job title. Job title with name '{request.Name}' already exists.");
                SetViewData();


                return View("~/Views/Administrator/JobTitles/EditJobTitle.cshtml", request);
            }

            var jobTitle = new JobTitle
            {
                Id = request.Id,
                Name = request.Name
            };

            await _jobTitleService.UpdateJobTitle(jobTitle);



            Log.Information($"jobTitle was succesfully update");
            return RedirectToAction("JobTitles");
        }




        [UserExists]
        [HttpGet]
        [Route("AddJobTitle")]
        public async Task<IActionResult> AddJobTitle()
        {


            Log.Information($"open the page CreateCourse");
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
            else
            {
                ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
            }
            ViewData["CompanyName"] = companyName;
            return View("~/Views/Administrator/JobTitles/AddJobTitle.cshtml");
        }

        [HttpPost]
        [UserExists]
        [Route("AddJobTitle")]
        public async Task<IActionResult> AddJobTitle(CreateJobTitleRequest request)
        {
            Log.Information($"post Create jobtitle");

            var enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
            ViewData["CompanyName"] = companyName;

            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }


            if (!ModelState.IsValid)
            {
                Log.Warning($"Create Job title. request is not valid");


                ViewBag.JobTitles = _jobTitleService.GetAllJobTitles(HttpContext.Session.GetInt32("EnterpriseId").Value).Result;
                ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

                return View("~/Views/Administrator/JobTitles/AddJobTitle.cshtml", request);
            }




            
            if (enterpriseId == null)
            {
                Log.Error($"Create job title, enterprise  id is nill,request{request}");

                return RedirectToAction("Login", "Login");
            }

            var enterprise = await _enterpriseService.GetEnterprise(enterpriseId);
            if (enterprise == null)
            {
                Log.Error($"Create job title, enterprise  is nill,request{request}");

                return RedirectToAction("Login", "Login");
            }

            // Перевірка на існування посади
            if ((await _jobTitleService.GetAllJobTitles(enterpriseId))
                .Any(n => n.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("Name", "Посада з такою назвою вже існує.");
                Log.Warning($"Create Job title. Job title with name '{request.Name}' already exists.");
                SetViewData();

                // Повертаємо модель з помилкою до сторінки
                return View("~/Views/Administrator/JobTitles/AddJobTitle.cshtml", request);
            }


            var jobTitle = new JobTitle
            {
                Name = request.Name,
                Enterprise = enterprise
            };

            await _jobTitleService.AddingRole(jobTitle);

            

            Log.Information($"jobTitle was succesfully created");
            ViewBag.JobTitles = await _jobTitleService.GetAllJobTitles(HttpContext.Session.GetInt32("EnterpriseId").Value);
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

            return RedirectToAction("JobTitles", "JobTitles");


           
        }

        [HttpPost]
        [Route("DeleteJobTitle/id")]
        [UserExists]
        public async Task<IActionResult> DeleteJobTitle(int id)
        {
            Log.Information($"deleting jobTitle with Id:({id})");
            await _jobTitleService.DeleteJobTitle(id);

            Log.Information($"jobTitle with id:({id})was succesfully deleted");
            return RedirectToAction("JobTitles");
        }
    }




}
