using Microsoft.EntityFrameworkCore;
using PlatformEducationWorkers.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Core.Interfaces.Repositories
{
    public interface IEnterpriseRepository
    {
        Task AddEnterpriseAsync(Enterprice enterprise);

        Task AddJobTitleAsync(JobTitle jobTitle);
        Task AddUserAsync(User user);

        Task SaveChangesAsync();

        Task BeginTransactionAsync();

        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
