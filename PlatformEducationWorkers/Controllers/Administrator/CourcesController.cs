using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Models;
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
        public async Task<IActionResult> Index()
        {
            //todo enterprice
            var cources = await _courceService.GetAllCourcesEnterprice(1/*Convert.ToInt32("EnterpriceId")*/); 
            ViewBag.Cources = cources; 
            return View("~/Views/Administrator/Cources/Index.cshtml");
        }


        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            //todo enterprice
            var jobTitles = await _jobTitleService.GetRole(1/*Convert.ToInt32("EnterpriceId")*/);
            ViewBag.JobTitles = jobTitles;
            return View("~/Views/Administrator/Cources/CreateCource.cshtml");
        }

        
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(Cources cource)
        {
            if (ModelState.IsValid)
            {
                // Додавання логіки для збереження питань у JSON форматі
                var questionsList = new List<Question>();

                var questionsJson = Request.Form["Questions"];
                if (!string.IsNullOrWhiteSpace(questionsJson))
                {
                    questionsList = JsonConvert.DeserializeObject<List<Question>>(questionsJson);
                }

                cource.Questions = JsonConvert.SerializeObject(questionsList);


                // Додайте доступні ролі до курсу
                List<JobTitle> accessRoleToTest = new List<JobTitle>();

                // Зберігаємо вибрані JobTitle
                foreach (var roleId in Request.Form["AccessRoles"])
                {
                    var jobTitle = await _jobTitleService.GetRole(int.Parse(roleId));
                    accessRoleToTest.Add(jobTitle);
                }
                cource.AccessRoles = accessRoleToTest;

                await _courceService.AddCource(cource);
                return RedirectToAction("Index");
            }


              //todo enterprice
                var jobTitles = await _jobTitleService.GetRole(1/*Convert.ToInt32("EnterpriceId")*/);
            ViewBag.JobTitles = jobTitles;
            return View(cource);
        }


        [HttpGet]
        [Route("Detail/{id}")]
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
        public async Task<IActionResult> HistoryPassage(int id)
        {
            //todo
            var historyPassages = await _userResultService.GetAllResultEnterprice(1/*(int)HttpContext.Session.GetInt32("EnterpriceId")*/); 
            if (historyPassages == null)
            {
                return NotFound(); 
            }

            ViewBag.HistoryPassages = historyPassages; 
            return View("~/Views/Administrator/Cources/HistoryPassage.cshtml");
        }



    }
}
