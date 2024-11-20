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
        private readonly ILogger<CourcesController> _logger;

        public CourcesController(ICourcesService courceService, IJobTitleService jobTitleService, IUserResultService userResultService, IEnterpriceService enterpriceService, ILogger<CourcesController> logger)
        {
            _courceService = courceService;
            _jobTitleService = jobTitleService;
            _userResultService = userResultService;
            _enterpriceService = enterpriceService;
            _logger = logger;
        }

        [HttpGet]
        [Route("Cources")]
        [UserExists]
        public async Task<IActionResult> Index()
        {
            //todo enterprice
            _logger.LogInformation("Accessing Cources Index");
            var cources = await _courceService.GetAllCourcesEnterprice(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId")));
            ViewBag.Cources = cources.ToList();
            return View("~/Views/Administrator/Cources/Index.cshtml");
        }


        [HttpGet]
        [Route("Create")]
        [UserExists]
        public async Task<IActionResult> CreateCource()
        {
            _logger.LogInformation("Accessing Create Cource");
            var jobTitles = await _jobTitleService.GetAllJobTitles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId")));
            if (jobTitles == null || !jobTitles.Any())
            {
                ViewBag.JobTitles = new List<JobTitle>();  
            }
            else
            {
                ViewBag.JobTitles = jobTitles.ToList();  
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
                _logger.LogWarning("Create Cource failed due to invalid model state");
                return View("~/Views/Administrator/Cources/CreateCource.cshtml", request);
            }
            
            _logger.LogInformation("Creating new Cource: {TitleCource}", request.TitleCource);

            Enterprice enterprice = _enterpriceService.GetEnterprice(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"))).Result;

            string questions = JsonConvert.SerializeObject(request.Questions);
            string context = JsonConvert.SerializeObject(request.ContentCourse);
            var jobTitles = new List<JobTitle>();

            foreach (var jobTitleId in request.AccessRoleIds)
            {
                var jobTitle = await _jobTitleService.GetJobTitle(jobTitleId);
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
                AccessRoles = jobTitles,
                ShowCorrectAnswers = request.ShowCorrectAnswers,
                ShowSelectedAnswers=request.ShowUserAnwers,
            };

            await _courceService.AddCource(newCource);
            _logger.LogInformation("Cource {TitleCource} created successfully", request.TitleCource);

            return RedirectToAction("Index");
        }


        [HttpGet]
        [Route("Detail/{id}")]
        [UserExists]
        public async Task<IActionResult> DetailCource(int id)
        {
            _logger.LogInformation("Accessing Detail Cource for ID: {CourceId}", id);

            var cource = await _courceService.GetCourcesById(id);
            if (cource == null)
            {
                _logger.LogWarning("Cource not found for ID: {CourceId}", id);

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
            _logger.LogInformation("Accessing History Passage for Cource ID: {CourceId}", id);

            var historyPassages = await _userResultService.GetAllResultEnterprice(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId")));
            if (historyPassages == null)
            {
                _logger.LogWarning("No history passages found for Enterprise ID: {EnterpriseId}", HttpContext.Session.GetString("EnterpriseId"));

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
            _logger.LogInformation("Attempting to delete Cource with ID: {CourceId}", id);

            var cource = await _courceService.GetCourcesById(id);
            if (cource == null)
            {
                _logger.LogWarning("Cource not found for ID: {CourceId}", id);

                return NotFound();
            }


            var courcePassages = await _userResultService.SearchUserResult(cource.Id);
            if (courcePassages != null)
            {
                await _userResultService.DeleteResult(courcePassages.Id);
                _logger.LogInformation("Deleted user result for Cource ID: {CourceId}", cource.Id);

            }
            await _courceService.DeleteCource(cource.Id);
            _logger.LogInformation("Cource with ID: {CourceId} deleted successfully", cource.Id);

            return RedirectToAction("Index");

        }







        [HttpGet]
        [Route("EditCource")]
        [UserExists]
        public async Task<IActionResult> EditCource(int id)
        {
            _logger.LogInformation("Accessing Edit Cource for ID: {CourceId}", id);

            var cource = await _courceService.GetCourcesById(id);
            if (cource == null)
            {
                _logger.LogWarning("Cource not found for ID: {CourceId}", id);

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
                _logger.LogWarning("Edit Cource failed due to invalid model state");

                return View(request);
            }

            Cources cource =await _courceService.GetCourcesById(request.Id);
            if (cource == null)
            {
                _logger.LogWarning("Cource not found for ID: {CourceId}", request.Id);

                return NotFound();
            }

            // Оновлюємо дані курсу
            cource.TitleCource = request.TitleCource;
            cource.Description = request.Description;
            cource.ContentCourse = request.ContentCourse;
            cource.Questions = JsonConvert.SerializeObject(request.Questions);

            await _courceService.UpdateCource(cource);
            _logger.LogInformation("Cource with ID: {CourceId} updated successfully", cource.Id);

            return RedirectToAction("Detail",cource.Id);
        }

    }
}