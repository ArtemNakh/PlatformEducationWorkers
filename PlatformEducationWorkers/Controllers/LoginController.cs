using Amazon.Runtime.Internal;
using Amazon.S3.Model;
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
using PlatformEducationWorkers.Models.Azure;
using PlatformEducationWorkers.Request.AccountRequest;
using PlatformEducationWorkers.Request.Login_RegisterRequest;
using Serilog;
using System.IO;

namespace PlatformEducationWorkers.Controllers
{

    public class LoginController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEnterpriseService _enterpriseService;
        private readonly IJobTitleService _jobTitleService;
        private readonly ICreateEnterpriseService _createEnterpriseService;
        private readonly ILoggerService  _loggingService;
        private readonly AzureBlobAvatarOperation AzureOperation;

        public LoginController(IUserService userService, IEnterpriseService enterpriceService, IJobTitleService jobTitleService, ICreateEnterpriseService createEnterpriseService, ILoggerService loggingService, AzureBlobAvatarOperation azureOperation, EmailService emailService)
        {
            _userService = userService;
            _enterpriseService = enterpriceService;
            _jobTitleService = jobTitleService;
            _createEnterpriseService = createEnterpriseService;
            _loggingService = loggingService;
            AzureOperation = azureOperation;
           // _emailService = emailService;


        }

        [HttpGet]
        [Route("Login")]
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login( LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
               return BadRequest(ModelState);
            }

            try
            {
               
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
                          
                            if (user.ProfileAvatar != null && !string.IsNullOrEmpty(user.ProfileAvatar))
                            {
                               

                                // Декодуємо базу64 зображення в byte[]
                                byte[] avatarBytes = await AzureOperation.UnloadAvatarFromBlobAsync(user.ProfileAvatar); ;
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
                    Log.Information($"login worker  ,user Id :{user.Id}");

                    if (userRole == Role.Admin.ToString())
                    {
                        return RedirectToAction("MainAdmin", "MainAdmin", new { area = "Administrator" });
                    }
                    
                    else if (userRole == Role.Workers.ToString())
                    {
                        return RedirectToAction("MainWorker", "MainWorker", new { area = "Worker" });
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
         
            return View();
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register( RegisterCompanyRequest model)
        {
            if (!ModelState.IsValid)
            {
              
                return View(model);
            }
            string avatar = "";
            try
            {
              
                var enterprise = new Enterprise
                {
                    Title = model.Title,
                    DateCreate = DateTime.UtcNow,
                    Email = model.Email,
                    Users = new List<User>(),
                    Courses = new List<Courses>(),
                    PasswordEmail=model.PasswordEmail,

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
                        photoAvatar=await AzureOperation.UploadAvatarToBlobAsync(fileBytes);
                        avatar = photoAvatar;
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

              
                return RedirectToAction("Login", "Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await AzureOperation.DeleteAvatarFromBlobAsync(avatar);
                return View(model);
            }
        }




    }
}
