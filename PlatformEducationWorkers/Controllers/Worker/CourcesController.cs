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
        public readonly IEnterpriseService _enterpriseService;
        public readonly IUserService _userService;
        private readonly ILogger<CourcesController> _logger;

        public CourcesController(ICourcesService courcesService, IUserResultService userResultService, IUserService userService, ILogger<CourcesController> logger, IEnterpriseService enterpriseService)
        {
            _courcesService = courcesService;
            _userResultService = userResultService;
            _userService = userService;
            _logger = logger;
            _enterpriseService = enterpriseService;
        }

        // Метод для показу всіх непройдених курсів
        [HttpGet]
        [Route("Cources")]
        [UserExists]
        public async Task<IActionResult> UncompleteCourses()
        {
            try
            {
                _logger.LogInformation("Завантаження непройдених курсів.");

                int userId = HttpContext.Session.GetInt32("UserId").Value;
                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;

                var uncompletedCourses = await _courcesService.GetUncompletedCourcesForUser(userId, enterpriseId);

                var companyName = (await _enterpriseService.GetEnterprice(enterpriseId)).Title;

                ViewData["CompanyName"] = companyName;
                ViewBag.UncompletedCources = uncompletedCourses;
                return View("~/Views/Worker/Cources/UncompleteCourses.cshtml");
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

                int userId = HttpContext.Session.GetInt32("UserId").Value;

                var coursesStatistics = await _userResultService.GetAllUserResult(userId);

                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                var companyName = (await _enterpriseService.GetEnterprice(enterpriseId)).Title;

                ViewData["CompanyName"] = companyName;
                ViewBag.CoursesStatistics = coursesStatistics;
                return View("~/Views/Worker/Cources/Statistics.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка під час завантаження статистики курсів.");
                return StatusCode(500, "Сталася помилка.");
            }
        }


        [HttpGet]
        [Route("ResultCourse")]
        [UserExists]
        public async Task<IActionResult> ResultCourse(int id)
        {
            try
            {
                _logger.LogInformation("Завантаження деталей курсу з ID {CourseId}.", id);

                var courseResult = await _userResultService.SearchUserResult(id);//await _courcesService.GetCourcesById(id);
                if (courseResult == null)
                {
                    _logger.LogWarning("Курс з ID {CourseId} не знайдено.", id);
                    return NotFound();
                }

                // Обробка вмісту курсу
                string content = "";
                List<UserQuestionRequest> questions = new() ;/*= new List<UserQuestionRequest>();*/

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

                ViewData["CompanyName"] = companyName;
                ViewBag.Course = courseResult.Cource;
                ViewBag.Result = courseResult;
                ViewBag.Content = content;
                ViewBag.Questions = questions;

                return View("~/Views/Worker/Cources/Result.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка під час завантаження деталей курсу з ID {CourseId}.", id);
                return StatusCode(500, "Сталася помилка.");
            }

        }

        
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
                        //questions = JsonConvert.DeserializeObject<List<UserQuestionRequest>>(course.Questions);
                        questions = JsonConvert.DeserializeObject<List<QuestionContextRequest>>(course.Questions);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Помилка десеріалізації питань для курсу {CourseId}.", courseId);
                    }
                }
                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                var companyName = (await _enterpriseService.GetEnterprice(enterpriseId)).Title;

                ViewData["CompanyName"] = companyName;
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

                int maxRating = userResultRequest.Questions.Select(a=>a.Answers.Where(an=>an.IsCorrectAnswer)).Count();
                int userRating = userResultRequest.Questions.Select(n => n.Answers.Where(b => b.IsSelected == true && b.IsCorrectAnswer == true)).Count();





                User user = await _userService.GetUser(HttpContext.Session.GetInt32("UserId").Value);
                if (user == null)
                {
                    _logger.LogWarning("Користувача з ID {UserId} не знайдено.", HttpContext.Session.GetString("UserId"));
                    return RedirectToAction("Login", "Login");
                }
                string userAnswersJson = JsonConvert.SerializeObject(userResultRequest.Questions);


                var userResult = new UserResults
                {
                    User = user,
                    Cource = course,
                    DateCompilation = DateTime.Now,
                    Rating = userRating,
                    MaxRating = maxRating,
                    answerJson = userAnswersJson,

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


        [HttpGet]
        [Route("Theory")]
        [UserExists]
        public async Task<IActionResult> TheoryCourse(int courseId)
        {
            try
            {
                _logger.LogInformation("Теорія курса з ID {CourseId}.", courseId);

                var course = await _courcesService.GetCourcesById(courseId);
                if (course == null)
                {
                    _logger.LogWarning("Курс з ID {CourseId} не знайдено.", courseId);
                    return NotFound();
                }


                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                var companyName = (await _enterpriseService.GetEnterprice(enterpriseId)).Title;

                ViewData["CompanyName"] = companyName;
                ViewBag.Course = course;

                return View("~/Views/Worker/Cources/TheoryCourse.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка під час теорії  курса з ID {CourseId}.", courseId);
                return StatusCode(500, "Сталася помилка.");
            }

        }

    }
}

