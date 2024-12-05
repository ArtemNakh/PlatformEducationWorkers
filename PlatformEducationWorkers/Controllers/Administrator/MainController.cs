using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Route("Admin")]
    [Area("Administrator")]
    public class MainController : Controller
    {
        private readonly IEnterpriseService  _enterpriseService;
        private readonly IUserService _userService;

        private readonly IUserResultService _userResultService;
        private readonly ILoggerService _loggerService;
        private readonly ICourcesService _courseService;


        public MainController(IEnterpriseService enterpriceService, IUserService userService, IUserResultService userResultService, ICourcesService courceService, ILoggerService loggerService)
        {
             _enterpriseService = enterpriceService;
            _userService = userService;
          
            _userResultService = userResultService;
            _courseService = courceService;
            _loggerService = loggerService;
        }

        [HttpPost]
        [Route("DeleteEnterprice")]
        [UserExists]
        public async Task<IActionResult> DeleteEnterprice()
        {
            try
            {

                await _loggerService.LogAsync(Logger.LogType.Info, $"DeleteEnterprice action started.", HttpContext.Session.GetInt32("UserId").Value);

                int enterpriceId = HttpContext.Session.GetInt32("EnterpriseId").Value;
               

                int userId = HttpContext.Session.GetInt32("UserId").Value;
                await _loggerService.LogAsync(Logger.LogType.Info, $"Fetching enterprise with ID: {enterpriceId}", HttpContext.Session.GetInt32("UserId").Value);


                Enterprice enterprice =await  _enterpriseService.GetEnterprice(enterpriceId);
                await _loggerService.LogAsync(Logger.LogType.Info, $"Fetching user with ID: {userId}", HttpContext.Session.GetInt32("UserId").Value);


                User user = await _userService.GetUser(userId);

                if (enterprice == null)
                {
                    string errorMessage = $"Enterprise with ID {enterpriceId} not found.";
                   
                    await _loggerService.LogAsync(Logger.LogType.Error, $"Enterprise with ID {enterpriceId} not found.", HttpContext.Session.GetInt32("UserId").Value);

                    throw new Exception(errorMessage);

                }

                if (user == null)
                {
                    string errorMessage = $"User with ID {userId} not found.";
                    await _loggerService.LogAsync(Logger.LogType.Error, errorMessage, HttpContext.Session.GetInt32("UserId").Value);
                    
                    throw new Exception(errorMessage);
                }


                if (enterprice.Owner != null && enterprice.Owner.Id == user.Id)
                {
                    string errorMessage = $"User with ID {userId} cannot delete enterprise because they are not the owner.";
                    
                    await _loggerService.LogAsync(Logger.LogType.Error, errorMessage, HttpContext.Session.GetInt32("UserId").Value);

                    throw new Exception(errorMessage);
                }
                await _loggerService.LogAsync(Logger.LogType.Info, "Deleting enterprise with ID: {enterpriceId}", HttpContext.Session.GetInt32("UserId").Value);

                await  _enterpriseService.DeleteingEnterprice(enterpriceId);
                await _loggerService.LogAsync(Logger.LogType.Info, "Enterprise deleted successfully. Redirecting to Index.", HttpContext.Session.GetInt32("UserId").Value);


                return RedirectToAction("Index", "Main");
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync(Logger.LogType.Error, "Error occurred while deleting enterprise.", HttpContext.Session.GetInt32("UserId").Value);



                // Обробка помилки і повернення на попередню сторінку з повідомленням
                TempData["ErrorMessage"] = $"Error deleting Enterprice: {ex.Message}";
                return RedirectToAction("Index", "Main");
            }
        }

        [Route("Main")]
        [UserExists]
        public async Task<IActionResult> Index()
        {
            // Отримуємо останні 5 проходжень для підприємства
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var lastPassages = await _userResultService.GetLastPassages(enterpriseId);
            var newUsers= await _userService.GetNewUsers(enterpriseId);
            var newCources= await _courseService.GetNewCourses(enterpriseId);
            var AverageRating = await _userResultService.GetAverageRating(enterpriseId);
            var companyName = (await  _enterpriseService.GetEnterprice(enterpriseId)).Title; 
            ViewData["CompanyName"] = companyName; 
            ViewBag.LastPassages = lastPassages;
            ViewBag.NewUsers = newUsers;
            ViewBag.NewCourses = newCources;
            ViewBag.AverageRating = AverageRating;
            await _loggerService.LogAsync(Logger.LogType.Info, "Accessed Main Index page.", HttpContext.Session.GetInt32("UserId").Value);



            return View("~/Views/Administrator/Main/Index.cshtml");
        }
    }
}
