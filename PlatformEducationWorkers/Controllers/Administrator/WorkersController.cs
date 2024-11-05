using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Request;

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
        [UserExists]
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



        [HttpGet]
        [Route("CreateWorker")]
        [UserExists]
        public IActionResult CreateWorker()
        {
            ViewBag.JobTitles = _jobTitleService.GetAllRoles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriceId"))).Result;
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();  

            return View("~/Views/Administrator/Workers/CreateWorker.cshtml");
        }
       

        [HttpPost]
        [UserExists]
        [Route("CreateWorker")]
        public async Task<IActionResult> CreateWorker(CreateUserRequest request)
        {
            if (ModelState.IsValid)
            {
                JobTitle jobTitle = _jobTitleService.GetRole(request.JobTitleId).Result;
                var user = new User
                {
                    Name = request.Name,
                    Surname = request.Surname,
                    Birthday = request.Birthday,
                    Email = request.Email,
                    Password = request.Password,
                    Login = request.Login,
                    JobTitle = jobTitle,
                    Role = request.Role, 
                    DateCreate = DateTime.Now
                };

                await _userService.AddUser(user);
                return RedirectToAction("Index");
            }

            ViewBag.JobTitles = _jobTitleService.GetAllRoles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriceId"))).Result;
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

            return View("~/Views/Administrator/Workers/CreateWorker.cshtml");
        }

        [HttpGet]
        [Route("Detail/{id}")]
        [UserExists]
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
