using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Models;

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
        public async Task<IActionResult> Login(string login, string password)
        {
            var user = await _userService.Login(login, password);
            if (user != null)
            {
                // Якщо логін успішний, зберігаємо логін у сесію
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserRole", user.Role.ToString());
                HttpContext.Session.SetString("EnterpriceId", user.Enterprise.Id.ToString());


                string userRole = HttpContext.Session.GetString("UserRole");

                if (userRole == "Admin")
                {
                    return RedirectToAction("Index", "MainController");
                }
                else if (userRole == "Worker")
                {
                    return RedirectToAction("Index", "MainController");
                }
                else
                {
                    ViewData["Error"] = "Invalid login or password";
                    return View();
                }
            }
            else
            {

                ViewData["Error"] = "Invalid login or password";
                return View();
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
        public async Task<IActionResult> Register(RegisterCompanyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Повернення на сторінку реєстрації з помилками валідації
                return View(model);
            }


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
                JobTitle= newJobTitle
            };


            await _userService.AddUser(user);

            return RedirectToAction("Login", "Login");




        }
    }
}
