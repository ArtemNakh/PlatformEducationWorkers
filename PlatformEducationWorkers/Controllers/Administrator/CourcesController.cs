using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Models;
using PlatformEducationWorkers.Request;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Route("Admin")]
    [Area("Administrator")]
    public class CourcesController : Controller
    {
        private readonly ICourcesService _courceService;
        private readonly IUserResultService _userResultService;
        private readonly IJobTitleService _jobTitleService;

        public CourcesController(ICourcesService courceService, IJobTitleService jobTitleService, IUserResultService userResultService)
        {
            _courceService = courceService;
            _jobTitleService = jobTitleService;
            _userResultService = userResultService;
        }

        [Route("Cources")]
        [UserExists]
        public async Task<IActionResult> Index()
        {
            //todo enterprice
            var cources = await _courceService.GetAllCourcesEnterprice(Convert.ToInt32("EnterpriseId"));
            ViewBag.Cources = cources;
            return View("~/Views/Administrator/Cources/Index.cshtml");
        }


        [HttpGet]
        [Route("Create")]
        [UserExists]
        public async Task<IActionResult> CreateCource()
        {
            //todo enterprice
            var jobTitles = await _jobTitleService.GetRole(Convert.ToInt32("EnterpriseId"));
            ViewBag.JobTitles = jobTitles;
            return View("~/Views/Administrator/Cources/CreateCource.cshtml");
        }


        [HttpPost]
        [Route("Create")]
        [UserExists]
        public async Task<IActionResult> CreateCource(CreateCourceRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Administrator/Cources/CreateCource.cshtml", request);
            }

            var jobTitles = new List<JobTitle>();

            foreach (var jobTitleId in request.AccessRoleIds)
            {
                var jobTitle = await _jobTitleService.GetRole(jobTitleId);
                if (jobTitle != null)
                {
                    jobTitles.Add(jobTitle);
                }
            }

            var newCource = new Cources
            {
                TitleCource = request.TitleCource,
                Description = request.Description,
                ContentCourse = request.ContentCourse,
                Questions = request.Questions,
                DateCreate = DateTime.UtcNow,
                Enterprise = new Enterprice { Id = request.EnterpriseId },
                AccessRoles = jobTitles
            };

            await _courceService.AddCource(newCource);

            return RedirectToAction("Index");
        }


        [HttpGet]
        [Route("Detail/{id}")]
        [UserExists]
        public async Task<IActionResult> Detail(int id)
        {
            var cource = await _courceService.GetCourcesById(id);
            if (cource == null)
            {
                return NotFound();
            }

            ViewBag.Cource = cource;
            return View("~/Views/Administrator/Cources/DetailCource.cshtml");
        }

        [HttpGet]
        [Route("HistoryPassage")]
        [UserExists]
        public async Task<IActionResult> HistoryPassage(int id)
        {
            //todo
            var historyPassages = await _userResultService.GetAllResultEnterprice(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId")));
            if (historyPassages == null)
            {
                return NotFound();
            }

            ViewBag.HistoryPassages = historyPassages;
            return View("~/Views/Administrator/Cources/HistoryPassage.cshtml");
        }



    }
}
