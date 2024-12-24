using Azure.Core;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Core;
using PlatformEducationWorkers.Core.Azure;
using PlatformEducationWorkers.Models.Questions;
using PlatformEducationWorkers.Core.Results;
using Serilog;
using System.Text.Json.Serialization;

namespace PlatformEducationWorkers.Controllers.Administrator
{
    [Area("Administrator")]
    public class MainAdminController : Controller
    {
        private readonly IEnterpriseService  _enterpriseService;
        private readonly IUserService _userService;

        private readonly IUserResultService _userResultService;
        private readonly ICourseService _courseService;


        public MainAdminController(IEnterpriseService enterpriceService, IUserService userService, IUserResultService userResultService, ICourseService courceService)
        {
            _enterpriseService = enterpriceService;
            _userService = userService;

            _userResultService = userResultService;
            _courseService = courceService;

        }

        [HttpPost]
        [Route("DeleteEnterprice")]
        [UserExists]
        public async Task<IActionResult> DeleteEnterprice()
        {

            Log.Information($"post deleting enterprise");
            try
            {

                
                int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
               

                int userId = HttpContext.Session.GetInt32("UserId").Value;
                

                Enterprise enterprice =await _enterpriseService.GetEnterprise(enterpriseId);
                enterprice.Owner = null;
                await _enterpriseService.UpdateEnterprise(enterprice);
                enterprice = await _enterpriseService.GetEnterprise(enterpriseId);

                User user = await _userService.GetUser(userId);

                if (enterprice == null)
                {
                    string errorMessage = $"Enterprise with ID {enterpriseId} not found.";
                    Log.Error($"delete enterprise ,enterprise is null,enterprise id:{enterpriseId}, error:{errorMessage}");

                    throw new Exception(errorMessage);

                }

                if (user == null)
                {
                    string errorMessage = $"User with ID {userId} not found.";
                    Log.Error($"delete enterprise ,user is null,user id:{userId}, error:{errorMessage}");

                    throw new Exception(errorMessage);
                }


                if (enterprice.Owner != null && enterprice.Owner.Id != user.Id)
                {
                    string errorMessage = $"User with ID {userId} cannot delete enterprise because they are not the owner.";
                    Log.Error($"delete enterprise ,enterprise owner is not null and owner is not current user, error:{errorMessage}");


                    throw new Exception(errorMessage);
                }

               


               


               

               

                await  _enterpriseService.DeleteingEnterprise(enterpriseId);

                Log.Information($"deleting enerprise with id({enterpriseId}) was succesfully");

                return RedirectToAction("Login", "Login");
            }
            catch (Exception ex)
            {
                Log.Error($"delete enterprise ,error:{ex}");
                TempData["ErrorMessage"] = $"Error deleting Enterprice: {ex.Message}";
                return RedirectToAction("Main", "Main");
            }
        }

        [Route("MainAdmin")]
        [UserExists]
        public async Task<IActionResult> MainAdmin()
        {

            Log.Information($"open the  Main administration page");
            int enterpriseId = HttpContext.Session.GetInt32("EnterpriseId").Value;
            int numbersLastPassage = 5;
            var lastPassages = await _userResultService.GetLastPassages(enterpriseId, numbersLastPassage);
            var newUsers= await _userService.GetNewUsers(enterpriseId);
            var newCources= await _courseService.GetNewCourses(enterpriseId);
            var AverageRating = await _userResultService.GetAverageRating(enterpriseId);
            var companyName = (await  _enterpriseService.GetEnterprise(enterpriseId)).Title;
           

            byte[] avatarBytes = HttpContext.Session.Get("UserAvatar");

            ViewData["CompanyName"] = companyName; 
            ViewBag.LastPassages = lastPassages;
            ViewBag.NewUsers = newUsers;
            ViewBag.NewCourses = newCources;
            ViewBag.AverageRating = AverageRating;
           
            if (avatarBytes != null && avatarBytes.Length > 0)
            {
                string base64Avatar = Convert.ToBase64String(avatarBytes);
                ViewData["UserAvatar"] = $"data:image/jpeg;base64,{base64Avatar}";
            }
            else
            {
                ViewData["UserAvatar"] = AvatarHelper.GetDefaultAvatar();
            }


            return View("~/Views/Administrator/Main/Main.cshtml");
        }
    }
}
