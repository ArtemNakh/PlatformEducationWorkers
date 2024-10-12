using PlatformEducationWorkers.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Core.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAllRoles(int idEnterprice);
        Task<Role> GetRole(int idRole);
        Task DeleteRole(int idRole);
        Task<Role> AddingRole(Role role);
        Task<Role> UpdateRole(Role role);

    }
}
