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

        public LoginController(IUserService userService, IEnterpriceService enterpriceService)
        {
            _userService = userService;
            _enterpriceService = enterpriceService;
        }

        [HttpGet]
        [Route("Login")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string login, string password)
        {
            // Аутентифікація користувача через IUserService
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
                // Якщо логін неуспішний, повертаємо повідомлення про помилку
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

        [Route("Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterCompanyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var enterprice = new Enterprice
                {
                    Title = model.Title,
                    Email = model.Email,
                    DateCreate = DateTime.Now
                };

                // Додаємо нову фірму
                var newEnterprice = await _enterpriceService.AddingEnterprice(enterprice);

                // Створюємо власника
                var user = new User
                {
                    Name = model.OwnerName,
                    Surname = model.OwnerSurname,
                    Birthday = model.Birthday,
                    Email = model.Email,
                    Password = model.Password,
                    Login = model.Login,
                    DateCreate = DateTime.Now,
                    Enterprise = newEnterprice // Прив’язуємо до фірми
                };

                // Додаємо користувача
              await  _userService.AddUser(user);
               
                return RedirectToAction("Index", "Home"); // Перенаправлення на домашню сторінку
            }

            return View(model); // Якщо валідація не пройшла, повертаємо модель з помилками
        }

    }
}
