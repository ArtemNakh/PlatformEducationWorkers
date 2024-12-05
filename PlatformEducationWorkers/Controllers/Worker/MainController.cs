﻿using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;

namespace PlatformEducationWorkers.Controllers.Worker
{
    [Route("Worker")]
    [Area("Worker")]
    public class MainController : Controller
    {
        private readonly ILogger<MainController> _logger;
        private readonly IEnterpriseService _enterpriseService;
        private readonly ICourcesService _courseService;


        public MainController(ILogger<MainController> logger, IEnterpriseService enterpriseService, ICourcesService courceService)
        {
            _logger = logger;
            _enterpriseService = enterpriseService;
            _courseService = courceService;
        }

        [HttpGet]
        [Route("Main")]
        [UserExists]
        public async Task<IActionResult> Main()
        {
            _logger.LogInformation("User accessed the Main page of the Worker area.");

            try
            {
                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                int workerId = HttpContext.Session.GetInt32("UserId").Value; 

                var companyName = (await _enterpriseService.GetEnterprice(enterpriseId)).Title;
                var unfinishedCourses = (await _courseService.GetUncompletedCourcesForUser( workerId,enterpriseId));

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
