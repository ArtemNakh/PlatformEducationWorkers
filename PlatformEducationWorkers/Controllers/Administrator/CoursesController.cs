using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Models.Questions;
using PlatformEducationWorkers.Request.CourceRequest;
using PlatformEducationWorkers.Request.PassageCource;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Route("Admin")]
    [Area("Administrator")]
    public class CoursesController : Controller
    {
        private readonly ICourcesService _courceService;
        private readonly IUserResultService _userResultService;
        private readonly IJobTitleService _jobTitleService;
        private readonly IEnterpriseService _enterpriseService;
        private readonly IUserService _userService;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(ICourcesService courceService, IJobTitleService jobTitleService, IUserResultService userResultService, IEnterpriseService enterpriceService, ILogger<CoursesController> logger, IUserService userService)
        {
            _courceService = courceService;
            _jobTitleService = jobTitleService;
            _userResultService = userResultService;
            _enterpriseService = enterpriceService;
            _logger = logger;
            _userService = userService;
        }

        [HttpGet]
        [Route("Courses")]
        [UserExists]
        public async Task<IActionResult> Courses()
        {
            _logger.LogInformation("Accessing Cources Index");
            var cources = await _courceService.GetAllCourcesEnterprice(HttpContext.Session.GetInt32("EnterpriseId").Value);
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprice(enterpriseId)).Title;
            ViewData["CompanyName"] = companyName;
            ViewBag.Cources = cources.ToList();
            return View("~/Views/Administrator/Cources/Courses.cshtml");
        }


        [HttpGet]
        [Route("Create")]
        [UserExists]
        public async Task<IActionResult> CreateCourse()
        {
            _logger.LogInformation("Accessing Create Cource");
            var jobTitles = await _jobTitleService.GetAllJobTitles(HttpContext.Session.GetInt32("EnterpriseId").Value);
            if (jobTitles == null || !jobTitles.Any())
            {
                ViewBag.JobTitles = new List<JobTitle>();  
            }
            else
            {
                ViewBag.JobTitles = jobTitles.ToList();  
            }

            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprice(enterpriseId)).Title;
            ViewData["CompanyName"] = companyName;
            return View("~/Views/Administrator/Cources/CreateCource.cshtml");
        }


        [HttpPost]
        [Route("Create")]
        [UserExists]
        public async Task<IActionResult> CreateCourse(CreateCourceRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Create Cource failed due to invalid model state");
                var jobTitleslist = await _jobTitleService.GetAllJobTitles(HttpContext.Session.GetInt32("EnterpriseId").Value);
                if (jobTitleslist == null || !jobTitleslist.Any())
                {
                    ViewBag.JobTitles = new List<JobTitle>();
                }
                else
                {
                    ViewBag.JobTitles = jobTitleslist.ToList();
                }
                return View("~/Views/Administrator/Cources/CreateCource.cshtml", request);
            }
            
            _logger.LogInformation("Creating new Cource: {TitleCource}", request.TitleCource);

            Enterprice enterprice =await _enterpriseService.GetEnterprice(HttpContext.Session.GetInt32("EnterpriseId").Value);

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
                ShowSelectedAnswers=request.ShowUserAnswers,
                ShowContextPassage = request.ShowContextPassage,
            };

            await _courceService.AddCource(newCource);
            _logger.LogInformation("Cource {TitleCource} created successfully", request.TitleCource);

            return RedirectToAction("Index");
        }


        [HttpGet]
        [Route("Detail/{id}")]
        [UserExists]
        public async Task<IActionResult> DetailCourse(int id)
        {
            _logger.LogInformation("Accessing Detail Cource for ID: {CourceId}", id);

            var cource = await _courceService.GetCourcesById(id);
            if (cource == null)
            {
                _logger.LogWarning("Cource not found for ID: {CourceId}", id);

                return NotFound();
            }

            // Десеріалізація JSON у відповідні об'єкти
            List<QuestionContext> questions = null;
            string contentCourse = null;

            if (!string.IsNullOrEmpty(cource.Questions))
            {
                questions = JsonConvert.DeserializeObject<List<QuestionContext>>(cource.Questions);
            }

            if (!string.IsNullOrEmpty(cource.ContentCourse))
            {
                contentCourse = JsonConvert.DeserializeObject<string>(cource.ContentCourse);
            }

            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprice(enterpriseId)).Title;
            ViewData["CompanyName"] = companyName;
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
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var historyPassages = await _userResultService.GetAllResultEnterprice(enterpriseId);
            var courses=await _courceService.GetAllCourcesEnterprice(enterpriseId);
            if (historyPassages == null)
            {
                _logger.LogWarning("No history passages found for Enterprise ID: {EnterpriseId}", enterpriseId);

                return NotFound();
            }


            var companyName = (await _enterpriseService.GetEnterprice(enterpriseId)).Title;
            ViewData["CompanyName"] = companyName;
            ViewBag.HistoryPassages = historyPassages;
            ViewBag.Courses = courses;
            return View("~/Views/Administrator/Cources/HistoryPassage.cshtml");
        }

        [HttpGet]
        [Route("FindHistoryPassage")]
        [UserExists]
        public async Task<IActionResult> FindHistoryPassage(int? courseId)
        {
            if(courseId == null)
            {
                _logger.LogWarning("No passages courses  for the course ID: {EnterpriseId}", courseId);

                return NotFound();
            }
            var historyPassages = await _userResultService.GetAllResultCourses(courseId.Value);
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var courses = await _courceService.GetAllCourcesEnterprice(enterpriseId);
            var companyName = (await _enterpriseService.GetEnterprice(enterpriseId)).Title;

            ViewBag.HistoryPassages = historyPassages.ToList();
            ViewBag.Courses = courses.ToList();
            ViewData["CompanyName"] = companyName;
            return View("~/Views/Administrator/Cources/HistoryPassage.cshtml");

        }


        [HttpPost]
        [Route("DeleteCource")]
        [UserExists]
        public async Task<IActionResult> DeleteCourse(int id)
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

            return RedirectToAction("Courses");

        }







        [HttpGet]
        [Route("EditCource")]
        [UserExists]
        public async Task<IActionResult> EditCourse(int id)
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
                ContentCourse = JsonConvert.DeserializeObject <string>(cource.ContentCourse),
                Questions = JsonConvert.DeserializeObject<List<QuestionContext>>(cource.Questions)
            };
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprice(enterpriseId)).Title;
            ViewData["CompanyName"] = companyName;

            return View("~/Views/Administrator/Cources/EditCource.cshtml",request);

        }

        [HttpPost]
        [Route("EditCource")]
        [UserExists]
        public async Task<IActionResult> EditCourse(EditCourceRequest request)
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

            return RedirectToAction("DetailCource   ", cource.Id);
        }


        [HttpGet]
        [Route("SearchCourses")]
        [UserExists]
        public async Task<IActionResult> SearchCourses(string searchTerm)
        {
            // Отримати всі курси
            var allCourses =await _courceService.GetAllCourcesEnterprice(HttpContext.Session.GetInt32("EnterpriseId").Value);

            // Якщо пошуковий термін не порожній, виконати фільтрацію
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                allCourses = allCourses.Where(c => c.TitleCource.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Передати відфільтровані курси у ViewBag
            ViewBag.Cources = allCourses;

            return View("~/Views/Administrator/Cources/Courses.cshtml");
        }


        [HttpGet]
        [Route("DetailPassageCourse")]
        [UserExists]
        public async Task<IActionResult> DetailPassageCourse(int id)
        {
            try
            {
                _logger.LogInformation("Завантаження деталей курсу з ID {CourseId}.", id);

                var courseResult = await _userResultService.SearchUserResult(id);
                if (courseResult == null)
                {
                    _logger.LogWarning("Курс з ID {CourseId} не знайдено.", id);
                    return NotFound();
                }

                // Обробка вмісту курсу
                string content = "";
                List<UserQuestionRequest> questions = new();

                if (!string.IsNullOrEmpty(courseResult.Cource.ContentCourse))
                {
                    try
                    {
                        content = JsonConvert.DeserializeObject<string>(courseResult.Cource.ContentCourse);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Помилка десеріалізації ContentCourse для курсу {CourseId}.", id);
                    }
                }

                if (!string.IsNullOrEmpty(courseResult.answerJson))
                {
                    try
                    {
                        questions = JsonConvert.DeserializeObject<List<UserQuestionRequest>>(courseResult.answerJson);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Помилка десеріалізації Questions для курсу {CourseId}.", id);
                    }
                }

                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                var companyName = (await _enterpriseService.GetEnterprice(enterpriseId)).Title;
                User user = courseResult.User;

                ViewData["CompanyName"] = companyName;
                ViewBag.Course = courseResult.Cource;
                ViewBag.Result = courseResult;
                ViewBag.Content = content;
                ViewBag.Questions = questions;
                ViewBag.USer = user;

                return View("~/Views/Administrator/Cources/DetailPassageCourse.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка під час завантаження деталей курсу з ID {CourseId}.", id);
                return StatusCode(500, "Сталася помилка.");
            }

        }

    }
}