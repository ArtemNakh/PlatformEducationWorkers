using PlatformEducationWorkers.Core.Interfaces;
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
                if(jobTitle.Enterprise==null ||  _repository.GetQuery<JobTitle>(r=>r.Name == jobTitle.Name && r.Enterprise.Id== jobTitle.Enterprise.Id).Result.Count()>0 )
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

        public Task DeleteRole(int idJobTitle)
        {
            try
            {
                return _repository.Delete<JobTitle>(idJobTitle);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleted job Title, error:{ex}");
            }
        }

        public Task<IEnumerable<JobTitle>> GetAllRoles(int idEnterprice)
        {
            try
            {
                return _repository.GetQuery<JobTitle>(j=>j.Enterprise.Id==idEnterprice);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get all job Title enterprice, error:{ex}");
            }
        }

        public Task<JobTitle> GetRole(int idRole)
        {
            try
            {
                return _repository.GetById<JobTitle>(idRole);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get job Title by id, error:{ex}");
            }
        }

        public Task<JobTitle> UpdateRole(JobTitle newJobTitle)
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
