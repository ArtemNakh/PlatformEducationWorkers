using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Models;
using PlatformEducationWorkers.Request;
using PlatformEducationWorkers.Request.AccountRequest;

namespace PlatformEducationWorkers.Controllers
{

    [Route("Login")]
    public class LoginController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEnterpriceService _enterpriceService;
        private readonly IJobTitleService _jobTitleService;
        private readonly ICreateEnterpriseService _createEnterpriseService;

        public LoginController(IUserService userService, IEnterpriceService enterpriceService, IJobTitleService jobTitleService, ICreateEnterpriseService createEnterpriseService)
        {
            _userService = userService;
            _enterpriceService = enterpriceService;
            _jobTitleService = jobTitleService;
            _createEnterpriseService = createEnterpriseService;
        }

        [HttpGet]
        [Route("Login")]
        public IActionResult Index()
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

            var user = await _userService.Login(request.Login, request.Password);
            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserRole", user.Role.ToString());
                HttpContext.Session.SetString("EnterpriseId", user.Enterprise.Id.ToString());


                string userRole = HttpContext.Session.GetString("UserRole");

                if (userRole == Role.Admin.ToString())
                {
                    return View("~/Views/Administrator/Main/Index.cshtml");
                }
                else if (userRole == Role.Workers.ToString())
                {
                    return View("~/Views/Worker/Main/Index.cshtml");
                }
                else
                {
                    TempData["Error"] = "Invalid login or password";
                    return RedirectToAction("Login", "Login");
                }
            }
            else
            {
                TempData["Error"] = "Invalid login or password"; 
                return RedirectToAction("Login", "Login");
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("Register")]
        public IActionResult Register()
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

            try
            {
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
                    Name =model.OwnerName,
                    Surname =model.OwnerSurname,
                    Birthday =model.Birthday,
                    DateCreate = DateTime.UtcNow,
                    Email = model.Email,
                    Password =model.Password,
                    Login = model.Login,
                    Role = Role.Admin
                };

                await _createEnterpriseService.AddEnterpriseWithOwnerAsync(enterprise, "Owner", owner);


                return RedirectToAction("Login", "Login");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message); 
                return View(model); 
            }

        }


       

    }
}
