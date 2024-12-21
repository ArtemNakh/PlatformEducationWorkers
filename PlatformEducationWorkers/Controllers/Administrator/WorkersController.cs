using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Core;
using PlatformEducationWorkers.Core.Azure;
using PlatformEducationWorkers.Request.AccountRequest;
using Serilog;


namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Area("Administrator")]
    public class WorkersController : Controller
    {
        private readonly IJobTitleService _jobTitleService;
        private readonly IUserService _userService;
        private readonly IEnterpriseService  _enterpriseService;

        public WorkersController(IUserService userService, IJobTitleService jobTitleService, IEnterpriseService enterpriceService)
        {
            _userService = userService;
            _jobTitleService = jobTitleService;
            _enterpriseService = enterpriceService;
        }

        [HttpGet]
        [Route("Workers")]
        [UserExists]
        public async Task<IActionResult> Workers()
        {

            Log.Information($"open the page Workers");
            try
            {
                var enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;

               

                var users = await _userService.GetAllUsersEnterprise(enterpriseId);
              var JobTitles=await _jobTitleService.GetAllJobTitles(enterpriseId);

                var companyName = (await  _enterpriseService.GetEnterprise(enterpriseId)).Title;
                byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
                if (avatarBytes != null && avatarBytes.Length > 0)
                {
                    string base64Avatar = Convert.ToBase64String(avatarBytes);
                    ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
                }
                else
                {
                    ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
                }
                ViewData["CompanyName"] = companyName;
                ViewBag.Users = users?.ToList();
                ViewBag.JobTitles = JobTitles.ToList();
                return View("~/Views/Administrator/Workers/Workers.cshtml");
            }
            catch (Exception ex)
            {
                Log.Error($"workers enterprise ,error:{ex}");
                TempData["ErrorMessage"] = "An error occurred while loading workers.";
                return RedirectToAction("Login", "Login");
            }
        }

        [HttpGet]
        [Route("SearchWorkers")]
        [UserExists]
        public async Task<IActionResult> SearchWorkers(string name, string jobTitle)
        {

            Log.Information($"Find workers for name and/or jobTitle");
            try
            {
                var enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                var companyName = (await _enterpriseService.GetEnterprise(enterpriseId)).Title;
               
                // Отримуємо всіх користувачів для підприємства
                var users = await _userService.GetAllUsersEnterprise(enterpriseId);

                var JobTitles = await _jobTitleService.GetAllJobTitles(enterpriseId);

                // Якщо є фільтри, застосовуємо їх
                if (!string.IsNullOrEmpty(name))
                {
                    users = users.Where(u => u.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(jobTitle))
                {
                    users = users.Where(n => n.JobTitle.Name == jobTitle).AsQueryable(); 
                }
                byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
                if (avatarBytes != null && avatarBytes.Length > 0)
                {
                    string base64Avatar = Convert.ToBase64String(avatarBytes);
                    ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
                }
                else
                {
                    ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
                }
                ViewData["CompanyName"] = companyName;
                ViewBag.Users = users;
                ViewBag.JobTitles = JobTitles.ToList();

                return View("~/Views/Administrator/Workers/Workers.cshtml");
            }
            catch (Exception ex)
            {
                Log.Error($"search workers enterprise, error:{ex}");
                TempData["ErrorMessage"] = "An error occurred while loading workers.";
                return RedirectToAction("Workers");
            }
        }


        [HttpGet]
        [Route("CreateWorker")]
        [UserExists]
        public async  Task<IActionResult> CreateWorker()
        {

            Log.Information($"open the page CreateWorkers");
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await  _enterpriseService.GetEnterprise(enterpriseId)).Title;
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
            else
            {
                ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
            }
            ViewData["CompanyName"] = companyName;

            ViewBag.JobTitles = await _jobTitleService.GetAllJobTitles(enterpriseId);
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

            return View("~/Views/Administrator/Workers/CreateWorker.cshtml");
        }


        [HttpPost]
        [UserExists]
        [Route("CreateWorker")]
        public async Task<IActionResult> CreateWorker(CreateUserRequest request)
        {

            Log.Information($"Post request Create Worker");
            if (ModelState.IsValid)
            {
                try
                {
                    JobTitle jobTitle = await _jobTitleService.GetJobTitle(request.JobTitleId);
                    Enterprise enterprice = await  _enterpriseService.GetEnterprise(HttpContext.Session.GetInt32("EnterpriseId").Value);
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

                    if (request.ProfileAvatar != null && request.ProfileAvatar.Length > 0)
                    {
                        // Конвертуємо аватарку у Base64
                        using (var memoryStream = new MemoryStream())
                        {
                            await request.ProfileAvatar.CopyToAsync(memoryStream);
                            byte[] fileBytes = memoryStream.ToArray();
                            user.ProfileAvatar= Convert.ToBase64String(fileBytes);
                        }
                    }
                    await _userService.AddUser(user);

                    Log.Information($"workers was succesfully created");

                    return RedirectToAction("Workers");
                }
                catch (Exception ex)
                {
                    Log.Error($"create worker enterprise , error:{ex}");
                    return BadRequest(ex.Message);
                }
            }


            Log.Warning($"create worker  ,request is not valid ,request:{request}");

            ViewBag.JobTitles = _jobTitleService.GetAllJobTitles(HttpContext.Session.GetInt32("EnterpriseId").Value).Result;
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

            return View("~/Views/Administrator/Workers/CreateWorker.cshtml");
        }


        [HttpGet]
        [Route("DetailWorker/{id}")]
        [UserExists]
        public async Task<IActionResult> DetailWorker(int id)
        {

            Log.Information($"open the page DetailWorker");

            var user = await _userService.GetUser(id);
            if (user == null)
            {
                Log.Error($"detail worker  ,user is null,find for id :{id}");

                return NotFound();
            }

            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await  _enterpriseService.GetEnterprise(enterpriseId)).Title;
            
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
            else
            {
                ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
            }
            ViewData["CompanyName"] = companyName;
            ViewBag.JobTitles = await _jobTitleService.GetAllJobTitles(enterpriseId);

            return View("~/Views/Administrator/Workers/DetailWorker.cshtml", user);
        }

        [HttpGet]
        [Route("EditWorker/{id}")]
        [UserExists]
        public async Task<IActionResult> EditWorker(int id)
        {

            Log.Information($"open the page editWorker");
            var user = await _userService.GetUser(id);
            if (user == null)
            {
                Log.Error($"edit worker  ,user is null,find for id :{id}");

                return NotFound();
            }

            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            var companyName = (await  _enterpriseService.GetEnterprise(enterpriseId)).Title;

            ViewData["CompanyName"] = companyName;

            // Populate job titles and roles for dropdowns
            ViewBag.JobTitles = (await _jobTitleService.GetAllJobTitles(enterpriseId)).ToList();
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
            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
            else
            {
                ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
            }
            return View("~/Views/Administrator/Workers/EditWorker.cshtml", updateUserRequest);
        }

        [HttpPost]
        [Route("EditWorker/{id}")]
        [UserExists]
        public async Task<IActionResult> EditWorker(int id, UpdateUserRequest request)
        {
            Log.Information($"post reqeust edit workers");
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userService.GetUser(id);
                    if (user == null)
                    {
                        Log.Error($"detail worker  ,user is null,find for id :{id}");

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
                    // Обробка аватарки
                    if (request.ProfileAvatar != null && request.ProfileAvatar.Length > 0)
                    {
                        // Конвертуємо аватарку у Base64
                        using (var memoryStream = new MemoryStream())
                        {
                            await request.ProfileAvatar.CopyToAsync(memoryStream);
                            byte[] fileBytes = memoryStream.ToArray();
                            string base64Image = Convert.ToBase64String(fileBytes);

                            // Збереження Base64 рядка в базу даних
                            user.ProfileAvatar = base64Image;
                        }
                    }

                    await _userService.UpdateUser(user);



                    Log.Information($"workers was successfully update");
                    return RedirectToAction("Workers");
                }
                catch (Exception ex)
                {
                    Log.Error($"edit worker  ,error:{ex}");

                    return BadRequest(ex.Message);
                }
            }
            Log.Warning($"edit worker  ,request is not valid, request{request}");

            ViewBag.JobTitles = await _jobTitleService.GetAllJobTitles(HttpContext.Session.GetInt32("EnterpriseId").Value);
            ViewBag.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
            return View("~/Views/Administrator/Workers/EditWorker.cshtml", request);
        }

        [HttpPost]
        [UserExists]
        [Route("DeleteWorker/{id}")]
        public async Task<IActionResult> DeleteWorker(int id)
        {
            Log.Information($"post request on delete worker with id:({id})");
            if (id == null)
            {
                Log.Error($"delete  worker  ,id is null");


                return NotFound();
            }

            try
            {
                User user = await _userService.GetUser(id);
                int enterpriceId = HttpContext.Session.GetInt32("EnterpriseId").Value;
                Enterprise enterprice = await  _enterpriseService.GetEnterprise(enterpriceId);

                if (user == null)
                {
                    Log.Error($"delete worker  ,user is null,find for id :{id}");

                    return NotFound();
                }

                if (user.Id == enterprice.Owner.Id)
                {
                    Log.Error($"edit worker  ,user is a owner enterprise,find for id :{id}");


                    return BadRequest("Delete worker impossible because he is the owner.");
                }

                await _userService.DeleteUser(id);

                Log.Information($"worker with id:({id}) was succesfully deleted");

                return RedirectToAction("Workers");
            }
            catch (Exception ex)
            {
                Log.Error($"delete worker  ,error :{ex}");
                return RedirectToAction("Workers");
            }
        }

    }
}
