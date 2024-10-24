using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Core.Interfaces;

namespace PlatformEducationWorkers.Controllers
{

    [Route("Login")]
    public class LoginController : Controller
    {
        private readonly IUserService _userService;

        public LoginController(IUserService userService)
        {
            _userService = userService;
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
    }
}
