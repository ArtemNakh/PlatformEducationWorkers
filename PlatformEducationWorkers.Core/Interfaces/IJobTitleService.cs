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
        Task<IEnumerable<JobTitle>> GetAllRoles(int idEnterprice);
        Task<JobTitle> GetRole(int idRole);
        Task DeleteRole(int idRole);
        Task<JobTitle> AddingRole(JobTitle role);
        Task<JobTitle> UpdateRole(JobTitle role);

    }
}
