using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Request.AccountRequest;

namespace PlatformEducationWorkers.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEnterpriseService _enterpriceService;

        private readonly ILoggerService _loggingService;
        public AccountController(IUserService userService, ILogger<AccountController> logger, IEnterpriseService enterpriceService, ILoggerService loggingService)
        {
            _userService = userService;
            _enterpriceService = enterpriceService;
            _loggingService = loggingService;
        }

        [HttpGet]
        [Route("Credentials")]
        [UserExists]
        public async Task<IActionResult> Credentials()
        {
            try
            {
                await _loggingService.LogAsync(Logger.LogType.Info, $"Fetching user credentials.", HttpContext.Session.GetInt32("UserId").Value);

                var userId = HttpContext.Session.GetInt32("UserId").Value;
                var user = await _userService.GetUser(userId);
                var userRole = HttpContext.Session.GetString("UserRole");

                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                var companyName = (await _enterpriceService.GetEnterprice(enterpriseId)).Title;
                ViewData["CompanyName"] = companyName;
                ViewBag.UserRole = userRole;

                await _loggingService.LogAsync(Logger.LogType.Info, $"Successfully fetched credentials for user ID: {userId}", HttpContext.Session.GetInt32("UserId").Value);


                return View(user);
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync(Logger.LogType.Error, $"Error occurred while fetching user credentials.", HttpContext.Session.GetInt32("UserId").Value);


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
                await _loggingService.LogAsync(Logger.LogType.Info, $"Editing credentials for user.", HttpContext.Session.GetInt32("UserId").Value);



                var userId = HttpContext.Session.GetInt32("UserId").Value;
                var user = await _userService.GetUser(userId);

                if (user == null)
                {
                    await _loggingService.LogAsync(Logger.LogType.Warning, $"User not found. User ID: {userId}", HttpContext.Session.GetInt32("UserId").Value);


                    TempData["Error"] = "Користувача не знайдено.";
                    return RedirectToAction("Login", "Login");
                }

                var model = new UpdateUserCredentialsRequest
                {
                    NewLogin = user.Login,
                    NewPassword = user.Password
                };

                var userRole = HttpContext.Session.GetString("UserRole");
                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                var companyName = (await _enterpriceService.GetEnterprice(enterpriseId)).Title;

                ViewData["CompanyName"] = companyName;
                ViewBag.UserRole = userRole;
                await _loggingService.LogAsync(Logger.LogType.Info, $"Successfully loaded credentials for editing. User ID: {userId}", HttpContext.Session.GetInt32("UserId").Value);


                return View(model);
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync(Logger.LogType.Info, $"Error occurred while loading edit credentials form.", HttpContext.Session.GetInt32("UserId").Value);


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
                await _loggingService.LogAsync(Logger.LogType.Warning, $"Invalid model state for updating credentials.", HttpContext.Session.GetInt32("UserId").Value);

                return View(request);
            }

            try
            {
                await _loggingService.LogAsync(Logger.LogType.Info, $"Updating credentials for user.", HttpContext.Session.GetInt32("UserId").Value);

                int userId = HttpContext.Session.GetInt32("UserId").Value;
                var user = await _userService.GetUser(userId);

                if (user == null)
                {
                    await _loggingService.LogAsync(Logger.LogType.Info, $"User not found during credentials update. User ID: {userId}", HttpContext.Session.GetInt32("UserId").Value);
                                        
                    TempData["Error"] = "Користувача не знайдено.";
                    return RedirectToAction("Login", "Login");
                }

                user.Login = request.NewLogin;
                user.Password = request.NewPassword;

                await _userService.UpdateUser(user);
                await _loggingService.LogAsync(Logger.LogType.Info, $"Successfully updated credentials for user ID: {userId}", HttpContext.Session.GetInt32("UserId").Value);


                TempData["Success"] = "Дані успішно оновлено.";
                return RedirectToAction("Credentials");
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync(Logger.LogType.Error, $"Error occurred while updating user credentials.", HttpContext.Session.GetInt32("UserId").Value);

                ModelState.AddModelError(string.Empty, "Помилка оновлення даних.");
                return View(request);
            }
        }
    }
}
