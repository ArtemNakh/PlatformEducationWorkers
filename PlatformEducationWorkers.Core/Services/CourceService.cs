using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;


namespace PlatformEducationWorkers.Core.Services
{
    public class CourceService : ICourcesService
    {
        private readonly IRepository _repository;

        public CourceService(IRepository repository)
        {
            _repository = repository;
        }

        public Task<Cources> AddCource(Cources cources)
        {
            try
            {
                //додати валідацію
                if (cources == null)
                    throw new Exception("Cource is null");

                return _repository.Add(cources);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error adding cource,  error:{ex}");
            }
        }

        public Task DeleteCource(int courceId)
        {
            try
            {
                //додати валідацію
                if (courceId == null)
                    throw new Exception("Cource is null");

                return _repository.Delete<Cources>(courceId);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error delete cource,  error:{ex}");
            }
        }

        public Task<IEnumerable<Cources>> GetAllCourcesEnterprice(int enterpriceId)
        {
            try
            {
                //додати валідацію
                if (enterpriceId == null)
                    throw new Exception("enterprice is null");

                return _repository.GetQuery<Cources>(u => u.Enterprise.Id == enterpriceId);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get all cource enterprice by enterprice ,  error:{ex}");
            }
        }

        public async Task<IEnumerable<Cources>> GetAllCourcesUser(int userId)
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
                IEnumerable<Cources> cources = await _repository.GetQuery<Cources>(c => c.AccessRoles.Any(ar => ar.Id == user.JobTitle.Id));

                return cources.ToList();
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get all cource enterprice by user ,  error:{ex}");
            }
        }

        public Task<Cources> GetCourcesById(int courceId)
        {
            try
            {
                //додати валідацію
                if (courceId == null)
                    throw new Exception("cource is null");



                return _repository.GetById<Cources>(courceId);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get cource by id ,  error:{ex}");
            }
        }

        public Task<IEnumerable<Cources>> GetCourcesByJobTitle(int jobTitleId, int enterpriceId)
        {
            try
            {
                if (jobTitleId == null)
                    throw new Exception("jobTitleId is null");
                if (enterpriceId == null)
                    throw new Exception("enterpriceId is null");

                JobTitle jobtitile = _repository.GetById<JobTitle>(jobTitleId).Result;

                return _repository.GetQuery<Cources>(n => n.AccessRoles.Contains(jobtitile) && n.Enterprise.Id == enterpriceId);

            }
            catch (Exception ex)
            {

                throw new Exception($"error, get courceByJobTitle, {ex}");
            }
        }

        public Task<Cources> GetCourcesBytitle(int titleCource)
        {
            throw new NotImplementedException();
        }


        public Task<Cources> UpdateCource(Cources cources)
        {
            try
            {
                //додати валідацію
                if (cources == null)
                    throw new Exception("cource is null");

                Cources oldCource = _repository.GetById<Cources>(cources.Id).Result;

                oldCource.TitleCource = cources.TitleCource;
                oldCource.AccessRoles = cources.AccessRoles;
                oldCource.Questions = cources.Questions;
                oldCource.ContentCourse = cources.ContentCourse;
                oldCource.Description = cources.Description;

                return _repository.Update(cources);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get cource by id ,  error:{ex}");
            }
        }



        public async Task<IEnumerable<Cources>> GetUncompletedCourcesForUser(int userId, int enterpriceId)
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
                var allCourses = await _repository.GetQueryAsync<Cources>(u => u.Enterprise.Id == enterpriceId);

                // Асинхронно отримуємо користувача
                var user = await _repository.GetByIdAsync<User>(userId);

                // Список для зберігання непройдених курсів
                List<Cources> uncompletedCourses = new List<Cources>();

                foreach (var course in allCourses)
                {
                    // Перевіряємо, чи курс був пройдений
                    bool courseCompleted = userCourses.Any(uc => uc.Cource.Id == course.Id);

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


    }
}
