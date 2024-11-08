using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Route("Admin")]
    [Area("Administrator")]
    public class MainController : Controller
    {
        private readonly IEnterpriceService _enterpriceService;
        private readonly IUserService _userService;

        public MainController(IEnterpriceService enterpriceService, IUserService userService)
        {
            _enterpriceService = enterpriceService;
            _userService = userService;
        }

        [HttpPost]
        [Route("DeleteEnterprice")]
        [UserExists]
        public async Task<IActionResult> DeleteEnterprice()
        {
            try
            {
                int enterpriceId = Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"));
                if (enterpriceId == 0)
                {
                    return NotFound();
                }
                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
                if (enterpriceId == 0)
                {
                    return NotFound();
                }
                Enterprice enterprice=await _enterpriceService.GetEnterprice(enterpriceId);
                User user = await _userService.GetUser(userId);

                if (enterprice == null)
                    throw new Exception($"enterprice with id {enterpriceId} not found");

                if (user == null)
                    throw new Exception($"user with id {enterpriceId} not found");

                if (enterprice.Owner != null && enterprice.Owner.Id == user.Id)
                {
                    throw new Exception($"user with id {enterpriceId} can not delete enterprice because he dosn`t owner");
                }

                // Викликаємо сервіс для видалення фірми та її залежних об'єктів
                await _enterpriceService.DeleteingEnterprice(enterpriceId);

                // Перенаправляємо на сторінку зі списком фірм або іншою сторінкою
                return RedirectToAction("Index", "Main");
            }
            catch (Exception ex)
            {
                // Обробка помилки і повернення на попередню сторінку з повідомленням
                TempData["ErrorMessage"] = $"Error deleting Enterprice: {ex.Message}";
                return RedirectToAction("Index", "Main");
            }
        }

        [Route("Main")]
        [UserExists]
        public IActionResult Index()
        {
            return View("~/Views/Administrator/Main/Index.cshtml");
        }
    }
}
