
using PlatformEducationWorkers.Core.Models;

namespace PlatformEducationWorkers.Core.Interfaces.Enterprises
{
    public interface IEnterpriseService
    {
        Task<Enterprise> UpdateEnterprise(Enterprise enterprise);
        Task DeleteingEnterprise(int enterpriseId);
        Task<Enterprise> GetEnterprise(int enterpriseId);
        Task<Enterprise> GetEnterpriseByUser(int userId);
        Task<Enterprise> GetEnterprise(string title);
    }
}
