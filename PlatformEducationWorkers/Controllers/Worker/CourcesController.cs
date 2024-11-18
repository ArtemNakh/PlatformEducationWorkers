using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Models.Questions;
using PlatformEducationWorkers.Request.PassageCource;

namespace PlatformEducationWorkers.Controllers.Worker
{
    [Route("Workers")]
    [Area("Worker")]
    public class CourcesController : Controller
    {
        public readonly ICourcesService _courcesService;
        public readonly IUserResultService _userResultService;
        public readonly IUserService _userService;
        private readonly ILogger<CourcesController> _logger;

        public CourcesController(ICourcesService courcesService, IUserResultService userResultService, IUserService userService, ILogger<CourcesController> logger)
        {
            _courcesService = courcesService;
            _userResultService = userResultService;
            _userService = userService;
            _logger = logger;
        }

        // Метод для показу всіх непройдених курсів
        [HttpGet]
        [Route("Cources")]
        [UserExists]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Завантаження непройдених курсів.");

                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
                int enterpriseId = Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"));

                var uncompletedCourses = await _courcesService.GetUncompletedCourcesForUser(userId, enterpriseId);

                ViewBag.UncompletedCources = uncompletedCourses;
                return View("~/Views/Worker/Cources/Index.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка під час завантаження непройдених курсів.");
                return StatusCode(500, "Сталася помилка.");
            }
        }


        [HttpGet]
        [Route("Statistics")]
        [UserExists]
        public async Task<IActionResult> StatisticCources()
        {
            try
            {
                _logger.LogInformation("Завантаження статистики курсів.");

                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

                var coursesStatistics = await _userResultService.GetAllUserResult(userId);

                ViewBag.CoursesStatistics = coursesStatistics;
                return View("~/Views/Worker/Cources/Statistics.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка під час завантаження статистики курсів.");
                return StatusCode(500, "Сталася помилка.");
            }
        }

        //
        [HttpGet]
        [Route("DetailCource")]
        [UserExists]
        public async Task<IActionResult> DetailCource(int id)
        {
            try
            {
                _logger.LogInformation("Завантаження деталей курсу з ID {CourseId}.", id);

                var courseDetail = await _courcesService.GetCourcesById(id);
                if (courseDetail == null)
                {
                    _logger.LogWarning("Курс з ID {CourseId} не знайдено.", id);
                    return NotFound();
                }

                // Обробка вмісту курсу
                List<string> content = new();
                List<QuestionContextRequest> questions = new();

                if (!string.IsNullOrEmpty(courseDetail.ContentCourse))
                {
                    try
                    {
                        content = JsonConvert.DeserializeObject<List<string>>(courseDetail.ContentCourse);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Помилка десеріалізації ContentCourse для курсу {CourseId}.", id);
                    }
                }

                if (!string.IsNullOrEmpty(courseDetail.Questions))
                {
                    try
                    {
                        questions = JsonConvert.DeserializeObject<List<QuestionContextRequest>>(courseDetail.Questions);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Помилка десеріалізації Questions для курсу {CourseId}.", id);
                    }
                }

                ViewBag.Course = courseDetail;
                ViewBag.Content = content;
                ViewBag.Questions = questions;

                return View("~/Views/Worker/Cources/DetailCource.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка під час завантаження деталей курсу з ID {CourseId}.", id);
                return StatusCode(500, "Сталася помилка.");
            }

        }


        // Метод для перегляду курсу та його питань
        [HttpGet]
        [Route("PassageCource")]
        [UserExists]
        public async Task<IActionResult> PassageCource(int courseId)
        {
            try
            {
                _logger.LogInformation("Початок проходження курсу з ID {CourseId}.", courseId);

                var course = await _courcesService.GetCourcesById(courseId);
                if (course == null)
                {
                    _logger.LogWarning("Курс з ID {CourseId} не знайдено.", courseId);
                    return NotFound();
                }

                List<QuestionContextRequest> questions = new();
                if (!string.IsNullOrEmpty(course.Questions))
                {
                    try
                    {
                        questions = JsonConvert.DeserializeObject<List<QuestionContextRequest>>(course.Questions);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Помилка десеріалізації питань для курсу {CourseId}.", courseId);
                    }
                }

                ViewBag.Course = course;
                ViewBag.Questions = questions;

                return View("~/Views/Worker/Cources/PassageCource.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка під час проходження курсу з ID {CourseId}.", courseId);
                return StatusCode(500, "Сталася помилка.");
            }
        }

        [HttpPost]
        [Route("PassageCource")]
        [UserExists]
        public async Task<IActionResult> SaveResultCource(UserResultRequest userResultRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Модель UserResultRequest недійсна: {ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                var course = await _courcesService.GetCourcesById(userResultRequest.CourseId);
                if (course == null)
                {
                    _logger.LogWarning("Курс з ID {CourseId} не знайдено.", userResultRequest.CourseId);
                    return NotFound();
                }

                int maxRating = userResultRequest.Questions.Count;
                int userRating = userResultRequest.Questions.Count(q => q.IsCorrect);

                User user = await _userService.GetUser(Convert.ToInt32(HttpContext.Session.GetString("UserId")));
                if (user == null)
                {
                    _logger.LogWarning("Користувача з ID {UserId} не знайдено.", HttpContext.Session.GetString("UserId"));
                    return RedirectToAction("Login", "Login");
                }

                var userResult = new UserResults
                {
                    User = user,
                    Cource = course,
                    DateCompilation = DateTime.Now,
                    Rating = userRating,
                    MaxRating = maxRating
                };

                await _userResultService.AddResult(userResult);

                _logger.LogInformation("Результат курсу з ID {CourseId} успішно збережено.", userResultRequest.CourseId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка під час збереження результату курсу з ID {CourseId}.", userResultRequest.CourseId);
                return StatusCode(500, "Сталася помилка.");
            }
        }
    }
}
