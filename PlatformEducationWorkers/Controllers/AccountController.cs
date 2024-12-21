using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Core.Azure;
using PlatformEducationWorkers.Request.AccountRequest;
using Serilog;

namespace PlatformEducationWorkers.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEnterpriseService _enterpriseService;

        public AccountController(IUserService userService,  IEnterpriseService enterpriceService, ILoggerService loggingService)
        {
            _userService = userService;
            _enterpriseService = enterpriceService;

        }

        [HttpGet]
        [Route("Credentials")]
        [UserExists]
        public async Task<IActionResult> Credentials()
        {
            Log.Information($"open the page Credentials ");

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
                Log.Error($"Error open the main worker page ");


                TempData["Error"] = "Помилка завантаження даних користувача.";
                return RedirectToAction("Login","Login");
            }
        }

        [HttpGet]
        [Route("EditCredentials")]
        [UserExists]
        public async Task<IActionResult> EditCredentials()
        {
            Log.Information($"open the page Edit Credentials ");

            try
            {
                


                var userId = HttpContext.Session.GetInt32("UserId").Value;
                var user = await _userService.GetUser(userId);

                if (user == null)
                {
                    Log.Error($"user is null,userId({userId}) ");


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
                else
                {
                    string defaultAvatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Sourses", "DefaultAvatar.png");
                    byte[] defaultAvatarBytes = System.IO.File.ReadAllBytes(defaultAvatarPath);
                    string base64DefaultAvatar = Convert.ToBase64String(defaultAvatarBytes);
                    ViewData["UserAvatar"] = $"data:image/png;base64,{base64DefaultAvatar}";
                }
                ViewData["CompanyName"] = companyName;
                ViewBag.UserRole = userRole;


                return View();
            }
            catch (Exception ex)
            {
                Log.Error($"Error open the page Edit Credentials ");


                TempData["Error"] = "Помилка завантаження форми редагування.";
                return RedirectToAction("Credentials");
            }
        }

        [HttpPost]
        [Route("EditCredentials")]
        [UserExists]
        public async Task<IActionResult> EditCredentials(UpdateUserCredentialsRequest request)
        {
            Log.Information($"post request the page Edit Credentials ");
            if (!ModelState.IsValid)
            {
                Log.Warning($"models is not valid, request:{request} ");
                return RedirectToAction("Credentials");
            }

            try
            {
               
                int userId = HttpContext.Session.GetInt32("UserId").Value;
                User user= await _userService.GetUser(userId);

                if (userId == null)
                {
                    Log.Error($"userId is null ");
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
                        
                        user.ProfileAvatar = Convert.ToBase64String(memoryStream.ToArray());
                    }
                   
                }


                await _userService.UpdateUser(user);
                Log.Information($"Edit credentials was succesffuly ");

                TempData["Success"] = "Дані успішно оновлено.";
                return RedirectToAction("Credentials");
            }
            catch (Exception ex)
            {
                Log.Error($"post request the page Edit Credentials ");
                ModelState.AddModelError(string.Empty, "Помилка оновлення даних.");
                return RedirectToAction("Credentials");
            }
        }
    }
}
