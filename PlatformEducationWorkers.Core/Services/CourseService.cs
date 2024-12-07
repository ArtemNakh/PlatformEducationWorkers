using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;

using System.Net.Http.Json;


namespace PlatformEducationWorkers.Core.Services
{
    public class CourseService : ICoursesService
    {
        private readonly IRepository _repository;

        public CourseService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<Courses> AddCourse(Courses courses)
        {
            
            try
            {
                //додати валідацію
                if (courses == null)
                    throw new Exception("Cource is null");


                return await _repository.Add(courses);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error adding cource,  error:{ex}");
            }
        }

        public  Task DeleteCourse(int courseId)
        {
            try
            {
                //додати валідацію
                if (courseId == null)
                    throw new Exception("Cource is null");

                return  _repository.Delete<Courses>(courseId);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error delete cource,  error:{ex}");
            }
        }

        public Task<IEnumerable<Courses>> GetAllCoursesEnterprise(int enterpriseId)
        {
            try
            {
                //додати валідацію
                if (enterpriseId == null)
                    throw new Exception("enterprice is null");

                return _repository.GetQuery<Courses>(u => u.Enterprise.Id == enterpriseId);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get all cource enterprice by enterprice ,  error:{ex}");
            }
        }

        public async Task<IEnumerable<Courses>> GetAllCoursesUser(int userId)
        {
            try
            {
                //додати валідацію
                if (userId == null)
                    throw new Exception("user is null");

                User user = await _repository.GetById<User>(userId);
                if (user == null)
                    throw new Exception("User not found");


                // Змінити запит, щоб враховувати, що AccessRoles — це колекція, а JobTitle — одиночний елемент
                IEnumerable<Courses> cources = await _repository.GetQuery<Courses>(c => c.AccessRoles.Any(ar => ar.Id == user.JobTitle.Id));

                return cources.ToList();
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get all cource enterprice by user ,  error:{ex}");
            }
        }

        public async Task<Courses> GetCoursesById(int courseId)
        {
            try
            {
                //додати валідацію
                if (courseId == null)
                    throw new Exception("cource is null");



                return await _repository.GetByIdAsync<Courses>(courseId);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get cource by id ,  error:{ex}");
            }
        }

        public Task<IEnumerable<Courses>> GetCoursesByJobTitle(int jobTitleId, int enterpriseId)
        {
            try
            {
                if (jobTitleId == null)
                    throw new Exception("jobTitleId is null");
                if (enterpriseId == null)
                    throw new Exception("enterpriceId is null");

                JobTitle jobtitile = _repository.GetById<JobTitle>(jobTitleId).Result;

                return _repository.GetQuery<Courses>(n => n.AccessRoles.Contains(jobtitile) && n.Enterprise.Id == enterpriseId);

            }
            catch (Exception ex)
            {

                throw new Exception($"error, get courceByJobTitle, {ex}");
            }
        }

        public Task<Courses> GetCoursesBytitle(int titleCource)
        {
            throw new NotImplementedException();
        }


        public Task<Courses> UpdateCourse(Courses courses)
        {
            try
            {
                //додати валідацію
                if (courses == null)
                    throw new Exception("cource is null");

                Courses oldCource = _repository.GetById<Courses>(courses.Id).Result;

                oldCource.TitleCource = courses.TitleCource;
                oldCource.AccessRoles = courses.AccessRoles;
                oldCource.Questions = courses.Questions;
                oldCource.ContentCourse = courses.ContentCourse;
                oldCource.Description = courses.Description;

                return _repository.Update(courses);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get cource by id ,  error:{ex}");
            }
        }



        public async Task<IEnumerable<Courses>> GetUncompletedCoursesForUser(int userId, int enterpriceId)
        {
            try
            {
                if (userId == null)
                    throw new Exception("UserId is null");
                if (enterpriceId == null)
                    throw new Exception("enterprice is null");

                // Асинхронно отримуємо результати користувача
                var userCourses = await _repository.GetQueryAsync<UserResults>(uc => uc.User.Id == userId);

                // Асинхронно отримуємо всі курси для підприємства
                var allCourses = await _repository.GetQueryAsync<Courses>(u => u.Enterprise.Id == enterpriceId);

                // Асинхронно отримуємо користувача
                var user = await _repository.GetByIdAsync<User>(userId);

                // Список для зберігання непройдених курсів
                List<Courses> uncompletedCourses = new List<Courses>();

                foreach (var course in allCourses)
                {
                    // Перевіряємо, чи курс був пройдений
                    bool courseCompleted = userCourses.Any(uc => uc.Course.Id == course.Id);

                    bool isJobTitleMatch = course.AccessRoles.Any(c => c.Id == user.JobTitle.Id);

                    // Якщо курс підходить по JobTitle і ще не пройдений
                    if (isJobTitleMatch && !courseCompleted)
                    {
                        uncompletedCourses.Add(course);
                    }
                }

                // Повертаємо список непройдених курсів
                return await Task.FromResult(uncompletedCourses.AsEnumerable());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting uncompleted courses for user, error: {ex}");
            }
        }

        public async Task<IEnumerable<Courses>> GetNewCourses(int enterpriseId)
        {
            try
            {
                var cources = await _repository.GetQueryAsync<Courses>(
                    c => c.Enterprise.Id == enterpriseId);
                return cources.OrderByDescending(course => course.DateCreate).Take(5);
               
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving new courses: {ex.Message}", ex);
            }
        }

    }
}
