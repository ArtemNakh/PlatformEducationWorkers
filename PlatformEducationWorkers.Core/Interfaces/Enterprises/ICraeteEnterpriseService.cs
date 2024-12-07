using PlatformEducationWorkers.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Core.Interfaces.Enterprises
{
    public interface ICreateEnterpriseService
    {
        Task AddEnterpriseWithOwnerAsync(Enterprise enterprise, string jobTitleName, User owner);
    }
}
