using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Request.AccountRequest;

namespace PlatformEducationWorkers.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;
        public AccountController(IUserService userService, ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [Route("Credentials")]
        [UserExists]
        public async Task<IActionResult> Credentials()
        {
            try
            {
                _logger.LogInformation("Fetching user credentials.");

                var userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
                var user = await _userService.GetUser(userId);
                var userRole = HttpContext.Session.GetString("UserRole");

                ViewBag.UserRole = userRole;

                _logger.LogInformation("Successfully fetched credentials for user ID: {UserId}", userId);
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user credentials.");
                TempData["Error"] = "Помилка завантаження даних користувача.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        [Route("EditCredentials")]
        [UserExists]
        public async Task<IActionResult> EditCredentials()
        {
            try
            {
                _logger.LogInformation("Editing credentials for user.");

                var userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
                var user = await _userService.GetUser(userId);

                if (user == null)
                {
                    _logger.LogWarning("User not found. User ID: {UserId}", userId);
                    TempData["Error"] = "Користувача не знайдено.";
                    return RedirectToAction("Login", "Login");
                }

                var model = new UpdateUserCredentialsRequest
                {
                    NewLogin = user.Login,
                    NewPassword = user.Password
                };

                var userRole = HttpContext.Session.GetString("UserRole");
                ViewBag.UserRole = userRole;

                _logger.LogInformation("Successfully loaded credentials for editing. User ID: {UserId}", userId);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading edit credentials form.");
                TempData["Error"] = "Помилка завантаження форми редагування.";
                return RedirectToAction("Credentials");
            }
        }

        [HttpPost]
        [Route("EditCredentials")]
        [UserExists]
        public async Task<IActionResult> EditCredentials(UpdateUserCredentialsRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for updating credentials.");
                return View(request);
            }

            try
            {
                _logger.LogInformation("Updating credentials for user.");

                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
                var user = await _userService.GetUser(userId);

                if (user == null)
                {
                    _logger.LogWarning("User not found during credentials update. User ID: {UserId}", userId);
                    TempData["Error"] = "Користувача не знайдено.";
                    return RedirectToAction("Login", "Login");
                }

                user.Login = request.NewLogin;
                user.Password = request.NewPassword;

                await _userService.UpdateUser(user);

                _logger.LogInformation("Successfully updated credentials for user ID: {UserId}", userId);

                TempData["Success"] = "Дані успішно оновлено.";
                return RedirectToAction("Credentials");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user credentials.");
                ModelState.AddModelError(string.Empty, "Помилка оновлення даних.");
                return View(request);
            }
        }
    }
}
