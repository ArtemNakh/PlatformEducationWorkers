
using PlatformEducationWorkers.Core.Models;

namespace PlatformEducationWorkers.Core.Interfaces
{
    public interface ICourseService
    {
        Task<Courses> AddCourse(Courses courses);
        Task<Courses> UpdateCourse(Courses courses);
        Task DeleteCourse(int courseId);
        Task<IEnumerable<Courses>> GetAllCoursesEnterprise(int enterpriseId);
        Task<IEnumerable<Courses>> GetAllCoursesUser(int userId);
        Task<Courses> GetCoursesById(int courseId);
        Task<IEnumerable<Courses>> GetCoursesByJobTitle(int jobTitleId);
        Task<IEnumerable<Courses>> GetUncompletedCoursesForUser(int userId, int enterpriseId);
        Task<IEnumerable<Courses>> GetNewCourses(int enterpriseId);
    }
}
