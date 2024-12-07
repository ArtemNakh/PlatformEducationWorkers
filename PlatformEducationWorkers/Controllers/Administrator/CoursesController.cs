using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
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
        private readonly ICoursesService _courseService;
        private readonly IUserResultService _userResultService;
        private readonly IJobTitleService _jobTitleService;
        private readonly IEnterpriseService _enterpriseService;
        private readonly IUserService _userService;

        private readonly ILoggerService _loggingService;

        public CoursesController(ICoursesService courceService, IJobTitleService jobTitleService, IUserResultService userResultService, IEnterpriseService enterpriceService, ILogger<CoursesController> logger, IUserService userService, ILoggerService loggingService)
        {
            _courseService = courceService;
            _jobTitleService = jobTitleService;
            _userResultService = userResultService;
            _enterpriseService = enterpriceService;
            _userService = userService;
            _loggingService = loggingService;
        }

        [HttpGet]
        [Route("Courses")]
        [UserExists]
        public async Task<IActionResult> Courses()
        {
            //await _loggingService.LogAsync(Logger.LogType.Info, $"Accessing Cources Index");

            var cources = await _courseService.GetAllCoursesEnterprise(HttpContext.Session.GetInt32("EnterpriseId").Value);
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
            ViewData["CompanyName"] = companyName;
            ViewBag.Cources = cources.ToList();
            return View("~/Views/Administrator/Cources/Courses.cshtml");
        }


        [HttpGet]
        [Route("Create")]
        [UserExists]
        public async Task<IActionResult> CreateCourse()
        {
            //await _loggingService.LogAsync(Logger.LogType.Info, $"Accessing Create Cource", HttpContext.Session.GetInt32("UserId").Value);
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
            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
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
                //await _loggingService.LogAsync(Logger.LogType.Warning, $"Create Cource failed due to invalid model state", HttpContext.Session.GetInt32("UserId").Value);

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
          //  await _loggingService.LogAsync(Logger.LogType.Info, $"Creating new Cource: {request.TitleCource}", HttpContext.Session.GetInt32("UserId").Value);


            Enterprise enterprice =await _enterpriseService.GetEnterprise(HttpContext.Session.GetInt32("EnterpriseId").Value);

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

            var newCource = new Courses
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

            await _courseService.AddCourse(newCource);
            await _loggingService.LogAsync(Logger.LogType.Info, $"Cource {request.TitleCource} created successfully", HttpContext.Session.GetInt32("UserId").Value);

            
            return RedirectToAction("Courses");
        }


        [HttpGet]
        [Route("Detail/{id}")]
        [UserExists]
        public async Task<IActionResult> DetailCourse(int id)
        {
            //await _loggingService.LogAsync(Logger.LogType.Info, $"Accessing Detail Cource for ID: {id}", HttpContext.Session.GetInt32("UserId").Value);


            var cource = await _courseService.GetCoursesById(id);
            if (cource == null)
            {
               // await _loggingService.LogAsync(Logger.LogType.Warning, $"Cource not found for ID: {id}", HttpContext.Session.GetInt32("UserId").Value);

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
            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
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

            //await _loggingService.LogAsync(Logger.LogType.Info, $"Accessing History Passage for Cource ID: {id}", HttpContext.Session.GetInt32("UserId").Value);

            
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var historyPassages = await _userResultService.GetAllResultEnterprice(enterpriseId);
            var courses=await _courseService.GetAllCoursesEnterprise(enterpriseId);
            if (historyPassages == null)
            {
                await _loggingService.LogAsync(Logger.LogType.Warning, $"No history passages found for Enterprise ID: {enterpriseId}", HttpContext.Session.GetInt32("UserId").Value);

                return NotFound();
            }


            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
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
              // await _loggingService.LogAsync(Logger.LogType.Warning, $"No passages courses  for the course ID: {courseId}", HttpContext.Session.GetInt32("UserId").Value);

                return NotFound();
            }
            var historyPassages = await _userResultService.GetAllResultCourses(courseId.Value);
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var courses = await _courseService.GetAllCoursesEnterprise(enterpriseId);
            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;

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
            
            await _loggingService.LogAsync(Logger.LogType.Info, $"Attempting to delete Cource with ID: {id}", HttpContext.Session.GetInt32("UserId").Value);

            var cource = await _courseService.GetCoursesById(id);
            if (cource == null)
            {
               await _loggingService.LogAsync(Logger.LogType.Warning, $"Cource not found for ID: {id}", HttpContext.Session.GetInt32("UserId").Value);
                

                return NotFound();
            }


            var courcePassages = await _userResultService.SearchUserResult(cource.Id);
            if (courcePassages != null)
            {
                await _userResultService.DeleteResult(courcePassages.Id);
                await _loggingService.LogAsync(Logger.LogType.Warning, $"Deleted user result for Cource ID: {cource.Id}", HttpContext.Session.GetInt32("UserId").Value);

            }
            await _courseService.DeleteCourse(cource.Id);

            await _loggingService.LogAsync(Logger.LogType.Info, $"Cource with ID: {cource.Id} deleted successfully", HttpContext.Session.GetInt32("UserId").Value);

            return RedirectToAction("Courses");

        }







        [HttpGet]
        [Route("EditCource")]
        [UserExists]
        public async Task<IActionResult> EditCourse(int id)
        {

           // await _loggingService.LogAsync(Logger.LogType.Info, $"Accessing Edit Cource for ID: {id}", HttpContext.Session.GetInt32("UserId").Value);

            var cource = await _courseService.GetCoursesById(id);
            if (cource == null)
            {
                await _loggingService.LogAsync(Logger.LogType.Warning, $"Cource not found for ID: {id}", HttpContext.Session.GetInt32("UserId").Value);

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
            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
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
                await _loggingService.LogAsync(Logger.LogType.Warning, $"Edit Cource failed due to invalid model state", HttpContext.Session.GetInt32("UserId").Value);



                return View(request);
            }

            Courses cource =await _courseService.GetCoursesById(request.Id);
            if (cource == null)
            {
                await _loggingService.LogAsync(Logger.LogType.Warning, $"Cource not found for ID: {request.Id}", HttpContext.Session.GetInt32("UserId").Value);


                return NotFound();
            }

            // Оновлюємо дані курсу
            cource.TitleCource = request.TitleCource;
            cource.Description = request.Description;
            cource.ContentCourse = request.ContentCourse;
            cource.Questions = JsonConvert.SerializeObject(request.Questions);

            await _courseService.UpdateCourse(cource); 
            await _loggingService.LogAsync(Logger.LogType.Warning, $"Cource with ID: {cource.Id} updated successfully", HttpContext.Session.GetInt32("UserId").Value);



            return RedirectToAction("DetailCource", cource.Id);
        }


        [HttpGet]
        [Route("SearchCourses")]
        [UserExists]
        public async Task<IActionResult> SearchCourses(string searchTerm)
        {
            // Отримати всі курси
            var allCourses =await _courseService.GetAllCoursesEnterprise(HttpContext.Session.GetInt32("EnterpriseId").Value);

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
                await _loggingService.LogAsync(Logger.LogType.Info, $"Завантаження деталей курсу з ID {id}.", HttpContext.Session.GetInt32("UserId").Value);



                var courseResult = await _userResultService.SearchUserResult(id);
                if (courseResult == null)
                {
                    await _loggingService.LogAsync(Logger.LogType.Warning, $"Курс з ID {id} не знайдено.", HttpContext.Session.GetInt32("UserId").Value);


                    return NotFound();
                }

                // Обробка вмісту курсу
                string content = "";
                List<UserQuestionRequest> questions = new();

                if (!string.IsNullOrEmpty(courseResult.Course.ContentCourse))
                {
                    try
                    {
                        content = JsonConvert.DeserializeObject<string>(courseResult.Course.ContentCourse);
                    }
                    catch (JsonException ex)
                    {
                        await _loggingService.LogAsync(Logger.LogType.Error, $"Помилка десеріалізації ContentCourse для курсу {id}.", HttpContext.Session.GetInt32("UserId").Value);
                        return BadRequest(ex.Message);
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
                        await _loggingService.LogAsync(Logger.LogType.Error, $"Помилка десеріалізації Questions для курсу {id}.", HttpContext.Session.GetInt32("UserId").Value);

                        return BadRequest(ex.Message);
                    }
                }

                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
                User user = courseResult.User;

                ViewData["CompanyName"] = companyName;
                ViewBag.Course = courseResult.Course;
                ViewBag.Result = courseResult;
                ViewBag.Content = content;
                ViewBag.Questions = questions;
                ViewBag.USer = user;

                return View("~/Views/Administrator/Cources/DetailPassageCourse.cshtml");
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync(Logger.LogType.Error, $"Помилка під час завантаження деталей курсу з ID {id}.", HttpContext.Session.GetInt32("UserId").Value);

                return StatusCode(500, "Сталася помилка.");
            }

        }

    }
}