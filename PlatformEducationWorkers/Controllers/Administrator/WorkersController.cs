using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Route("Admin")]
    [Area("Administrator")]
    public class WorkersController : Controller
    {
        private readonly JobTitleService jobTitleService;
        private readonly UserService userService;

       
        [Route("Workers")]
        public async Task<IActionResult> Index()
        {
            //todo тимчасово
            var enterpriceId = HttpContext.Session.GetInt32("EnterpriceId");
            if (enterpriceId == null)
            {
                // Можна повернути пусту сторінку або обробити якось інакше
                ViewBag.Users = new List<User>(); // Повертаємо пустий список, якщо EnterpriceId не існує
                return View("~/Views/Administrator/Workers/Index.cshtml");
            }
            //todo тимчаслвл
            var users = await userService.GetAllUsersEnterprice(Convert.ToInt32(HttpContext.Session.GetInt32("EnterpriceId"))); // Припускаємо, що цей метод повертає список користувачів
            ViewBag.Users = users; // Зберігаємо список користувачів у ViewBag
            return View("~/Views/Administrator/Workers/Index.cshtml");
        }


        public IActionResult Create()
        {
            ViewBag.JobTitles = new List<JobTitle>(); //= jobTitleService.GetAllRoles(Convert.ToInt32( HttpContext.Session.GetString("EnterpriceId")));
            ViewBag.Roles = new List<string> { "Admin", "User" }; // ролі
            return View("~/Views/Administrator/Workers/CreateUser.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                user.DateCreate = DateTime.Now; // задаємо дату створення
                await userService.AddUser(user);
                return RedirectToAction("Index"); // перенаправлення після успішного створення
            }
            ViewBag.JobTitles = jobTitleService.GetAllRoles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriceId")));
            ViewBag.Roles = new List<string> { "Admin", "User" };
            return View(user);
        }
    }
}
