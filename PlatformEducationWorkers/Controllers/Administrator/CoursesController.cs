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
using PlatformEducationWorkers.Core;
using PlatformEducationWorkers.Core.Azure;
using PlatformEducationWorkers.Core.Questions;
using PlatformEducationWorkers.Core.Results;
using PlatformEducationWorkers.Request.CourceRequest;
using Serilog;
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



        public CoursesController(ICoursesService courceService, IJobTitleService jobTitleService, IUserResultService userResultService, IEnterpriseService enterpriceService, IUserService userService, IConfiguration configuration)
        {
            _courseService = courceService;
            _jobTitleService = jobTitleService;
            _userResultService = userResultService;
            _enterpriseService = enterpriceService;
            _userService = userService;
        }

        [HttpGet]
        [Route("Courses")]
        [UserExists]
        public async Task<IActionResult> Courses()
        {
            Log.Information($"open the page courses");
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
            else
            {
                ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
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
            Log.Information($"open the page CreateCourse");

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
            else
            {
                ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
            }

            ViewData["CompanyName"] = companyName;
            return View("~/Views/Administrator/Cources/CreateCource.cshtml");
        }

       

        [HttpPost]
        [Route("Create")]
        [UserExists]
        public async Task<IActionResult> CreateCourse(CreateCourceRequest request)
        {
            Log.Information($" Page CreateCourse (Post)");
            if (!ModelState.IsValid)
            {

                var jobTitleslist = await _jobTitleService.GetAllJobTitles(HttpContext.Session.GetInt32("EnterpriseId").Value);
                Log.Warning($"request is not correct{request}");
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


            var newCourse = new Courses
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

            await _courseService.AddCourse(newCourse);
            Log.Information($"Course was seccesfull added{newCourse}");

            return RedirectToAction("Courses");
        }


        [HttpGet]
        [Route("Detail/{id}")]
        [UserExists]
        public async Task<IActionResult> DetailCourse(int id)
        {

            Log.Information($"open the page detail Course");
            var cource = await _courseService.GetCoursesById(id);
            if (cource == null)
            {
                 Log.Error($"Course is null, detail course with id{id}");
                return NotFound();
            }

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

            Log.Information($"open the page historyPassage");
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var historyPassages = await _userResultService.GetAllResultEnterprice(enterpriseId);
            var courses = await _courseService.GetAllCoursesEnterprise(enterpriseId);
            if (historyPassages == null)
            {
                Log.Error($"HistoryPassages is null, history passage course with id{id}");

                return NotFound();
            }


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
            ViewBag.HistoryPassages = historyPassages;
            ViewBag.Courses = courses;
            return View("~/Views/Administrator/Cources/HistoryPassage.cshtml");
        }

        [HttpGet]
        [Route("FindHistoryPassage")]
        [UserExists]
        public async Task<IActionResult> FindHistoryPassage(int? courseId)
        {
            Log.Information($"find history passage on the page history passage");
            if (courseId == null)
            {
                Log.Error($"Find history passages is null, history passage course with id{courseId}");

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
            else
            {
                ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
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

            Log.Information($"post request on delete course with id:({id})");

            var cource = await _courseService.GetCoursesById(id);
            if (cource == null)
            {
                Log.Error($"id course  is null, detail  course with id{id}");
                return NotFound();
            }


            var courcePassages = await _userResultService.GetAllResultCourses(cource.Id);
            foreach (var courcePassage in courcePassages)
            {
                if (courcePassages != null)
                {
                    await _userResultService.DeleteResult(courcePassage.Id);

                }
            }
            

            await _courseService.DeleteCourse(cource.Id);


            Log.Information($"course with id:({id}) was succesfull deleted");
            return RedirectToAction("Courses");

        }







        [HttpGet]
        [Route("EditCource")]
        [UserExists]
        public async Task<IActionResult> EditCourse(int id)
        {
            Log.Information($"open the page edit course");
            var cource = await _courseService.GetCoursesById(id);
            if (cource == null)
            {
                Log.Warning($"id course  is null, edit course with id{id}");
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
            else
            {
                ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
            }
            
            ViewBag.ChooseJobTitle = cource.AccessRoles.ToList();
            ViewBag.AvaliableJobTitle = (await _jobTitleService.GetAvaliableRoles(cource.Enterprise.Id)).ToList();
            ViewData["CompanyName"] = companyName;
            return View("~/Views/Administrator/Cources/EditCource.cshtml", request);

        }

        [HttpPost]
        [Route("EditCource")]
        [UserExists]
        public async Task<IActionResult> EditCourse(EditCourceRequest request)
        {
            Log.Information($"post page CreateCourse");

            //Видалення пустих питань та відповідей 
            List<QuestionContext> questionContext=new List<QuestionContext>();
            foreach (var question in request.Questions)
            {
                
                if(question.Text !=null)
                {
                    QuestionContext questionCurrent = new QuestionContext();
                    questionCurrent.Text = question.Text;
                    if(question.PhotoQuestionBase64!=null)
                    {
                        questionCurrent.PhotoQuestionBase64=question.PhotoQuestionBase64;
                    }

                    foreach (var answer in question.Answers)
                    {
                       
                        if(answer.Text!=null)
                        {
                            AnswerContext answerCurrent = new AnswerContext();
                            answerCurrent.Text = answer.Text;
                            answerCurrent.IsCorrect=answer.IsCorrect;
                            answerCurrent.PhotoAnswerBase64 = answer.PhotoAnswerBase64;
                            
                            questionCurrent.Answers.Add(answerCurrent);
                        }
                    }
                    questionContext.Add(questionCurrent);
                }
            }
            request.Questions = questionContext;







            if (!ModelState.IsValid)
            {
                Log.Warning($"request  is no valid, edit course request{request}");
                return View(request);
            }

            Courses course = await _courseService.GetCoursesById(request.Id);
            if (course == null)
            {
                Log.Error($"course  is null, edit course request{course}");

                return NotFound();
            }

            List<JobTitle> jobTitles = new List<JobTitle>();
            foreach (var IdJobTitle in request.AccessRoles)
            {
                jobTitles.Add(await _jobTitleService.GetJobTitle(IdJobTitle));
            }


            


            // Оновлюємо дані курсу
            course.TitleCource = request.TitleCource;
            course.Description = request.Description;
            course.AccessRoles = jobTitles;
            course.ContentCourse = JsonConvert.SerializeObject(request.ContentCourse);
            course.Questions = JsonConvert.SerializeObject(request.Questions);

            //додати видалення проходжень курсів(так як курс був змінено

            await _courseService.UpdateCourse(course);



            Log.Information($"course with id:({request.Id}) was succesfully update ");
            return RedirectToAction("DetailCourse", new { id = request.Id });


        }


        [HttpGet]
        [Route("SearchCourses")]
        [UserExists]
        public async Task<IActionResult> SearchCourses(string searchTerm)
        {
            Log.Information($"find courses");
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
            else
            {
                ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
            }


            ViewBag.Cources = allCourses;
            return View("~/Views/Administrator/Cources/Courses.cshtml");
        }


        [HttpGet]
        [Route("DetailPassageCourse")]
        [UserExists]
        public async Task<IActionResult> DetailPassageCourse(int id)
        {
            Log.Information($"open the page detail passage course");
            try
            {
               
                var courseResult = await _userResultService.SearchUserResult(id);
                if (courseResult == null)
                {
                    Log.Error($"course result  is null, detail passage course with id course{id}");


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
                        Log.Error($"error deserialize content course with id course{id}");

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
                        Log.Error($"error deserialize answer course with id course result{id}");

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
                else
                {
                    ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
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
                Log.Error($"error detail passage course with id passage course. error: {ex}");

                return StatusCode(500, "Сталася помилка.");
            }

        }

    }
}