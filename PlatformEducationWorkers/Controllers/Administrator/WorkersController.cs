using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Route("Admin")]
    [Area("Administrator")]
    public class WorkersController : Controller
    {
        private readonly IJobTitleService _jobTitleService;
        private readonly IUserService _userService;

        public WorkersController(IUserService userService, IJobTitleService jobTitleService)
        {
            _userService = userService;
            _jobTitleService = jobTitleService;
        }

        [Route("Workers")]
        public async Task<IActionResult> Index()
        {
            //todo тимчасово
            var enterpriceId = HttpContext.Session.GetInt32("EnterpriceId");
            if (enterpriceId == null)
            {
                ViewBag.Users = new List<User>();
                return View("~/Views/Administrator/Workers/Index.cshtml");
            }
            //todo тимчаслвл
            var users = await _userService.GetAllUsersEnterprice(Convert.ToInt32(HttpContext.Session.GetInt32("EnterpriceId"))); 
            ViewBag.Users = users; 
            return View("~/Views/Administrator/Workers/Index.cshtml");
        }


        public IActionResult Create()
        {
            ViewBag.JobTitles = new List<JobTitle>(); //= jobTitleService.GetAllRoles(Convert.ToInt32( HttpContext.Session.GetString("EnterpriceId")));
            ViewBag.Roles = new List<string> { "Admin", "User" }; 
            return View("~/Views/Administrator/Workers/CreateUser.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                user.DateCreate = DateTime.Now; 
                await _userService.AddUser(user);
                return RedirectToAction("Index"); 
            }
            ViewBag.JobTitles = _jobTitleService.GetAllRoles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriceId")));
            ViewBag.Roles = new List<string> { "Admin", "User" };
            return View(user);
        }

        [HttpGet]
        [Route("Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userService.GetUser(id); 
            if (user == null)
            {
                return NotFound(); 
            }

            ViewBag.User = user; 
            return View("~/Views/Administrator/Workers/DetailWorkers.cshtml");
        }

    }
}
