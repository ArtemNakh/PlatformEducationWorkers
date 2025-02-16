﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core;
using PlatformEducationWorkers.Models.Questions;
using PlatformEducationWorkers.Models.UserResults;
using PlatformEducationWorkers.Request.PassageCource;
using Serilog;

namespace PlatformEducationWorkers.Controllers.Worker
{
    [Area("Worker")]
    public class CoursesController : Controller
    {
        private readonly ICourseService _coursesService;
        private readonly IUserResultService _userResultService;
        private readonly IEnterpriseService _enterpriseService;
        private readonly IUserService _userService;

        public CoursesController(ICourseService courcesService, IUserResultService userResultService, IUserService userService, IEnterpriseService enterpriseService)
        {
            _coursesService = courcesService;
            _userResultService = userResultService;
            _userService = userService;
            _enterpriseService = enterpriseService;
        }

        // Метод для показу всіх непройдених курсів
        [HttpGet]
        [Route("Cources")]
        [UserExists]
        public async Task<IActionResult> UncompleteCourses()
        {
            Log.Information($"open the page UncompleteCourses");
            try
            {
                int userId = HttpContext.Session.GetInt32("UserId").Value;
                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;

                IEnumerable<Courses> uncompletedCourses = await _coursesService.GetUncompletedCoursesForUser(userId, enterpriseId);
                
                var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
                byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
                if (avatarBytes != null && avatarBytes.Length > 0)
                {
                    string base64Avatar = Convert.ToBase64String(avatarBytes);
                    ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
                }
                else
                {
                    ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
                }
                ViewData["CompanyName"] = companyName;
                ViewBag.UncompletedCources = uncompletedCourses;

                return View("~/Views/Worker/Cources/UncompleteCourses.cshtml");
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR open the page UncompleteCourses");
                return StatusCode(500, "Сталася помилка.");
            }
        }

        [HttpGet]
        [Route("Statistics")]
        [UserExists]
        public async Task<IActionResult> StatisticCourses()
        {
            Log.Information($"open the page StatisticCourses");
            try
            {
                int userId = HttpContext.Session.GetInt32("UserId").Value;

                var coursesStatistics = (await _userResultService.GetAllUserResult(userId)).ToList();

                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
                byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
                if (avatarBytes != null && avatarBytes.Length > 0)
                {
                    string base64Avatar = Convert.ToBase64String(avatarBytes);
                    ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
                }
                else
                {
                    ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
                }
                ViewData["CompanyName"] = companyName;
                ViewBag.CoursesStatistics = coursesStatistics;
                return View("~/Views/Worker/Cources/Statistics.cshtml");
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR open the page StatisticCourses");
                return StatusCode(500, "Сталася помилка.");
            }
        }

        [HttpGet]
        [Route("ResultCourse")]
        [UserExists]
        public async Task<IActionResult> ResultCourse(int id)
        {
            Log.Information($"open the page result course");
            try
            {
                int userId = HttpContext.Session.GetInt32("UserId").Value;

                var courseResult = await _userResultService.GetUserResult(id);
                if (courseResult == null)
                {


                    return NotFound();
                }

                // Обробка вмісту курсу
                string content = "";
                List<UserQuestionRequest> questions = new() ;


                if (!string.IsNullOrEmpty(courseResult.Course.ContentCourse))
                {
                    try
                    {
                        content = JsonConvert.DeserializeObject<string>(courseResult.Course.ContentCourse);
                        
                    }
                    catch (JsonException ex)
                    {
                        Log.Error($"Error json deserialize contentCourse, error({ex}) ");
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
                        Log.Error($"Error json deserialize user result(answerJson), error({ex}) ");
                    }
                }

                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
                byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
                if (avatarBytes != null && avatarBytes.Length > 0)
                {
                    string base64Avatar = Convert.ToBase64String(avatarBytes);
                    ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
                }
                else
                {
                    ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
                }
                ViewData["CompanyName"] = companyName;
                ViewBag.Course = courseResult.Course;
                ViewBag.Result = courseResult;
                ViewBag.Content = content;
                ViewBag.Questions = questions;

                return View("~/Views/Worker/Cources/Result.cshtml");
            }
            catch (Exception ex)
            {
                Log.Error($"Error open the page ResultCourse");
                return StatusCode(500, "Сталася помилка.");
            }

        }

        [HttpGet]
        [Route("PassageCource")]
        [UserExists]
        public async Task<IActionResult> PassageCourse(int courseId)
        {
            Log.Information($"open the page PassageCource");
            try
            {
                var course = await _coursesService.GetCoursesById(courseId);
                if (course == null)
                {
                    Log.Error($"course is null, course id:({courseId})");

                    return NotFound();
                }

                List<QuestionContext> questions = new();
                if (!string.IsNullOrEmpty(course.Questions))
                {
                    try
                    {
                         questions = JsonConvert.DeserializeObject<List<QuestionContext>>(course.Questions);
                        
                    }
                    catch (JsonException ex)
                    {
                        Log.Error($"Error json deserialize questions, error({ex}) ");
                    }
                }

               course.ContentCourse= JsonConvert.DeserializeObject<string>(course.ContentCourse);
                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
                byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
                if (avatarBytes != null && avatarBytes.Length > 0)
                {
                    string base64Avatar = Convert.ToBase64String(avatarBytes);
                    ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
                }
                else
                {
                    ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
                }

                ViewData["CompanyName"] = companyName;
                ViewBag.Course = course;
                ViewBag.Questions = questions;

                return View("~/Views/Worker/Cources/PassageCource.cshtml");
            }
            catch (Exception ex)
            {
                Log.Error($"Error open the page passage course");

                return StatusCode(500, "Сталася помилка.");
            }
        }

        [HttpPost]
        [Route("PassageCource")]
        [UserExists]
        public async Task<IActionResult> SaveResultCourse(UserResultRequest userResultRequest)
        {
            Log.Information($"post request page PassageCource");
            try
            {
                if (!ModelState.IsValid)
                {
                    Log.Warning($"models is not valid, models: {userResultRequest}");

                    // Збираємо помилки валідації
                    var validationErrors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    ViewBag.ValidationErrors = validationErrors;
                    ViewBag.ErrorMessage = "Перевірте правильність заповнення форми.";

                    var courserep = await _coursesService.GetCoursesById(userResultRequest.CourseId);
                    ViewBag.Course = courserep;

                    var questions  = JsonConvert.DeserializeObject<List<QuestionContext>>(courserep.Questions);
                    
                    ViewBag.Questions = questions;
                    byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
                    if (avatarBytes != null && avatarBytes.Length > 0)
                    {
                        string base64Avatar = Convert.ToBase64String(avatarBytes);
                        ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
                    }
                    else
                    {
                        ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
                    }
                    return View("~/Views/Worker/Cources/PassageCource.cshtml");
                }

                var course = await _coursesService.GetCoursesById(userResultRequest.CourseId,true);
                if (course == null)
                {
                    Log.Error($"course is null, courseId:({userResultRequest.CourseId})");
                    return NotFound();
                }

                // Загальна кількість вірних відповідей
                int totalCorrectAnswers = userResultRequest.Questions
                    .SelectMany(q => q.Answers)
                    .Count(a => a.IsCorrectAnswer);

                // Кількість правильних відповідей, вибраних користувачем
                int correctSelectedAnswers = userResultRequest.Questions
                    .SelectMany(q => q.Answers)
                    .Count(a => a.IsCorrectAnswer && a.IsSelected);

                User user = await _userService.GetUser(HttpContext.Session.GetInt32("UserId").Value);
                if (user == null)
                {
                    Log.Error($"user is null, courseId:({HttpContext.Session.GetInt32("UserId").Value})");

                    return RedirectToAction("Login", "Login");
                }

                string userAnswersJson = JsonConvert.SerializeObject(userResultRequest.Questions);

                var userResult = new UserResults
                {
                    User = user,
                    Course =course,
                    DateCompilation = DateTime.Now,
                    Rating = correctSelectedAnswers,
                    MaxRating = totalCorrectAnswers,
                    answerJson = userAnswersJson,

                };

                await _userResultService.AddResult(userResult);
                Log.Information($"passage course was succesfully added for user(id):{user.Id}/ course(id):{course.Id}");

                return RedirectToAction("UncompleteCourses");
            }
            catch (Exception ex)
            {
                Log.Error($"Error in post request PassageCourse: {ex.Message}");

                ViewBag.ErrorMessage = "Сталася помилка при збереженні результатів. Спробуйте ще раз.";
                return View("~/Views/Worker/Cources/PassageCource.cshtml");
            }
        }


        [HttpGet]
        [Route("Theory")]
        [UserExists]
        public async Task<IActionResult> TheoryCourse(int courseId)
        {
            Log.Information($"open the page TheoryCourse");

            try
            {
                var course = await _coursesService.GetCoursesById(courseId);
                course.ContentCourse= JsonConvert.DeserializeObject<string>(course.ContentCourse);
                if (course == null)
                {
                    Log.Error($"Course is null, courseId({courseId})");

                    return NotFound();
                }

                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
                byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
                if (avatarBytes != null && avatarBytes.Length > 0)
                {
                    string base64Avatar = Convert.ToBase64String(avatarBytes);
                    ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
                }
                else
                {
                    ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
                }
                ViewData["CompanyName"] = companyName;
                ViewBag.Course = course;

                return View("~/Views/Worker/Cources/TheoryCourse.cshtml");
            }
            catch (Exception ex)
            {
                Log.Information($"Error open the page TheoryCourse");


                return StatusCode(500, "Сталася помилка.");
            }

        }

    }
}

