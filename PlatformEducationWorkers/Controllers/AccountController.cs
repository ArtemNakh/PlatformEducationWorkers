using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Models.Azure;
using PlatformEducationWorkers.Request.AccountRequest;

namespace PlatformEducationWorkers.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEnterpriseService _enterpriseService;
        private readonly AzureBlobAvatarOperation AzureOperation;
        private readonly ILoggerService _loggingService;
        public AccountController(IUserService userService, ILogger<AccountController> logger, IEnterpriseService enterpriceService, ILoggerService loggingService, AzureBlobAvatarOperation azureOperation)
        {
            _userService = userService;
            _enterpriseService = enterpriceService;
            _loggingService = loggingService;
            AzureOperation = azureOperation;
        }

        [HttpGet]
        [Route("Credentials")]
        [UserExists]
        public async Task<IActionResult> Credentials()
        {
            try
            {
                
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

               

                return View(user);
            }
            catch (Exception ex)
            {
               

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
                


                var userId = HttpContext.Session.GetInt32("UserId").Value;
                var user = await _userService.GetUser(userId);

                if (user == null)
                {
                    await _loggingService.LogAsync(Logger.LogType.Warning, $"User not found. User ID: {userId}", HttpContext.Session.GetInt32("UserId").Value);


                    TempData["Error"] = "Користувача не знайдено.";
                    return RedirectToAction("Login", "Login");
                }

               

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


                return View();
            }
            catch (Exception ex)
            {
                

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
                        string photo = await AzureOperation.UploadAvatarToBlobAsync(memoryStream.ToArray());
                        user.ProfileAvatar = photo;
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
