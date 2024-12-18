using Microsoft.AspNetCore.Cors.Infrastructure;
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
using System.Text.Json.Serialization;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Area("Administrator")]
    public class MainAdminController : Controller
    {
        private readonly IEnterpriseService  _enterpriseService;
        private readonly IUserService _userService;

        private readonly IUserResultService _userResultService;
        private readonly ILoggerService _loggerService;
        private readonly ICoursesService _courseService;
        private readonly AzureBlobAvatarOperation _azureAvatarOperation;
        private readonly AzureBlobCourseOperation _azureCoursesOperation;

        public MainAdminController(IEnterpriseService enterpriceService, IUserService userService, IUserResultService userResultService, ICoursesService courceService, ILoggerService loggerService, AzureBlobAvatarOperation azureAvatarOperation, AzureBlobCourseOperation azureZoursesOperation)
        {
            _enterpriseService = enterpriceService;
            _userService = userService;

            _userResultService = userResultService;
            _courseService = courceService;
            _loggerService = loggerService;
            _azureAvatarOperation = azureAvatarOperation;
            _azureCoursesOperation = azureZoursesOperation;
        }

        [HttpPost]
        [Route("DeleteEnterprice")]
        [UserExists]
        public async Task<IActionResult> DeleteEnterprice()
        {
            try
            {

                
                int enterpriceId = HttpContext.Session.GetInt32("EnterpriseId").Value;
               

                int userId = HttpContext.Session.GetInt32("UserId").Value;
                

                Enterprise enterprice =await _enterpriseService.GetEnterprise(enterpriceId);
                enterprice.Owner = null;
                await _enterpriseService.UpdateEnterprise(enterprice);
                enterprice = await _enterpriseService.GetEnterprise(enterpriceId);

                User user = await _userService.GetUser(userId);

                if (enterprice == null)
                {
                    string errorMessage = $"Enterprise with ID {enterpriceId} not found.";
                   
                    throw new Exception(errorMessage);

                }

                if (user == null)
                {
                    string errorMessage = $"User with ID {userId} not found.";
                   
                    throw new Exception(errorMessage);
                }


                if (enterprice.Owner != null && enterprice.Owner.Id != user.Id)
                {
                    string errorMessage = $"User with ID {userId} cannot delete enterprise because they are not the owner.";
                    
                   
                    throw new Exception(errorMessage);
                }

                IEnumerable<Courses> courses = await _courseService.GetAllCoursesEnterprise(enterpriceId);
                foreach (var course in courses)
                {
                    List<QuestionContext> questions = JsonConvert.DeserializeObject<List<QuestionContext>>(course.Questions);
                    await _azureCoursesOperation.DeleteFilesFromBlobAsync(questions);
                }

                IEnumerable<UserResults> coursesRes = await _userResultService.GetAllResultEnterprice(enterpriceId);
                foreach (var resCourse in coursesRes)
                {
                   List< UserQuestionRequest> userAnswerts=JsonConvert.DeserializeObject<List<UserQuestionRequest>>(resCourse.answerJson);
                    await _azureCoursesOperation.DeleteFilesFromBlobAsync(userAnswerts);
                }

                IEnumerable<User> users = await _userService.GetAllUsersEnterprise(enterpriceId);
                foreach (var curUser in users)
                {
                    await _azureAvatarOperation.DeleteAvatarFromBlobAsync(curUser.ProfileAvatar);
                }
               

                await  _enterpriseService.DeleteingEnterprise(enterpriceId);
                

                return RedirectToAction("Login", "Login");
            }
            catch (Exception ex)
            {
               


                // Обробка помилки і повернення на попередню сторінку з повідомленням
                TempData["ErrorMessage"] = $"Error deleting Enterprice: {ex.Message}";
                return RedirectToAction("Main", "Main");
            }
        }

        [Route("Main")]
        [UserExists]
        public async Task<IActionResult> MainAdmin()
        {

            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            int numbersLastPassage = 5;
            var lastPassages = await _userResultService.GetLastPassages(enterpriseId, numbersLastPassage);
            var newUsers= await _userService.GetNewUsers(enterpriseId);
            var newCources= await _courseService.GetNewCourses(enterpriseId);
            var AverageRating = await _userResultService.GetAverageRating(enterpriseId);
            var companyName = (await  _enterpriseService.GetEnterprise(enterpriseId)).Title;
            // Отримуємо аватарку з сесії (якщо вона є)
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");

            ViewData["CompanyName"] = companyName; 
            ViewBag.LastPassages = lastPassages;
            ViewBag.NewUsers = newUsers;
            ViewBag.NewCourses = newCources;
            ViewBag.AverageRating = AverageRating;
           
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
            else
            {
                ViewData["UserAvatar"] = "/images/default-avatar.png";
            }


            return View("~/Views/Administrator/Main/Main.cshtml");
        }
    }
}
