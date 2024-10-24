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
        private readonly IJobTitleService _jobTitleService;

        public CourcesController(ICourcesService courceService, IJobTitleService jobTitleService)
        {
            _courceService = courceService;
            _jobTitleService = jobTitleService;
        }

        [Route("Cources")]
        public async Task<IActionResult> Index()
        {
            //todo enterprice
            var cources = await _courceService.GetAllCourcesEnterprice(1); // Замініть 1 на реальний ID підприємства
            ViewBag.Cources = cources; // Зберігаємо курси у ViewBag
            return View("~/Views/Administrator/Cources/Index.cshtml");
        }


        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            //todo enterprice
            //var jobTitles = await _jobTitleService.GetRole(Convert.ToInt32("EnterpriceId")); // Змініть на реальний метод отримання JobTitles
            var jobTitles = await _jobTitleService.GetRole(1); // Змініть на реальний метод отримання JobTitles
            ViewBag.JobTitles = jobTitles; // Зберігаємо список JobTitles у ViewBag
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

            // Якщо модель не валідна, відправляємо список JobTitles повторно
              //todo enterprice
               // var jobTitles = await _jobTitleService.GetRole(Convert.ToInt32("EnterpriceId"));
                var jobTitles = await _jobTitleService.GetRole(1);
            ViewBag.JobTitles = jobTitles;
            return View(cource);
        }



    }
}
