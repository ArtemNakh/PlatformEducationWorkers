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
using PlatformEducationWorkers.Core;
using PlatformEducationWorkers.Core.Azure;
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
        private readonly AzureBlobAvatarOperation AzureOperation;

        public LoginController(IUserService userService, IEnterpriseService enterpriceService, IJobTitleService jobTitleService, ICreateEnterpriseService createEnterpriseService, AzureBlobAvatarOperation azureOperation, EmailService emailService)
        {
            _userService = userService;
            _enterpriseService = enterpriceService;
            _jobTitleService = jobTitleService;
            _createEnterpriseService = createEnterpriseService;
            AzureOperation = azureOperation;

        }

        [HttpGet]
        [Route("Login")]
        public async Task<IActionResult> Login()
        {
            Log.Information($"Open the page Login");
            return View();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login( LoginRequest request)
        {
            Log.Information($"post request the page Login ");
            if (!ModelState.IsValid)
            {
                Log.Warning($"models is not valid,request:{request} ");
                return BadRequest(ModelState);
            }

            try
            {
               
                var user = await _userService.Login(request.Login, request.Password);
                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("UserRole", user.Role.ToString());
                    HttpContext.Session.SetInt32("EnterpriseId", (int)user.Enterprise.Id);

                    // Якщо є аватарка, перетворити її у byte[] і зберегти в сесії
                    if (!string.IsNullOrEmpty(user.ProfileAvatar))
                    {
                        try
                        {
                          
                            if (user.ProfileAvatar != null && !string.IsNullOrEmpty(user.ProfileAvatar))
                            {
                               

                                // Декодуємо базу64 зображення в byte[]
                                byte[] avatarBytes = Convert.FromBase64String(user.ProfileAvatar);
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
                        Log.Error($"Role for users is not correct,user id:{user.Id} ");
                        TempData["Error"] = "Invalid login or password";
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    Log.Error($"Error login on page Login ");

                    TempData["Error"] = "Invalid login or password";
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error login on page Login ");
                TempData["Error"] = "Invalid login or password";
                return RedirectToAction("Login", "Login");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            Log.Information($"Logout  from account ");
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Route("Register")]
        public async Task<IActionResult> Register()
        {
            Log.Information($"Open the page registration ");
            return View();
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register( RegisterCompanyRequest model)
        {
            Log.Information($"post request the page registration ");

            if (!ModelState.IsValid)
            {
                Log.Warning($"model is not valid,model:{model} ");

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
                        avatar = Convert.ToBase64String(fileBytes); 
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

                Log.Information($"enterprise was succesfully created ");

                return RedirectToAction("Login", "Login");
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding enterprise ");

                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }




    }
}
