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
        private readonly ILogger<WorkersController> _logger;
        public WorkersController(IUserService userService, IJobTitleService jobTitleService, IEnterpriceService enterpriceService, ILogger<WorkersController> logger)
        {
            _userService = userService;
            _jobTitleService = jobTitleService;
            _enterpriceService = enterpriceService;
            _logger = logger;
        }

        [Route("Workers")]
        [UserExists]
        public async Task<IActionResult> Index()
        {
            try
            {
                var enterpriseId = Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"));
               

                _logger.LogInformation("Fetching all users for enterprise ID {EnterpriseId}.", enterpriseId);
                var users = await _userService.GetAllUsersEnterprice(enterpriseId);

                ViewBag.Users = users?.ToList();
                return View("~/Views/Administrator/Workers/Index.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching workers.");
                TempData["ErrorMessage"] = "An error occurred while loading workers.";
                return RedirectToAction("Index");
            }
        }



        [HttpGet]
        [Route("CreateWorker")]
        [UserExists]
        public IActionResult CreateWorker()
        {
            _logger.LogInformation("Opening 'Create Worker' page.");
            ViewBag.JobTitles = _jobTitleService.GetAllJobTitles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"))).Result;
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
            _logger.LogInformation("open page Create Worker");
            return View("~/Views/Administrator/Workers/CreateWorker.cshtml");
        }


        [HttpPost]
        [UserExists]
        [Route("CreateWorker")]
        public async Task<IActionResult> CreateWorker(CreateUserRequest request)
        {
            _logger.LogInformation("Attempting to create a new worker.");
            if (ModelState.IsValid)
            {
                try
                {
                    JobTitle jobTitle = await _jobTitleService.GetJobTitle(request.JobTitleId);
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
                    _logger.LogInformation("Worker created successfully.");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while creating a worker.");
                }
            }

            _logger.LogWarning("Model state is invalid while creating a worker.");
            ViewBag.JobTitles = _jobTitleService.GetAllJobTitles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"))).Result;
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

            return View("~/Views/Administrator/Workers/CreateWorker.cshtml");
        }


        [HttpGet]
        [Route("DetailWorker/{id}")]
        [UserExists]
        public async Task<IActionResult> DetailWorker(int id)
        {
            _logger.LogInformation("Fetching details for worker ID {Id}.", id);
            var user = await _userService.GetUser(id);
            if (user == null)
            {
                _logger.LogWarning("Worker with ID {Id} not found.", id);
                return NotFound();
            }

            ViewBag.JobTitles = await _jobTitleService.GetAllJobTitles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId")));

            return View("~/Views/Administrator/Workers/DetailWorker.cshtml", user);
        }

        [HttpGet]
        [Route("EditWorker/{id}")]
        [UserExists]
        public async Task<IActionResult> EditWorker(int id)
        {
            _logger.LogInformation("Opening 'Edit Worker' page for ID {Id}.", id);

            var user = await _userService.GetUser(id);
            if (user == null)
            {
                _logger.LogWarning("Worker with ID {Id} not found.", id);

                return NotFound();
            }

            // Populate job titles and roles for dropdowns
            ViewBag.JobTitles = await _jobTitleService.GetAllJobTitles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId")));
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
            _logger.LogInformation("Updating worker with ID {Id}.", id);

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userService.GetUser(id);
                    if (user == null)
                    {
                        _logger.LogWarning("Worker with ID {Id} not found.", id);
                        return NotFound();
                    }

                    user.Name = request.Name;
                    user.Surname = request.Surname;
                    user.Email = request.Email;
                    user.Birthday = request.Birthday;
                    user.Role = request.Role;
                    user.JobTitle = await _jobTitleService.GetJobTitle(request.JobTitleId);

                    if (!string.IsNullOrEmpty(request.Password))
                    {
                        user.Password = request.Password;
                    }

                    if (!string.IsNullOrEmpty(request.Login))
                    {
                        user.Login = request.Login;
                    }

                    await _userService.UpdateUser(user);
                    _logger.LogInformation("Worker with ID {Id} updated successfully.", id);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while updating worker with ID {Id}.", id);
                }
            }

            _logger.LogWarning("Model state is invalid while updating worker with ID {Id}.", id);

            ViewBag.JobTitles = await _jobTitleService.GetAllJobTitles(Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId")));
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
            return View("~/Views/Administrator/Workers/EditWorker.cshtml", request);
        }

        [HttpPost]
        [UserExists]
        [Route("DeleteWorker/{id}")]
        public async Task<IActionResult> DeleteWorker(int id)
        {
            _logger.LogInformation("Attempting to delete worker with ID {Id}.", id);
            if (id == null)
            {
                _logger.LogWarning("Invalid worker ID provided for deletion.");
                return NotFound();
            }

            try
            {
                User user = await _userService.GetUser(id);
                int enterpriceId = Convert.ToInt32(HttpContext.Session.GetString("EnterpriseId"));
                Enterprice enterprice = await _enterpriceService.GetEnterprice(enterpriceId);

                if (user == null)
                {
                    _logger.LogWarning("Worker with ID {Id} not found.", id);
                    return NotFound();
                }

                if (user.Id == enterprice.Owner.Id)
                {
                    _logger.LogWarning("Cannot delete the owner of the enterprise.");
                    return BadRequest("Delete worker impossible because he is the owner.");
                }

                await _userService.DeleteUser(id);
                _logger.LogInformation("Worker with ID {Id} deleted successfully.", id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting worker with ID {Id}.", id);
                return RedirectToAction("Index");
            }
        }

    }
}
