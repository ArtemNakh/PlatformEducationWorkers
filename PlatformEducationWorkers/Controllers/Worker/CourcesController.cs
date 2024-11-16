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


        public CourcesController(ICourcesService courcesService, IUserResultService userResultService, IUserService userService)
        {
            _courcesService = courcesService;
            _userResultService = userResultService;
            _userService = userService;
        }

        // Метод для показу всіх непройдених курсів
        [HttpGet]
        [Route("Cources")]
        [UserExists]
        public async Task<IActionResult> Index()
        {
             //todo validation
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId")); 
            int enterpriseId = Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"));

            var uncompletedCourses = await _courcesService.GetUncompletedCourcesForUser(userId, enterpriseId);

            ViewBag.UncompletedCources = uncompletedCourses;

            return View("~/Views/Worker/Cources/Index.cshtml");
        }


        [HttpGet]
        [Route("Statistics")]
        [UserExists]
        public async Task<IActionResult> StatisticCources()
        {
            // Отримуємо список курсів і їхню статистику для відображення
           
            int userId = Convert.ToInt32(Convert.ToInt32( HttpContext.Session.GetString("UserId")));

            var coursesStatistics = await _userResultService.GetAllUserResult(userId);

            ViewBag.CoursesStatistics = coursesStatistics;

            return View("~/Views/Worker/Cources/Statistics.cshtml");
        }

        //
        [HttpGet]
        [Route("DetailCource")]
        [UserExists]
        public async Task<IActionResult> DetailCource(int id)
        {
            var courseDetail = await _courcesService.GetCourcesById(id);
            if (courseDetail == null)
            {
                return NotFound();
            }

            // Десеріалізуємо ContentCourse у список рядків
            List<string> content = new List<string>();
            if (!string.IsNullOrEmpty(courseDetail.ContentCourse))
            {
                try
                {
                    content = JsonConvert.DeserializeObject<List<string>>(courseDetail.ContentCourse);
                }
                catch (JsonException)
                {
                    // Обробка помилки десеріалізації
                    // Можна додати логування або інші дії
                }
            }

            // Десеріалізуємо Questions у список об'єктів QuestionRequest
            List<QuestionContextRequest> questions = new List<QuestionContextRequest>();
            if (!string.IsNullOrEmpty(courseDetail.Questions))
            {
                try
                {
                    questions = JsonConvert.DeserializeObject<List<QuestionContextRequest>>(courseDetail.Questions);
                }
                catch (JsonException)
                {
                    // Обробка помилки десеріалізації
                    // Можна додати логування або інші дії
                }
            }

            // Передаємо дані в ViewBag
            ViewBag.Course = courseDetail;
            ViewBag.Content = content;
            ViewBag.Questions = questions;
            return View("~/Views/Worker/Cources/DetailCource.cshtml");
        
        }


        // Метод для перегляду курсу та його питань
        [HttpGet]
        [Route("PassageCource")]
        [UserExists]
        public async Task<IActionResult> PassageCource(int courseId)
        {
            var course = await _courcesService.GetCourcesById(courseId);
            if (course == null)
            {
                return NotFound();
            }

            // Десеріалізуємо питання курсу
            List<QuestionContextRequest> questions = new List<QuestionContextRequest>();
            if (!string.IsNullOrEmpty(course.Questions))
            {
                try
                {
                    questions = JsonConvert.DeserializeObject<List<QuestionContextRequest>>(course.Questions);
                }
                catch (JsonException)
                {
                    // Обробка помилки десеріалізації
                }
            }

            ViewBag.Course = course;
            ViewBag.Questions = questions;

            return View("~/Views/Worker/Cources/PassageCource.cshtml");
        }

        [HttpPost]
        [Route("PassageCource")]
        [UserExists]
        public async Task<IActionResult> SaveResultCource(UserResultRequest userResultRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var course = await _courcesService.GetCourcesById(userResultRequest.CourseId);
            if (course == null)
            {
                return NotFound();
            }

            // Розраховуємо максимальний рейтинг та рейтинг користувача
            int maxRating = userResultRequest.Questions.Count;
            int userRating = userResultRequest.Questions.Count(q => q.IsCorrect);

            User user=await _userService.GetUser(Convert.ToInt32(HttpContext.Session.GetString("UserId")));
            if(user==null)
            {
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

            return RedirectToAction("Index");
        }
    }
}
