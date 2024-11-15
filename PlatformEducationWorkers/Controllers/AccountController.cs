using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Request.AccountRequest;

namespace PlatformEducationWorkers.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Route("Credentials")]
        [UserExists]
        public async Task<IActionResult> Credentials()
        {
            var userId =Convert.ToInt32( HttpContext.Session.GetString("UserId"));
           
            var user = await _userService.GetUser(userId);
            var userRole = HttpContext.Session.GetString("UserRole");
            ViewBag.UserRole = userRole;
            return View(user);
        }

        [HttpGet]
        [Route("EditCredentials")]
        [UserExists]
        public async Task<IActionResult> EditCredentials()
        {
            var userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            var user = await _userService.GetUser(userId);

            if (user == null)
            {
                TempData["Error"] = "Користувача не знайдено.";
                //todo(change path)
                return RedirectToAction("Login", "Login");
            }

            var model = new UpdateUserCredentialsRequest
            {
                NewLogin = user.Login,
                NewPassword = user.Password
            };

            var userRole = HttpContext.Session.GetString("UserRole");
            ViewBag.UserRole = userRole;
            return View(model);
        }

        [HttpPost]
        [Route("EditCredentials")]
        [UserExists]
        public async Task<IActionResult> EditCredentials(UpdateUserCredentialsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            try
            {
                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
                var user = await _userService.GetUser(userId);
                if (user == null)
                {
                    TempData["Error"] = "Користувача не знайдено.";
                    //todoChangePath
                    return RedirectToAction("Login", "Login");
                }

                user.Login = request.NewLogin;
                user.Password = request.NewPassword;

                await _userService.UpdateUser(user);

                TempData["Success"] = "Дані успішно оновлено.";
                return RedirectToAction("Credentials");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
        }

    }
}
