using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Models;
using PlatformEducationWorkers.Request;

namespace PlatformEducationWorkers.Controllers
{

    [Route("Login")]
    public class LoginController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEnterpriceService _enterpriceService;
        private readonly IJobTitleService _jobTitleService;

        public LoginController(IUserService userService, IEnterpriceService enterpriceService, IJobTitleService jobTitleService)
        {
            _userService = userService;
            _enterpriceService = enterpriceService;
            _jobTitleService = jobTitleService;
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

                var enterprice = new Enterprice
                {
                    Title = model.Title,
                    Email = model.Email,
                    DateCreate = DateTime.Now
                };


                enterprice = await _enterpriceService.AddingEnterprice(enterprice);

                var jobTitle = new JobTitle
                {
                    Name = "Власник",
                    Enterprise = enterprice
                };

                var newJobTitle = await _jobTitleService.AddingRole(jobTitle);


                var user = new User
                {
                    Name = model.OwnerName,
                    Surname = model.OwnerSurname,
                    Birthday = model.Birthday,
                    Email = model.Email,
                    Password = model.Password,
                    Login = model.Login,
                    DateCreate = DateTime.Now,
                    Enterprise = enterprice,
                    Role = Role.Admin,
                    JobTitle = newJobTitle
                };


                user= await _userService.AddUser(user);
                enterprice = await _enterpriceService.GetEnterpriceByUser(user.Id);

                enterprice.Owner = user;
                await _enterpriceService.UpdateEnterprice(enterprice);

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
