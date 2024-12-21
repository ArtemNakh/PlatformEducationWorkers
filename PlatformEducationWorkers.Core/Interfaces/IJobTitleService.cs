using PlatformEducationWorkers.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
