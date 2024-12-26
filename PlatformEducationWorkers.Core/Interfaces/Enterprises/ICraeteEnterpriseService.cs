using PlatformEducationWorkers.Core.Models;

namespace PlatformEducationWorkers.Core.Interfaces.Enterprises
{
    public interface ICreateEnterpriseService
    {
        Task AddEnterpriseWithOwnerAsync(Enterprise enterprise, string jobTitleName, User owner);
    }
}
