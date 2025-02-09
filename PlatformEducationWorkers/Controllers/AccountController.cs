using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Request.AccountRequest;
using Serilog;
using PlatformEducationWorkers.Core;

namespace PlatformEducationWorkers.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEnterpriseService _enterpriseService;
        public AccountController(IUserService userService,  IEnterpriseService enterpriceService)
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
                else
                {
                    ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
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
                ViewBag.ErrorMessage = "Помилка введення даних.Введіть інші дані";
                return View();
            }

            try
            {
               
                int userId = HttpContext.Session.GetInt32("UserId").Value;
                Role userRole;
                
                if(Role.Admin.ToString()==HttpContext.Session.GetString("UserRole"))
                {
                    userRole=Role.Admin;
                }
                else
                {
                    userRole=Role.Workers;
                }

                User user=new User();
                user.Id= userId;
                user.Role = userRole;
                if (!string.IsNullOrEmpty(request.NewPassword) && !string.IsNullOrEmpty(request.NewLogin))
                {
                    user.Password = request.NewPassword;
                    user.Login = request.NewLogin;
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
                ViewBag.ErrorMessage = "Помилка оновлення даних.Введіть інші дані";

                return View();
            }
        }
    }
}
