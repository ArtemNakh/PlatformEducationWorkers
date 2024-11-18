using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Route("Admin")]
    [Area("Administrator")]
    public class MainController : Controller
    {
        private readonly IEnterpriceService _enterpriceService;
        private readonly IUserService _userService;
        private readonly ILogger<MainController> _logger;

        public MainController(IEnterpriceService enterpriceService, IUserService userService, ILogger<MainController> logger)
        {
            _enterpriceService = enterpriceService;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost]
        [Route("DeleteEnterprice")]
        [UserExists]
        public async Task<IActionResult> DeleteEnterprice()
        {
            try
            {
                _logger.LogInformation("DeleteEnterprice action started.");

                int enterpriceId = Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"));
               

                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

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
        public IActionResult Index()
        {
            _logger.LogInformation("Accessed Main Index page.");

            return View("~/Views/Administrator/Main/Index.cshtml");
        }
    }
}
