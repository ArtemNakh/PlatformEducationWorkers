﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
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
        public readonly ILoggerService _loggerService;


        public CourcesController(ICourcesService courcesService, IUserResultService userResultService, IUserService userService,IEnterpriseService enterpriseService, ILoggerService loggerService)
        {
            _courcesService = courcesService;
            _userResultService = userResultService;
            _userService = userService;

            _enterpriseService = enterpriseService;
            _loggerService = loggerService;
        }

        // Метод для показу всіх непройдених курсів
        [HttpGet]
        [Route("Cources")]
        [UserExists]
        public async Task<IActionResult> UncompleteCourses()
        {
            try
            {

                await _loggerService.LogAsync(Logger.LogType.Info, $"Завантаження непройдених курсів.", HttpContext.Session.GetInt32("UserId").Value);

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
                await _loggerService.LogAsync(Logger.LogType.Error, $"Помилка під час завантаження непройдених курсів.", HttpContext.Session.GetInt32("UserId").Value);
               
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
                await _loggerService.LogAsync(Logger.LogType.Info, $"Завантаження статистики курсів.", HttpContext.Session.GetInt32("UserId").Value);



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

                await _loggerService.LogAsync(Logger.LogType.Info, $"Завантаження деталей курсу з ID {id}.", HttpContext.Session.GetInt32("UserId").Value);

                var courseResult = await _userResultService.SearchUserResult(id);//await _courcesService.GetCourcesById(id);
                if (courseResult == null)
                {
                    await _loggerService.LogAsync(Logger.LogType.Warning, $"Курс з ID {id} не знайдено.", HttpContext.Session.GetInt32("UserId").Value);


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
                        await _loggerService.LogAsync(Logger.LogType.Error, $"Помилка десеріалізації ContentCourse для курсу {id}.", HttpContext.Session.GetInt32("UserId").Value);


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
                        await _loggerService.LogAsync(Logger.LogType.Error, $"Помилка десеріалізації Questions для курсу {id}.", HttpContext.Session.GetInt32("UserId").Value);

                        
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
                await _loggerService.LogAsync(Logger.LogType.Error, $"Помилка під час завантаження деталей курсу з ID {id}.", HttpContext.Session.GetInt32("UserId").Value);


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

                await _loggerService.LogAsync(Logger.LogType.Info, $"Початок проходження курсу з ID {courseId}.", HttpContext.Session.GetInt32("UserId").Value);

                var course = await _courcesService.GetCourcesById(courseId);
                if (course == null)
                {
                    await _loggerService.LogAsync(Logger.LogType.Warning, $"Курс з ID {courseId} не знайдено.", HttpContext.Session.GetInt32("UserId").Value);


                    return NotFound();
                }

                List<QuestionContext> questions = new();
                if (!string.IsNullOrEmpty(course.Questions))
                {
                    try
                    {
                        //questions = JsonConvert.DeserializeObject<List<UserQuestionRequest>>(course.Questions);
                        questions = JsonConvert.DeserializeObject<List<QuestionContext>>(course.Questions);
                    }
                    catch (JsonException ex)
                    {
                        await _loggerService.LogAsync(Logger.LogType.Error, $"Помилка десеріалізації питань для курсу {courseId}.", HttpContext.Session.GetInt32("UserId").Value);


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
                await _loggerService.LogAsync(Logger.LogType.Error, $"Помилка під час проходження курсу з ID {courseId}.", HttpContext.Session.GetInt32("UserId").Value);


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
                    await _loggerService.LogAsync(Logger.LogType.Warning, $"Модель UserResultRequest недійсна: {ModelState}", HttpContext.Session.GetInt32("UserId").Value);


                    return BadRequest(ModelState);
                }

                var course = await _courcesService.GetCourcesById(userResultRequest.CourseId);
                if (course == null)
                {
                    await _loggerService.LogAsync(Logger.LogType.Warning, $"Курс з ID {userResultRequest.CourseId} не знайдено.", HttpContext.Session.GetInt32("UserId").Value);

                    return NotFound();
                }

                int maxRating = userResultRequest.Questions.Select(a=>a.Answers.Where(an=>an.IsCorrectAnswer)).Count();
                int userRating = userResultRequest.Questions.Select(n => n.Answers.Where(b => b.IsSelected == true && b.IsCorrectAnswer == true)).Count();





                User user = await _userService.GetUser(HttpContext.Session.GetInt32("UserId").Value);
                if (user == null)
                {
                    await _loggerService.LogAsync(Logger.LogType.Warning, $"Користувача з ID {HttpContext.Session.GetString("UserId")} не знайдено.", HttpContext.Session.GetInt32("UserId").Value);

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
                await _loggerService.LogAsync(Logger.LogType.Info, $"Результат курсу з ID {userResultRequest.CourseId} успішно збережено.", HttpContext.Session.GetInt32("UserId").Value);


                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync(Logger.LogType.Error, $"Помилка під час збереження результату курсу з ID {userResultRequest.CourseId}.", HttpContext.Session.GetInt32("UserId").Value);


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
                await _loggerService.LogAsync(Logger.LogType.Info, $"Теорія курса з ID {courseId}.", HttpContext.Session.GetInt32("UserId").Value);



                var course = await _courcesService.GetCourcesById(courseId);
                if (course == null)
                {
                    await _loggerService.LogAsync(Logger.LogType.Warning, $"Курс з ID {courseId} не знайдено.", HttpContext.Session.GetInt32("UserId").Value);


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
                await _loggerService.LogAsync(Logger.LogType.Error, $"Помилка під час теорії  курса з ID {courseId}.", HttpContext.Session.GetInt32("UserId").Value);


                return StatusCode(500, "Сталася помилка.");
            }

        }

    }
}

