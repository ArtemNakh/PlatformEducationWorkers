using PlatformEducationWorkers.Core.Models;

namespace PlatformEducationWorkers.Core.Interfaces.Repositories
{
    public interface IEnterpriseRepository
    {
        Task AddEnterpriseAsync(Enterprise enterprise);

        Task AddJobTitleAsync(JobTitle jobTitle);
        Task AddUserAsync(User user);

        Task SaveChangesAsync();

        Task BeginTransactionAsync();

        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
