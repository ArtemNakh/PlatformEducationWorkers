using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Request.AccountRequest;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Route("Admin")]
    [Area("Administrator")]
    public class WorkersController : Controller
    {
        private readonly IJobTitleService _jobTitleService;
        private readonly IUserService _userService;
        private readonly IEnterpriseService _enterpriceService;
        private readonly ILoggerService _loggerService;
        public WorkersController(IUserService userService, IJobTitleService jobTitleService, IEnterpriseService enterpriceService,ILoggerService loggerService)
        {
            _userService = userService;
            _jobTitleService = jobTitleService;
            _enterpriceService = enterpriceService;
            _loggerService = loggerService;
        }

        [HttpGet]
        [Route("Workers")]
        [UserExists]
        public async Task<IActionResult> Workers()
        {
            try
            {
                var enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;

                await _loggerService.LogAsync(Logger.LogType.Info, $"Fetching all users for enterprise ID {enterpriseId}.", HttpContext.Session.GetInt32("UserId").Value);


                var users = await _userService.GetAllUsersEnterprice(enterpriseId);
              var JobTitles=await _jobTitleService.GetAllJobTitles(enterpriseId);

                var companyName = (await _enterpriceService.GetEnterprice(enterpriseId)).Title;
                ViewData["CompanyName"] = companyName;
                ViewBag.Users = users?.ToList();
                ViewBag.JobTitles = JobTitles.ToList();
                return View("~/Views/Administrator/Workers/Workers.cshtml");
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync(Logger.LogType.Error, $"Fetching all users for enterprise ID {"An error occurred while fetching workers."}.", HttpContext.Session.GetInt32("UserId").Value);


                TempData["ErrorMessage"] = "An error occurred while loading workers.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Route("SearchWorkers")]
        [UserExists]
        public async Task<IActionResult> SearchWorkers(string name, string jobTitle)
        {
            try
            {
                var enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;

                await _loggerService.LogAsync(Logger.LogType.Info, $"Fetching all users for enterprise ID {enterpriseId}.", HttpContext.Session.GetInt32("UserId").Value);

                // Отримуємо всіх користувачів для підприємства
                var users = await _userService.GetAllUsersEnterprice(enterpriseId);

                var JobTitles = await _jobTitleService.GetAllJobTitles(enterpriseId);

                // Якщо є фільтри, застосовуємо їх
                if (!string.IsNullOrEmpty(name))
                {
                    users = users.Where(u => u.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(jobTitle))
                {
                    users = users.Where(n=>n.JobTitle.Name== jobTitle);
                }
                var companyName = (await _enterpriceService.GetEnterprice(enterpriseId)).Title;
                ViewData["CompanyName"] = companyName;
                ViewBag.Users = users?.ToList();
                ViewBag.JobTitles = JobTitles.ToList();

                return View("~/Views/Administrator/Workers/Workers.cshtml");
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync(Logger.LogType.Error, $"An error occurred while fetching workers.", HttpContext.Session.GetInt32("UserId").Value);


                TempData["ErrorMessage"] = "An error occurred while loading workers.";
                return RedirectToAction("Index");
            }
        }


        [HttpGet]
        [Route("CreateWorker")]
        [UserExists]
        public async  Task<IActionResult> CreateWorker()
        {
            await _loggerService.LogAsync(Logger.LogType.Info, $"Opening 'Create Worker' page.", HttpContext.Session.GetInt32("UserId").Value);

            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriceService.GetEnterprice(enterpriseId)).Title;

            ViewData["CompanyName"] = companyName;

            ViewBag.JobTitles = await _jobTitleService.GetAllJobTitles(enterpriseId);
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

            await _loggerService.LogAsync(Logger.LogType.Info, $"Open page Create Worker", HttpContext.Session.GetInt32("UserId").Value);

            return View("~/Views/Administrator/Workers/CreateWorker.cshtml");
        }


        [HttpPost]
        [UserExists]
        [Route("CreateWorker")]
        public async Task<IActionResult> CreateWorker(CreateUserRequest request)
        {
            await _loggerService.LogAsync(Logger.LogType.Info, $"Attempting to create a new worker.", HttpContext.Session.GetInt32("UserId").Value);


            if (ModelState.IsValid)
            {
                try
                {
                    JobTitle jobTitle = await _jobTitleService.GetJobTitle(request.JobTitleId);
                    Enterprice enterprice = await _enterpriceService.GetEnterprice(HttpContext.Session.GetInt32("EnterpriseId").Value);
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

                    await _loggerService.LogAsync(Logger.LogType.Info, $"Worker created successfully.", HttpContext.Session.GetInt32("UserId").Value);

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    await _loggerService.LogAsync(Logger.LogType.Error, $"Error while creating a worker.", HttpContext.Session.GetInt32("UserId").Value);
                    return BadRequest(ex.Message);
                }
            }


            await _loggerService.LogAsync(Logger.LogType.Info, $"Model state is invalid while creating a worker.", HttpContext.Session.GetInt32("UserId").Value);

            ViewBag.JobTitles = _jobTitleService.GetAllJobTitles(HttpContext.Session.GetInt32("EnterpriseId").Value).Result;
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

            return View("~/Views/Administrator/Workers/CreateWorker.cshtml");
        }


        [HttpGet]
        [Route("DetailWorker/{id}")]
        [UserExists]
        public async Task<IActionResult> DetailWorker(int id)
        {

            await _loggerService.LogAsync(Logger.LogType.Info, $"Fetching details for worker ID {id}.", HttpContext.Session.GetInt32("UserId").Value);

            var user = await _userService.GetUser(id);
            if (user == null)
            {

                await _loggerService.LogAsync(Logger.LogType.Warning, $"Worker with ID {id} not found.", HttpContext.Session.GetInt32("UserId").Value);

                return NotFound();
            }

            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriceService.GetEnterprice(enterpriseId)).Title;

            ViewData["CompanyName"] = companyName;
            ViewBag.JobTitles = await _jobTitleService.GetAllJobTitles(enterpriseId);

            return View("~/Views/Administrator/Workers/DetailWorker.cshtml", user);
        }

        [HttpGet]
        [Route("EditWorker/{id}")]
        [UserExists]
        public async Task<IActionResult> EditWorker(int id)
        {
            await _loggerService.LogAsync(Logger.LogType.Info, $"Opening 'Edit Worker' page for ID {id}.", HttpContext.Session.GetInt32("UserId").Value);

            var user = await _userService.GetUser(id);
            if (user == null)
            {

                await _loggerService.LogAsync(Logger.LogType.Warning, $"Worker with ID {id} not found.", HttpContext.Session.GetInt32("UserId").Value);

                return NotFound();
            }

            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await _enterpriceService.GetEnterprice(enterpriseId)).Title;

            ViewData["CompanyName"] = companyName;

            // Populate job titles and roles for dropdowns
            ViewBag.JobTitles = await _jobTitleService.GetAllJobTitles(enterpriseId);
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

            await _loggerService.LogAsync(Logger.LogType.Info, $"Updating worker with ID {id}.", HttpContext.Session.GetInt32("UserId").Value);

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userService.GetUser(id);
                    if (user == null)
                    {
                        await _loggerService.LogAsync(Logger.LogType.Warning, $"Worker with ID {id} not found.", HttpContext.Session.GetInt32("UserId").Value);

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
                    await _loggerService.LogAsync(Logger.LogType.Info, $"Worker with ID {id} updated successfully.", HttpContext.Session.GetInt32("UserId").Value);


                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    await _loggerService.LogAsync(Logger.LogType.Error, $"Error while updating worker with ID {id}.", HttpContext.Session.GetInt32("UserId").Value);
                    return BadRequest(ex.Message);
                }
            }

            await _loggerService.LogAsync(Logger.LogType.Warning, $"Model state is invalid while updating worker with ID {id}.", HttpContext.Session.GetInt32("UserId").Value);

            ViewBag.JobTitles = await _jobTitleService.GetAllJobTitles(HttpContext.Session.GetInt32("EnterpriseId").Value);
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
            return View("~/Views/Administrator/Workers/EditWorker.cshtml", request);
        }

        [HttpPost]
        [UserExists]
        [Route("DeleteWorker/{id}")]
        public async Task<IActionResult> DeleteWorker(int id)
        {
            await _loggerService.LogAsync(Logger.LogType.Info, $"Attempting to delete worker with ID {id}.", HttpContext.Session.GetInt32("UserId").Value);


            if (id == null)
            {
                await _loggerService.LogAsync(Logger.LogType.Warning, $"Invalid worker ID provided for deletion.", HttpContext.Session.GetInt32("UserId").Value);


                return NotFound();
            }

            try
            {
                User user = await _userService.GetUser(id);
                int enterpriceId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                Enterprice enterprice = await _enterpriceService.GetEnterprice(enterpriceId);

                if (user == null)
                {
                    await _loggerService.LogAsync(Logger.LogType.Warning, $"Worker with ID {id} not found.", HttpContext.Session.GetInt32("UserId").Value);

                    return NotFound();
                }

                if (user.Id == enterprice.Owner.Id)
                {
                    await _loggerService.LogAsync(Logger.LogType.Warning, $"Cannot delete the owner of the enterprise.", HttpContext.Session.GetInt32("UserId").Value);


                    return BadRequest("Delete worker impossible because he is the owner.");
                }

                await _userService.DeleteUser(id);
                await _loggerService.LogAsync(Logger.LogType.Info, $"Worker with ID {id} deleted successfully.", HttpContext.Session.GetInt32("UserId").Value);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync(Logger.LogType.Error, $"Error while deleting worker with ID { id}.", HttpContext.Session.GetInt32("UserId").Value);


                return RedirectToAction("Index");
            }
        }

    }
}
