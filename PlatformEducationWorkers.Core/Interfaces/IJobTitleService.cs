using PlatformEducationWorkers.Core.Models;

namespace PlatformEducationWorkers.Core.Interfaces
{
    public interface IJobTitleService
    {
        Task<IEnumerable<JobTitle>> GetAllJobTitles(int idEnterprice);
        Task<JobTitle> GetJobTitle(int idRole);
        Task DeleteJobTitle(int idRole);
        Task<JobTitle> AddingRole(JobTitle role);
        Task<JobTitle> UpdateJobTitle(JobTitle role);
        Task<IEnumerable<JobTitle>> GetAvaliableRoles(int enterpriseId);

    }
}
