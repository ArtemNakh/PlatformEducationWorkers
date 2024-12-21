using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;

namespace PlatformEducationWorkers.Core.Services
{
    public class JobTitleService : IJobTitleService
    {
        private readonly IRepository _repository;
        private readonly IEnterpriseService _enterpriseService;
        private readonly IUserService _userService;
        private readonly IUserResultService _userResultService;
        private readonly ICourseService _courseService;


        public JobTitleService(IRepository repository, IEnterpriseService enterpriseService, IUserService userService, IUserResultService userResultService, ICourseService courseService)
        {
            _repository = repository;
            _enterpriseService = enterpriseService;
            _userService = userService;
            _userResultService = userResultService;
            _courseService = courseService;
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
                IEnumerable<User> users = await _userService.GetUsersByJobTitle(idJobTitle);
                foreach (User user in users)
                {
                    //видаляємоо усі результати курсів користувача
                    IEnumerable<UserResults> userResults = await _userResultService.GetAllUserResult(user.Id);


                    foreach (var result in userResults)
                    {
                        await _userResultService.DeleteResult(result.Id);
                    }

                   await _userService.DeleteUser(user.Id);
                }


                // Видаляємо вибрану роль зі списку доступних ролей курса
                IEnumerable<Courses> courses = await _courseService.GetCoursesByJobTitle(idJobTitle);
                foreach (var course in courses)
                {
                    course.AccessRoles.Remove(jobTitle);
                    await _courseService.UpdateCourse(course);
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

        public async Task<IEnumerable<JobTitle>> GetAvaliableRoles(int enterpriseId)
        {
            try
            {
                if (enterpriseId == null || enterpriseId < 0)
                {
                    throw new Exception($"Error get avaliable job Title, error:enterpriseId null or less than 0");
                }
                if(!await _enterpriseService.HasEnterprise(enterpriseId))
                {
                    throw new Exception($"Enterprise with Id {enterpriseId} is not found");
                }


                return (await _repository.GetQueryAsync<JobTitle>(n=>n.Enterprise.Id == enterpriseId)).Skip(1);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get avaliable job Title, error:{ex}");
            }

        }
    }
}
