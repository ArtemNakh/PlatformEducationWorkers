using Amazon.Runtime.Internal;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Models.Azure;
using PlatformEducationWorkers.Models.Questions;
using PlatformEducationWorkers.Models.Results;
using PlatformEducationWorkers.Request.PassageCource;

namespace PlatformEducationWorkers.Controllers.Worker
{
    [Area("Worker")]
    public class CoursesController : Controller
    {
        private readonly ICoursesService _coursesService;
        private readonly IUserResultService _userResultService;
        private readonly IEnterpriseService _enterpriseService;
        private readonly IUserService _userService;
        private readonly ILoggerService _loggerService;
        private readonly AzureBlobCourseOperation AzureOperation;

        public CoursesController(ICoursesService courcesService, IUserResultService userResultService, IUserService userService, IEnterpriseService enterpriseService, ILoggerService loggerService, AzureBlobCourseOperation azureOperation)
        {
            _coursesService = courcesService;
            _userResultService = userResultService;
            _userService = userService;

            _enterpriseService = enterpriseService;
            _loggerService = loggerService;
            AzureOperation = azureOperation;
        }

        // Метод для показу всіх непройдених курсів
        [HttpGet]
        [Route("Cources")]
        [UserExists]
        public async Task<IActionResult> UncompleteCourses()
        {
            try
            {

                //await _loggerService.LogAsync(Logger.LogType.Info, $"Завантаження непройдених курсів.", HttpContext.Session.GetInt32("UserId").Value);

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
                ViewData["CompanyName"] = companyName;
                ViewBag.UncompletedCources = uncompletedCourses;
                return View("~/Views/Worker/Cources/UncompleteCourses.cshtml");
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync(Logger.LogType.Error, $"Помилка під час завантаження непройдених курсів.", HttpContext.Session.GetInt32("UserId").Value);
               
                return StatusCode(500, "Сталася помилка.");
            }
        }

        [HttpGet]
        [Route("Statistics")]
        [UserExists]
        public async Task<IActionResult> StatisticCourses()
        {
            try
            {
                //await _loggerService.LogAsync(Logger.LogType.Info, $"Завантаження статистики курсів.", HttpContext.Session.GetInt32("UserId").Value);



                int userId = HttpContext.Session.GetInt32("UserId").Value;

                var coursesStatistics = await _userResultService.GetAllUserResult(userId);

                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
                byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
                if (avatarBytes != null && avatarBytes.Length > 0)
                {
                    string base64Avatar = Convert.ToBase64String(avatarBytes);
                    ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
                }
                ViewData["CompanyName"] = companyName;
                ViewBag.CoursesStatistics = coursesStatistics;
                return View("~/Views/Worker/Cources/Statistics.cshtml");
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync(Logger.LogType.Error, $"Помилка під час завантаження статистики курсів.", HttpContext.Session.GetInt32("UserId").Value);


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

                
                var courseResult = await _userResultService.SearchUserResult(id);
                if (courseResult == null)
                {


                    return NotFound();
                }

                // Обробка вмісту курсу
                string content = "";
                List<UserQuestionRequest> questions = new() ;/*= new List<UserQuestionRequest>();*/

                if (!string.IsNullOrEmpty(courseResult.Course.ContentCourse))
                {
                    try
                    {
                        content = JsonConvert.DeserializeObject<string>(courseResult.Course.ContentCourse);
                        
                    }
                    catch (JsonException ex)
                    {
                        await _loggerService.LogAsync(Logger.LogType.Error, $"Помилка десеріалізації ContentCourse для курсу {id}.", HttpContext.Session.GetInt32("UserId").Value);


                    }
                }

                if (!string.IsNullOrEmpty(courseResult.answerJson))
                {
                    try
                    {
                        questions = JsonConvert.DeserializeObject<List<UserQuestionRequest>>(courseResult.answerJson);
                        questions=await AzureOperation.UnloadFileFromBlobAsync(questions);
                    }
                    catch (JsonException ex)
                    {
                        await _loggerService.LogAsync(Logger.LogType.Error, $"Помилка десеріалізації Questions для курсу {id}.", HttpContext.Session.GetInt32("UserId").Value);

                        
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
                ViewData["CompanyName"] = companyName;
                ViewBag.Course = courseResult.Course;
                ViewBag.Result = courseResult;
                ViewBag.Content = content;
                ViewBag.Questions = questions;

                return View("~/Views/Worker/Cources/Result.cshtml");
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync(Logger.LogType.Error, $"Помилка під час завантаження деталей курсу з ID {id}.", HttpContext.Session.GetInt32("UserId").Value);


                return StatusCode(500, "Сталася помилка.");
            }

        }

        
        [HttpGet]
        [Route("PassageCource")]
        [UserExists]
        public async Task<IActionResult> PassageCourse(int courseId)
        {
            try
            {

                //await _loggerService.LogAsync(Logger.LogType.Info, $"Початок проходження курсу з ID {courseId}.", HttpContext.Session.GetInt32("UserId").Value);

                var course = await _coursesService.GetCoursesById(courseId);
                if (course == null)
                {
                    

                    return NotFound();
                }

                List<QuestionContext> questions = new();
                if (!string.IsNullOrEmpty(course.Questions))
                {
                    try
                    {
                        //questions = JsonConvert.DeserializeObject<List<UserQuestionRequest>>(course.Questions);
                        questions = JsonConvert.DeserializeObject<List<QuestionContext>>(course.Questions);
                        questions = await AzureOperation.UnloadFileFromBlobAsync(questions);
                    }
                    catch (JsonException ex)
                    {
                        await _loggerService.LogAsync(Logger.LogType.Error, $"Помилка десеріалізації питань для курсу {courseId}.", HttpContext.Session.GetInt32("UserId").Value);


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
                ViewData["CompanyName"] = companyName;
                ViewBag.Course = course;
                ViewBag.Questions = questions;

                return View("~/Views/Worker/Cources/PassageCource.cshtml");
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync(Logger.LogType.Error, $"Помилка під час проходження курсу з ID {courseId}.", HttpContext.Session.GetInt32("UserId").Value);


                return StatusCode(500, "Сталася помилка.");
            }
        }

        [HttpPost]
        [Route("PassageCource")]
        [UserExists]
        public async Task<IActionResult> SaveResultCourse(UserResultRequest userResultRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                   
                    return BadRequest(ModelState);
                }

                var course = await _coursesService.GetCoursesById(userResultRequest.CourseId);
                if (course == null)
                {
                   
                    return NotFound();
                }

                int maxRating = userResultRequest.Questions.Select(a=>a.Answers.Where(an=>an.IsCorrectAnswer)).Count();
                int userRating = userResultRequest.Questions.Select(n => n.Answers.Where(b => b.IsSelected == true && b.IsCorrectAnswer == true)).Count();





                User user = await _userService.GetUser(HttpContext.Session.GetInt32("UserId").Value);
                if (user == null)
                {
                    
                    return RedirectToAction("Login", "Login");
                }
                userResultRequest.Questions = await AzureOperation.UploadFileToBlobAsync(userResultRequest.Questions);
                string userAnswersJson = JsonConvert.SerializeObject(userResultRequest.Questions);


                var userResult = new UserResults
                {
                    User = user,
                    Course = course,
                    DateCompilation = DateTime.Now,
                    Rating = userRating,
                    MaxRating = maxRating,
                    answerJson = userAnswersJson,

                };

                await _userResultService.AddResult(userResult);
               

                return RedirectToAction("UncompleteCourses");
            }
            catch (Exception ex)
            {
                
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
               // await _loggerService.LogAsync(Logger.LogType.Info, $"Теорія курса з ID {courseId}.", HttpContext.Session.GetInt32("UserId").Value);



                var course = await _coursesService.GetCoursesById(courseId);
                if (course == null)
                {
                    

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
                ViewData["CompanyName"] = companyName;
                ViewBag.Course = course;

                return View("~/Views/Worker/Cources/TheoryCourse.cshtml");
            }
            catch (Exception ex)
            {
               

                return StatusCode(500, "Сталася помилка.");
            }

        }

    }
}

