using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;

namespace PlatformEducationWorkers.Core.Services
{
    public class JobTitleService : IJobTitleService
    {
        private readonly IRepository _repository;

        public JobTitleService(IRepository repository)
        {
            _repository = repository;
        }

        public Task<JobTitle> AddingRole(JobTitle jobTitle)
        {
            try
            {
                if (jobTitle.Enterprise == null
                    || _repository.GetQuery<JobTitle>(r => r.Name == jobTitle.Name
                    && r.Enterprise.Id == jobTitle.Enterprise.Id).Result.Count() > 0)
                {
                    throw new Exception($"Error addingJobTitle, this state already exist ");
                }
                return _repository.Add(jobTitle);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error addingJobTitle, error:{ex}");
            }
        }

        public async Task DeleteJobTitle(int idJobTitle)
        {
            try
            {
                if (idJobTitle == null)
                {
                    throw new Exception($"id jobTitle is null.");
                }

                JobTitle jobTitle = await _repository.GetByIdAsync<JobTitle>(idJobTitle);

                if (jobTitle == null)
                {
                    throw new Exception($"JobTitle with id {idJobTitle} not found.");
                }

                //видалення усіх користувач 
                IEnumerable<User> users = await _repository.GetQuery<User>(r => r.JobTitle.Id == jobTitle.Id);
                foreach (User user in users)
                {
                    //видаляємоо усі результати курсів користувача
                    IEnumerable<UserResults> userResults = await _repository.GetQuery<UserResults>(ur => ur.User.Id == user.Id);

                    foreach (var result in userResults)
                    {
                        await _repository.Delete<UserResults>(result.Id);
                    }

                    await _repository.Delete<User>(user.Id);
                }


                // Видаляємо вибрану роль зі списку доступних ролей курса
                IEnumerable<Courses> courses = await _repository.GetQuery<Courses>(c => c.AccessRoles.Any(jt => jt.Id == jobTitle.Id));
                foreach (var course in courses)
                {
                    course.AccessRoles.Remove(jobTitle);
                    await _repository.Update(course);
                }

                await _repository.Delete<JobTitle>(idJobTitle);
                return;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleted job Title, error:{ex}");
            }
        }

        public Task<IEnumerable<JobTitle>> GetAllJobTitles(int idEnterprice)
        {
            try
            {
                return _repository.GetQuery<JobTitle>(j => j.Enterprise.Id == idEnterprice);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get all job Title enterprice, error:{ex}");
            }
        }

        public Task<JobTitle> GetJobTitle(int idJobTitle)
        {
            try
            {
                return _repository.GetById<JobTitle>(idJobTitle);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get job Title by id, error:{ex}");
            }
        }

        public Task<JobTitle> UpdateJobTitle(JobTitle newJobTitle)
        {
            try
            {
                JobTitle oldJobTitle = _repository.GetById<JobTitle>(newJobTitle.Id).Result;

                oldJobTitle.Name = newJobTitle.Name;
                return _repository.Update(oldJobTitle);

            }
            catch (Exception ex)
            {
                throw new Exception($"Error update job Title, error:{ex}");
            }
        }
    }
}
