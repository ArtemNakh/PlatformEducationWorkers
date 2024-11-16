using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Models.Questions;
using PlatformEducationWorkers.Request;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Route("Admin")]
    [Area("Administrator")]
    public class CourcesController : Controller
    {
        private readonly ICourcesService _courceService;
        private readonly IUserResultService _userResultService;
        private readonly IJobTitleService _jobTitleService;
        private readonly IEnterpriceService _enterpriceService;

        public CourcesController(ICourcesService courceService, IJobTitleService jobTitleService, IUserResultService userResultService, IEnterpriceService enterpriceService)
        {
            _courceService = courceService;
            _jobTitleService = jobTitleService;
            _userResultService = userResultService;
            _enterpriceService = enterpriceService;
        }

        [HttpGet]
        [Route("Cources")]
        [UserExists]
        public async Task<IActionResult> Index()
        {
            //todo enterprice
            var cources = await _courceService.GetAllCourcesEnterprice(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId")));
            ViewBag.Cources = cources.ToList();
            return View("~/Views/Administrator/Cources/Index.cshtml");
        }


        [HttpGet]
        [Route("Create")]
        [UserExists]
        public async Task<IActionResult> CreateCource()
        {
            var jobTitles = await _jobTitleService.GetAllRoles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId")));
            if (jobTitles == null || !jobTitles.Any())
            {
                ViewBag.JobTitles = new List<JobTitle>();  // Якщо ролі відсутні, передаємо порожній список
            }
            else
            {
                ViewBag.JobTitles = jobTitles.ToList();  // Передача списку ролей в ViewBag
            }
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
            Enterprice enterprice = _enterpriceService.GetEnterprice(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"))).Result;

            string questions = JsonConvert.SerializeObject(request.Questions);
            string context = JsonConvert.SerializeObject(request.ContentCourse);
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
                ContentCourse = context,
                Questions = questions,
                DateCreate = DateTime.UtcNow,
                Enterprise = enterprice,
                AccessRoles = jobTitles
            };

            await _courceService.AddCource(newCource);

            return RedirectToAction("Index");
        }


        [HttpGet]
        [Route("Detail/{id}")]
        [UserExists]
        public async Task<IActionResult> DetailCource(int id)
        {
            var cource = await _courceService.GetCourcesById(id);
            if (cource == null)
            {
                return NotFound();
            }

            // Десеріалізація JSON у відповідні об'єкти
            List<QuestionContextRequest> questions = null;
            string contentCourse = null;

            if (!string.IsNullOrEmpty(cource.Questions))
            {
                questions = JsonConvert.DeserializeObject<List<QuestionContextRequest>>(cource.Questions);
            }

            if (!string.IsNullOrEmpty(cource.ContentCourse))
            {
                contentCourse = JsonConvert.DeserializeObject<string>(cource.ContentCourse);
            }

            // Передача розпарсених даних окремо у ViewBag
            ViewBag.Questions = questions;
            ViewBag.ContentCourse = contentCourse;
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

        [HttpPost]
        [Route("DeleteCource")]
        [UserExists]
        public async Task<IActionResult> DeleteCource(int id)
        {

            var cource = await _courceService.GetCourcesById(id);
            if (cource == null)
            {
                return NotFound();
            }


            var courcePassages = await _userResultService.SearchUserResult(cource.Id);
            if (courcePassages != null)
            {
                await _userResultService.DeleteResult(courcePassages.Id);
            }
            await _courceService.DeleteCource(cource.Id);
            return RedirectToAction("Index");

        }







        [HttpGet]
        [Route("EditCource")]
        [UserExists]
        public async Task<IActionResult> EditCource(int id)
        {
            var cource = await _courceService.GetCourcesById(id);
            if (cource == null)
            {
                return NotFound();
            }

            var request = new EditCourceRequest
            {
                Id = cource.Id,
                TitleCource = cource.TitleCource,
                Description = cource.Description,
                ContentCourse = cource.ContentCourse,
                Questions = JsonConvert.DeserializeObject<List<QuestionContextRequest>>(cource.Questions)
            };

            ViewBag.Cource = request;

            return View("~/Views/Administrator/Cources/EditCource.cshtml",request);

        }

        [HttpPost]
        [Route("EditCource")]
        [UserExists]
        public async Task<IActionResult> EditCource(EditCourceRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            Cources cource =await _courceService.GetCourcesById(request.Id);
            if (cource == null)
            {
                return NotFound();
            }

            // Оновлюємо дані курсу
            cource.TitleCource = request.TitleCource;
            cource.Description = request.Description;
            cource.ContentCourse = request.ContentCourse;
            cource.Questions = JsonConvert.SerializeObject(request.Questions);

            await _courceService.UpdateCource(cource);

            return RedirectToAction("Detail",cource.Id);
        }

    }
}