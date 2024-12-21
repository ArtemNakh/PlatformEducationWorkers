using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlatformEducationWorkers.Core.Models;

namespace PlatformEducationWorkers.Core.Interfaces.Enterprises
{
    public interface IEnterpriseService
    {
        Task<Enterprise> AddingEnterprise(Enterprise enterprise);
        Task<Enterprise> UpdateEnterprise(Enterprise enterprise);
        Task DeleteingEnterprise(int enterpriseId);
        Task<Enterprise> GetEnterprise(int enterpriseId);
        Task<Enterprise> GetEnterpriseByUser(int userId);
        Task<Enterprise> GetEnterprise(string title);
        Task<bool> HasEnterprise(int enterpriseId);
    }
}
