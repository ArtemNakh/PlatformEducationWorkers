using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Route("Admin")]
    [Area("Administrator")]
    public class MainController : Controller
    {
        private readonly IEnterpriseService _enterpriceService;
        private readonly IUserService _userService;
        private readonly ILogger<MainController> _logger;
        private readonly IUserResultService _userResultService;
        private readonly ICourcesService _courseService;


        public MainController(IEnterpriseService enterpriceService, IUserService userService, ILogger<MainController> logger, IUserResultService userResultService, ICourcesService courceService)
        {
            _enterpriceService = enterpriceService;
            _userService = userService;
            _logger = logger;
            _userResultService = userResultService;
            _courseService = courceService;
        }

        [HttpPost]
        [Route("DeleteEnterprice")]
        [UserExists]
        public async Task<IActionResult> DeleteEnterprice()
        {
            try
            {
                _logger.LogInformation("DeleteEnterprice action started.");

                int enterpriceId = HttpContext.Session.GetInt32("EnterpriseId").Value;
               

                int userId = HttpContext.Session.GetInt32("UserId").Value;

                _logger.LogInformation($"Fetching enterprise with ID: {enterpriceId}");
                Enterprice enterprice =await _enterpriceService.GetEnterprice(enterpriceId);

                _logger.LogInformation($"Fetching user with ID: {userId}");
                User user = await _userService.GetUser(userId);

                if (enterprice == null)
                {
                    string errorMessage = $"Enterprise with ID {enterpriceId} not found.";
                    _logger.LogError(errorMessage);
                    throw new Exception(errorMessage);

                }

                if (user == null)
                {
                    string errorMessage = $"User with ID {userId} not found.";
                    _logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }


                if (enterprice.Owner != null && enterprice.Owner.Id == user.Id)
                {
                    string errorMessage = $"User with ID {userId} cannot delete enterprise because they are not the owner.";
                    _logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }

                _logger.LogInformation($"Deleting enterprise with ID: {enterpriceId}");
                await _enterpriceService.DeleteingEnterprice(enterpriceId);

                _logger.LogInformation("Enterprise deleted successfully. Redirecting to Index.");
                return RedirectToAction("Index", "Main");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting enterprise.");

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
            var companyName = (await _enterpriceService.GetEnterprice(enterpriseId)).Title; 
            ViewData["CompanyName"] = companyName; 
            ViewBag.LastPassages = lastPassages;
            ViewBag.NewUsers = newUsers;
            ViewBag.NewCourses = newCources;
            ViewBag.AverageRating = AverageRating;

            _logger.LogInformation("Accessed Main Index page.");

            return View("~/Views/Administrator/Main/Index.cshtml");
        }
    }
}
