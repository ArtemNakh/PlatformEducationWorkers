using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
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
        private readonly IEnterpriseService _enterpriceService;
        private readonly IJobTitleService _jobTitleService;
        private readonly ICreateEnterpriseService _createEnterpriseService;
        private readonly ILogger<LoginController> _logger;
        public LoginController(IUserService userService, IEnterpriseService enterpriceService, IJobTitleService jobTitleService, ICreateEnterpriseService createEnterpriseService, ILogger<LoginController> logger)
        {
            _userService = userService;
            _enterpriceService = enterpriceService;
            _jobTitleService = jobTitleService;
            _createEnterpriseService = createEnterpriseService;
            _logger = logger;
        }

        [HttpGet]
        [Route("Login")]
        public IActionResult Login()
        {
            _logger.LogInformation("Displaying login page.");
            return View();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login( LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid login request: {Login}", request.Login);
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Attempting to login user: {Login}", request.Login);

                var user = await _userService.Login(request.Login, request.Password);
                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("UserRole", user.Role.ToString());
                    HttpContext.Session.SetInt32("EnterpriseId", user.Enterprise.Id);

                    string userRole = HttpContext.Session.GetString("UserRole");
                    _logger.LogInformation("User {Login} logged in with role: {UserRole}", request.Login, userRole);

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
                        _logger.LogWarning("Invalid role for user: {Login}", request.Login);
                        TempData["Error"] = "Invalid login or password";
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid login attempt for user: {Login}", request.Login);
                    TempData["Error"] = "Invalid login or password";
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login attempt for user: {Login}", request.Login);
                TempData["Error"] = "An error occurred while logging in.";
                return RedirectToAction("Login", "Login");
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            _logger.LogInformation("User logging out.");
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Route("Register")]
        public IActionResult Register()
        {
            _logger.LogInformation("Rendering registration page.");
            return View();
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register( RegisterCompanyRequest model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid registration form submission for enterprise: {EnterpriseTitle}", model.Title);
                return View(model);
            }

            try
            {
                _logger.LogInformation("Registering new enterprise: {EnterpriseTitle} with owner: {OwnerName}", model.Title, model.OwnerName);

                var enterprise = new Enterprice
                {
                    Title = model.Title,
                    DateCreate = DateTime.UtcNow,
                    Email = model.Email,
                    Users = new List<User>(),
                    Cources = new List<Cources>()
                };

                var owner = new User
                {
                    Name = model.OwnerName,
                    Surname = model.OwnerSurname,
                    Birthday = model.Birthday,
                    DateCreate = DateTime.UtcNow,
                    Email = model.Email,
                    Password = model.Password,
                    Login = model.Login,
                    Role = Role.Admin
                };

                await _createEnterpriseService.AddEnterpriseWithOwnerAsync(enterprise, "Owner", owner);

                _logger.LogInformation("Enterprise {EnterpriseTitle} successfully registered.", model.Title);
                return RedirectToAction("Login", "Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering new enterprise: {EnterpriseTitle}", model.Title);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }




    }
}
