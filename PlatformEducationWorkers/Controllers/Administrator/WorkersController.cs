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
        private readonly IEnterpriceService _enterpriceService;
        public WorkersController(IUserService userService, IJobTitleService jobTitleService, IEnterpriceService enterpriceService)
        {
            _userService = userService;
            _jobTitleService = jobTitleService;
            _enterpriceService = enterpriceService;
        }

        [Route("Workers")]
        [UserExists]
        public async Task<IActionResult> Index()
        {
            var EnterpriseId = Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"));
            if (EnterpriseId == null)
            {
                ViewBag.Users = new List<User>();
                return View("~/Views/Administrator/Workers/Index.cshtml");
            }

            var users = await _userService.GetAllUsersEnterprice(EnterpriseId);

            ViewBag.Users = users?.ToList();

            return View("~/Views/Administrator/Workers/Index.cshtml");
        }



        [HttpGet]
        [Route("CreateWorker")]
        [UserExists]
        public IActionResult CreateWorker()
        {
            ViewBag.JobTitles = _jobTitleService.GetAllRoles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"))).Result;
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
                JobTitle jobTitle = await _jobTitleService.GetRole(request.JobTitleId);
                Enterprice enterprice = await _enterpriceService.GetEnterprice(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId")));
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
                    DateCreate = DateTime.Now,
                    Enterprise = enterprice

                };

                await _userService.AddUser(user);
                return RedirectToAction("Index");
            }

            ViewBag.JobTitles = _jobTitleService.GetAllRoles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"))).Result;
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

            return View("~/Views/Administrator/Workers/CreateWorker.cshtml");
        }


        [HttpGet]
        [Route("DetailWorker/{id}")]
        [UserExists]
        public async Task<IActionResult> DetailWorker(int id)
        {
            var user = await _userService.GetUser(id);
            if (user == null)
            {
                return NotFound();
            }

            // You can populate job titles if needed
            ViewBag.JobTitles = await _jobTitleService.GetAllRoles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId")));

            return View("~/Views/Administrator/Workers/DetailWorker.cshtml", user);
        }

        [HttpGet]
        [Route("EditWorker/{id}")]
        [UserExists]
        public async Task<IActionResult> EditWorker(int id)
        {
            var user = await _userService.GetUser(id);
            if (user == null)
            {
                return NotFound();
            }

            // Populate job titles and roles for dropdowns
            ViewBag.JobTitles = await _jobTitleService.GetAllRoles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId")));
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

            // Map the user data to the UpdateUserRequest model to populate the form
            var updateUserRequest = new UpdateUserRequest
            {
                Name = user.Name,
                Surname = user.Surname,
                Birthday = user.Birthday,
                Email = user.Email,
                JobTitleId = user.JobTitle.Id,
                Role = user.Role
            };

            return View("~/Views/Administrator/Workers/EditWorker.cshtml", updateUserRequest);
        }

        [HttpPost]
        [Route("EditWorker/{id}")]
        [UserExists]
        public async Task<IActionResult> EditWorker(int id, UpdateUserRequest request)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.GetUser(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Update the user details
                user.Name = request.Name;
                user.Surname = request.Surname;
                user.Email = request.Email;
                user.Birthday = request.Birthday;
                user.Role = request.Role;

                // Update the user's job title
                user.JobTitle = await _jobTitleService.GetRole(request.JobTitleId);

                // If the password is provided, update it
                if (!string.IsNullOrEmpty(request.Password))
                {
                    user.Password = request.Password;
                }

                // If the login is provided, update it
                if (!string.IsNullOrEmpty(request.Login))
                {
                    user.Login = request.Login;
                }

                // Save changes
                await _userService.UpdateUser(user);

                // Redirect to the user list page after successful update
                return RedirectToAction("Index");
            }

            // If the model is invalid, reload the form with validation messages
            ViewBag.JobTitles = await _jobTitleService.GetAllRoles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId")));
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
            return View("~/Views/Administrator/Workers/EditWorker.cshtml", request);
        }

        [HttpPost]
        [UserExists]
        [Route("DeleteWorker/{id}")]
        public async Task<IActionResult> DeleteWorker(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            User user = await _userService.GetUser(id);
            int enterpriceId = Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"));
            Enterprice enterprice = await _enterpriceService.GetEnterprice(enterpriceId);
            if (user == null)
            {
                return NotFound();
            }
            if (user.Id == enterprice.Owner.Id)
            {
                return BadRequest("Delete worker impossible because he owner");
            }

            _userService.DeleteUser(id);
            return RedirectToAction("Index");
        }

    }
}
