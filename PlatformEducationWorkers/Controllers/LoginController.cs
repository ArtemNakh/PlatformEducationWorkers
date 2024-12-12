using Amazon.Runtime.Internal;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Models;
using PlatformEducationWorkers.Request.AccountRequest;
using PlatformEducationWorkers.Request.Login_RegisterRequest;

namespace PlatformEducationWorkers.Controllers
{

    [Route("Login")]
    public class LoginController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEnterpriseService _enterpriseService;
        private readonly IJobTitleService _jobTitleService;
        private readonly ICreateEnterpriseService _createEnterpriseService;
        private readonly ILoggerService  _loggingService;
        public LoginController(IUserService userService, IEnterpriseService enterpriceService, IJobTitleService jobTitleService, ICreateEnterpriseService createEnterpriseService, ILoggerService loggingService)
        {
            _userService = userService;
            _enterpriseService = enterpriceService;
            _jobTitleService = jobTitleService;
            _createEnterpriseService = createEnterpriseService;
             _loggingService = loggingService;
        }

        [HttpGet]
        [Route("Login")]
        public async Task<IActionResult> Login()
        {
           await  _loggingService.LogAsync(Logger.LogType.Info, "Displaying login page.");
            return View();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login( LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
               await  _loggingService.LogAsync(Logger.LogType.Warning, "Invalid login request: {Login}");
                return BadRequest(ModelState);
            }

            try
            {
               //await  _loggingService.LogAsync(Logger.LogType.Info, $"Attempting to login user: {request.Login}");
               
                var user = await _userService.Login(request.Login, request.Password);
                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("UserRole", user.Role.ToString());
                    HttpContext.Session.SetInt32("EnterpriseId", user.Enterprise.Id);

                    // Якщо є аватарка, перетворити її у byte[] і зберегти в сесії
                    if (!string.IsNullOrEmpty(user.ProfileAvatar))
                    {
                        try
                        {
                            // Десеріалізуємо JSON, що містить аватарку
                            var avatarJson = JsonConvert.DeserializeObject<string>(user.ProfileAvatar);
                            if (avatarJson != null && !string.IsNullOrEmpty(avatarJson))
                            {
                                // Декодуємо базу64 зображення в byte[]
                                byte[] avatarBytes = Convert.FromBase64String(avatarJson);
                                HttpContext.Session.Set("UserAvatar", avatarBytes);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Якщо виникла помилка, можна поставити аватарку за замовчуванням
                            HttpContext.Session.Set("UserAvatar", new byte[0]);
                        }
                    }
                    else
                    {
                        HttpContext.Session.Set("UserAvatar", new byte[0]);
                    }


                    string userRole = HttpContext.Session.GetString("UserRole");

                   //await  _loggingService.LogAsync(Logger.LogType.Info, $"User {request.Login} logged in with role: {userRole}");

                    if (userRole == Role.Admin.ToString())
                    {
                        return RedirectToAction("Main", "Main", new { area = "Administrator" });
                    }
                    else if (userRole == Role.Workers.ToString())
                    {
                        return RedirectToAction("Main", "Main", new { area = "Worker" });
                    }
                    else
                    {
                       await  _loggingService.LogAsync(Logger.LogType.Warning, $"Invalid role for user: {request.Login}");

                        TempData["Error"] = "Invalid login or password";
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                   await  _loggingService.LogAsync(Logger.LogType.Error, $"Invalid login attempt for user: {request.Login}");


                    TempData["Error"] = "Invalid login or password";
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception ex)
            {
               await  _loggingService.LogAsync(Logger.LogType.Error, $"Error occurred during login attempt for user: {request.Login}");

                TempData["Error"] = "Invalid login or password";
                return RedirectToAction("Login", "Login");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
           //await  _loggingService.LogAsync(Logger.LogType.Info, $"User logging out.");

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Route("Register")]
        public async Task<IActionResult> Register()
        {
           await  _loggingService.LogAsync(Logger.LogType.Info, $"Rendering registration page.");

            return View();
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register( RegisterCompanyRequest model)
        {
            if (!ModelState.IsValid)
            {
               await  _loggingService.LogAsync(Logger.LogType.Warning, $"Invalid registration form submission for enterprise: {model.Title}");

                return View(model);
            }

            try
            {
               await  _loggingService.LogAsync(Logger.LogType.Info, $"Registering new enterprise: {model.Title} with owner: {model.OwnerName}");

                var enterprise = new Enterprise
                {
                    Title = model.Title,
                    DateCreate = DateTime.UtcNow,
                    Email = model.Email,
                    Users = new List<User>(),
                    Courses = new List<Courses>()
                };
                string photoAvatar = "";
                // Обробка аватарки
                if (model.ProfileAvatar != null && model.ProfileAvatar.Length > 0)
                {
                    // Конвертуємо аватарку у Base64
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ProfileAvatar.CopyToAsync(memoryStream);
                        byte[] fileBytes = memoryStream.ToArray();
                        string base64Image = Convert.ToBase64String(fileBytes);

                        // Збереження Base64 рядка в базу даних
                        photoAvatar = base64Image;
                    }
                }
                var owner = new User
                {
                    Name = model.OwnerName,
                    Surname = model.OwnerSurname,
                    Birthday = model.Birthday,
                    DateCreate = DateTime.UtcNow,
                    Email = model.Email,
                    Password = model.Password,
                    Login = model.Login,
                    Role = Role.Admin,
                    ProfileAvatar = photoAvatar,
                };

                await _createEnterpriseService.AddEnterpriseWithOwnerAsync(enterprise, "Owner", owner);
               //await  _loggingService.LogAsync(Logger.LogType.Info, $"Enterprise {model.Title} successfully registered.");

                return RedirectToAction("Login", "Login");
            }
            catch (Exception ex)
            {
               await  _loggingService.LogAsync(Logger.LogType.Error, $"Error occurred while registering new enterprise: {model.Title}");

                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }




    }
}
