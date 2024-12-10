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
        private readonly IEnterpriseService _enterpriseService;

        private readonly ILoggerService _loggingService;
        public AccountController(IUserService userService, ILogger<AccountController> logger, IEnterpriseService enterpriceService, ILoggerService loggingService)
        {
            _userService = userService;
            _enterpriseService = enterpriceService;
            _loggingService = loggingService;
        }

        [HttpGet]
        [Route("Credentials")]
        [UserExists]
        public async Task<IActionResult> Credentials()
        {
            try
            {
                //await _loggingService.LogAsync(Logger.LogType.Info, $"Fetching user credentials.", HttpContext.Session.GetInt32("UserId").Value);

                var userId = HttpContext.Session.GetInt32("UserId").Value;
                var user = await _userService.GetUser(userId);
                var userRole = HttpContext.Session.GetString("UserRole");

                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
                byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
                if (avatarBytes != null && avatarBytes.Length > 0)
                {
                    string base64Avatar = Convert.ToBase64String(avatarBytes);
                    ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
                }
                ViewData["CompanyName"] = companyName;
                ViewBag.UserRole = userRole;

                //await _loggingService.LogAsync(Logger.LogType.Info, $"Successfully fetched credentials for user ID: {userId}", HttpContext.Session.GetInt32("UserId").Value);


                return View(user);
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync(Logger.LogType.Error, $"Error occurred while fetching user credentials.", HttpContext.Session.GetInt32("UserId").Value);


                TempData["Error"] = "Помилка завантаження даних користувача.";
                return RedirectToAction("Login","Login");
            }
        }

        [HttpGet]
        [Route("EditCredentials")]
        [UserExists]
        public async Task<IActionResult> EditCredentials()
        {
            try
            {
                //await _loggingService.LogAsync(Logger.LogType.Info, $"Editing credentials for user.", HttpContext.Session.GetInt32("UserId").Value);



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
                   // NewLogin = user.Login,
                    //NewPassword = user.Password
                };

                var userRole = HttpContext.Session.GetString("UserRole");
                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
                byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
                if (avatarBytes != null && avatarBytes.Length > 0)
                {
                    string base64Avatar = Convert.ToBase64String(avatarBytes);
                    ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
                }
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
               
                return RedirectToAction("Credentials");
            }

            try
            {
               
                int userId = HttpContext.Session.GetInt32("UserId").Value;
                User user= await _userService.GetUser(userId);

                if (userId == null)
                {
                   // await _loggingService.LogAsync(Logger.LogType.Info, $"User not found during credentials update. User ID: {userId}", HttpContext.Session.GetInt32("UserId").Value);
                                        
                    TempData["Error"] = "Користувача не знайдено.";
                    return RedirectToAction("Login", "Login");
                }
                if (request.NewLogin != null)
                {
                    user.Login = request.NewLogin;
                }

                if (request.NewPassword != null)
                {
                    user.Password = request.NewPassword;
                }

                // Обробка аватарки
                if (request.ProfileAvatar != null && request.ProfileAvatar.Length > 0)
                {
                    // Конвертуємо аватарку у Base64
                    using (var memoryStream = new MemoryStream())
                    {
                        await request.ProfileAvatar.CopyToAsync(memoryStream);
                        byte[] fileBytes = memoryStream.ToArray();
                        string base64Image = Convert.ToBase64String(fileBytes);

                        // Збереження Base64 рядка в базу даних
                        user.ProfileAvatar = base64Image;
                    }
                }


                await _userService.UpdateUser(user);
                

                TempData["Success"] = "Дані успішно оновлено.";
                return RedirectToAction("Credentials");
            }
            catch (Exception ex)
            {
                
                ModelState.AddModelError(string.Empty, "Помилка оновлення даних.");
                return RedirectToAction("Credentials");
            }
        }
    }
}
