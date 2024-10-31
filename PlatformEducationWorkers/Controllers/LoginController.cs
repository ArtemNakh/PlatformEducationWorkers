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
        public async Task<IActionResult> Login( LoginRequest request/*string login, string password*/)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.Login(request.Login, request.Password);
            if (user != null)
            {
                // Якщо логін успішний, зберігаємо логін у сесію
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserRole", user.Role.ToString());
                HttpContext.Session.SetString("EnterpriceId", user.Enterprise.Id.ToString());


                string userRole = HttpContext.Session.GetString("UserRole");

                if (userRole == "Admin")
                {
                    return View("~/Views/Administrator/Main/Index.cshtml");// return RedirectToAction("Index", "Main","Admin");
                }
                else if (userRole == "Worker")
                {
                    return View("~/Views/Worker/Main/Index.cshtml");  // return RedirectToAction("Index", "Main","Worker");
                }
                else
                {
                    TempData["Error"] = "Invalid login or password"; // Використовуємо TempData для передачі помилки
                    return RedirectToAction("Login", "Login");
                }
            }
            else
            {
                TempData["Error"] = "Invalid login or password"; // Використовуємо TempData для передачі помилки
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
        public async Task<IActionResult> Register( RegisterCompanyRequest model/*RegisterCompanyViewModel model*/)
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


                var newEnterprice = await _enterpriceService.AddingEnterprice(enterprice);

                var jobTitle = new JobTitle
                {
                    Name = "Власник",
                    Enterprise = newEnterprice
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
                    Enterprise = newEnterprice,
                    Role = Role.Admin,
                    JobTitle = newJobTitle
                };


                await _userService.AddUser(user);

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
