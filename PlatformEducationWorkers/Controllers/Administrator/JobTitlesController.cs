using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    public class JobTitlesController : Controller
    {
        private readonly JobTitleService jobTitleService;

        public JobTitlesController(JobTitleService jobTitleService)
        {
            this.jobTitleService = jobTitleService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(JobTitle jobTitle)
        {
            if (ModelState.IsValid)
            {
                jobTitleService.AddingRole(jobTitle);
                return RedirectToAction("Create", "Users"); // перенаправлення до CreateUser
            }
            return View(jobTitle); // повернення до в'юхи з помилками
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
