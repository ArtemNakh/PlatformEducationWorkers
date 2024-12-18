using Amazon.Runtime.Internal;
using Azure;
using Azure.Core;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Models.Azure;
using PlatformEducationWorkers.Models.Questions;
using PlatformEducationWorkers.Models.Results;
using PlatformEducationWorkers.Request.CourceRequest;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Area("Administrator")]
    public class CoursesController : Controller
    {
        private readonly ICoursesService _courseService;
        private readonly IUserResultService _userResultService;
        private readonly IJobTitleService _jobTitleService;
        private readonly IEnterpriseService _enterpriseService;
        private readonly IUserService _userService;
        private readonly ILoggerService _loggingService;
        private readonly AzureBlobCourseOperation AzureOperation;


        public CoursesController(ICoursesService courceService, IJobTitleService jobTitleService, IUserResultService userResultService, IEnterpriseService enterpriceService, ILogger<CoursesController> logger, IUserService userService, ILoggerService loggingService, IConfiguration configuration, AzureBlobCourseOperation azureOperation)
        {
            _courseService = courceService;
            _jobTitleService = jobTitleService;
            _userResultService = userResultService;
            _enterpriseService = enterpriceService;
            _userService = userService;
            _loggingService = loggingService;
            AzureOperation = azureOperation;
        }

        [HttpGet]
        [Route("Courses")]
        [UserExists]
        public async Task<IActionResult> Courses()
        {
            var cources = await _courseService.GetAllCoursesEnterprise(HttpContext.Session.GetInt32("EnterpriseId").Value);
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
            // Отримуємо аватарку з сесії (якщо вона є)
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
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
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
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


            Enterprise enterprice = await _enterpriseService.GetEnterprise(HttpContext.Session.GetInt32("EnterpriseId").Value);

            request.Questions = await AzureOperation.UploadFileToBlobAsync(request.Questions);

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
                ShowSelectedAnswers = request.ShowUserAnswers,
                ShowContextPassage = request.ShowContextPassage,
            };

            await _courseService.AddCourse(newCource);
            // await _loggingService.LogAsync(Logger.LogType.Info, $"Cource {request.TitleCource} created successfully", HttpContext.Session.GetInt32("UserId").Value);


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
                questions = await AzureOperation.UnloadFileFromBlobAsync(questions);
            }

            if (!string.IsNullOrEmpty(cource.ContentCourse))
            {
                contentCourse = JsonConvert.DeserializeObject<string>(cource.ContentCourse);
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
            var courses = await _courseService.GetAllCoursesEnterprise(enterpriseId);
            if (historyPassages == null)
            {

                return NotFound();
            }


            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
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
            if (courseId == null)
            {
                // await _loggingService.LogAsync(Logger.LogType.Warning, $"No passages courses  for the course ID: {courseId}", HttpContext.Session.GetInt32("UserId").Value);

                return NotFound();
            }
            var historyPassages = await _userResultService.GetAllResultCourses(courseId.Value);
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var courses = await _courseService.GetAllCoursesEnterprise(enterpriseId);
            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
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


            var cource = await _courseService.GetCoursesById(id);
            if (cource == null)
            {


                return NotFound();
            }


            var courcePassages = await _userResultService.GetAllResultCourses(cource.Id);
            foreach (var courcePassage in courcePassages)
            {
                if (courcePassages != null)
                {
                    List<UserQuestionRequest> answers = JsonConvert.DeserializeObject<List<UserQuestionRequest>>(cource.Questions);
                    await AzureOperation.DeleteFilesFromBlobAsync(answers);
                    await _userResultService.DeleteResult(courcePassage.Id);

                }
            }
            
            List<QuestionContext> questions = JsonConvert.DeserializeObject<List<QuestionContext>>(cource.Questions);
           await AzureOperation.DeleteFilesFromBlobAsync(questions);
            await _courseService.DeleteCourse(cource.Id);


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
                ContentCourse = JsonConvert.DeserializeObject<string>(cource.ContentCourse),
                Questions = JsonConvert.DeserializeObject<List<QuestionContext>>(cource.Questions)
            };
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
            ViewData["CompanyName"] = companyName;

            return View("~/Views/Administrator/Cources/EditCource.cshtml", request);

        }

        [HttpPost]
        [Route("EditCource")]
        [UserExists]
        public async Task<IActionResult> EditCourse(EditCourceRequest request)
        {
            if (!ModelState.IsValid)
            {



                return View(request);
            }

            Courses cource = await _courseService.GetCoursesById(request.Id);
            if (cource == null)
            {


                return NotFound();
            }
            //не видаляються старі фото які були
            request.Questions = await AzureOperation.UploadFileToBlobAsync(request.Questions);

            // Оновлюємо дані курсу
            cource.TitleCource = request.TitleCource;
            cource.Description = request.Description;
            cource.ContentCourse = request.ContentCourse;
            cource.Questions = JsonConvert.SerializeObject(request.Questions);

            await _courseService.UpdateCourse(cource);



            return RedirectToAction("DetailCource", cource.Id);
        }


        [HttpGet]
        [Route("SearchCourses")]
        [UserExists]
        public async Task<IActionResult> SearchCourses(string searchTerm)
        {
            // Отримати всі курси
            var allCourses = await _courseService.GetAllCoursesEnterprise(HttpContext.Session.GetInt32("EnterpriseId").Value);

            // Якщо пошуковий термін не порожній, виконати фільтрацію
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                allCourses = allCourses.Where(c => c.TitleCource.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
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
                //await _loggingService.LogAsync(Logger.LogType.Info, $"Завантаження деталей курсу з ID {id}.", HttpContext.Session.GetInt32("UserId").Value);



                var courseResult = await _userResultService.SearchUserResult(id);
                if (courseResult == null)
                {


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
                        return BadRequest(ex.Message);
                    }
                }

                if (!string.IsNullOrEmpty(courseResult.answerJson))
                {
                    try
                    {
                        questions = JsonConvert.DeserializeObject<List<UserQuestionRequest>>(courseResult.answerJson);
                        questions= await AzureOperation.UnloadFileFromBlobAsync(questions);
                    }
                    catch (JsonException ex)
                    {
                        
                        return BadRequest(ex.Message);
                    }
                }

                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
                User user = courseResult.User;
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
                ViewBag.USer = user;

                return View("~/Views/Administrator/Cources/DetailPassageCourse.cshtml");
            }
            catch (Exception ex)
            {
              
                return StatusCode(500, "Сталася помилка.");
            }

        }

    }
}