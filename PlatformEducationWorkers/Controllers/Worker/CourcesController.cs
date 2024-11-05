using Microsoft.AspNetCore.Mvc;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Services;

namespace PlatformEducationWorkers.Controllers.Worker
{
    [Route("Worker")]
    [Area("Worker")]
    public class CourcesController : Controller
    {
        public readonly ICourcesService _courcesService;
        public readonly IUserResultService _userResultService;

        public CourcesController(ICourcesService courcesService, IUserResultService userResultService)
        {
            _courcesService = courcesService;
            _userResultService = userResultService;
        }

        // Метод для показу всіх непройдених курсів
        [Route("Cources")]
        [UserExists]
        public async Task<IActionResult> Index()
        {
            int jobTitleId = Convert.ToInt32(HttpContext.Session.GetInt32("JobTitleId")); 
            int userId = Convert.ToInt32(HttpContext.Session.GetInt32("UserId")); 
            int enterpriceId = Convert.ToInt32(HttpContext.Session.GetInt32("EnterpriceId")); 

            // Отримання курсів для поточного JobTitle
            var allCources = await _courcesService.GetCourcesByJobTitle(jobTitleId,enterpriceId);
            // Отримання результатів користувача по курсах
            var userResults = await _userResultService.GetAllUserResult(userId);

            // Фільтрація непройдених курсів
            var uncompletedCources = allCources.Where(course =>
                !userResults.Any(result => result.Cource.Id == course.Id)).ToList();

            ViewBag.UncompletedCources = uncompletedCources;

            return View("~/Views/Worker/Cources/Index.cshtml");
        }


        [Route("Statistics")]
        [UserExists]
        public async Task<IActionResult> StatisticCources()
        {
            // Отримуємо список курсів і їхню статистику для відображення
            // todo: отримати UserId з сесії
            int userId = Convert.ToInt32(1/*HttpContext.Session.GetInt32("UserId")*/); 

            var coursesStatistics = await _courcesService.GetAllCourcesUser(userId);

            ViewBag.CoursesStatistics = coursesStatistics;

            return View("~/Views/Worker/Cources/Statistics.cshtml");
        }


        [Route("PassageCource")]
        [UserExists]
        public IActionResult PassageCource()
        {
            return View("~/Views/Worker/Cources/PassageCource.cshtml");
        }


        [Route("Detail/{id}")]
        [UserExists]
        public async Task<IActionResult> Detail(int id)
        {
            var courseDetail = await _courcesService.GetCourcesById(id);
            if (courseDetail == null)
            {
                return NotFound();
            }
            return View("~/Views/Worker/Cources/Detail.cshtml", courseDetail);
        }

    }
}
