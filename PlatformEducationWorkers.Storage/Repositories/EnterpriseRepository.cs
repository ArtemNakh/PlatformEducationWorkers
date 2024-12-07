using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Storage.Repositories
{
    public class EnterpriseRepository : IEnterpriseRepository
    {
        private readonly PlatformEducationContex _context;
        private IDbContextTransaction _transaction;

        public EnterpriseRepository(PlatformEducationContex context)
        {
            _context = context;
        }

        public async Task AddEnterpriseAsync(Enterprise enterprise)
        {
            await _context.Set<Enterprise>().AddAsync(enterprise);
        }

        public async Task AddJobTitleAsync(JobTitle jobTitle)
        {
            await _context.Set<JobTitle>().AddAsync(jobTitle);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Set<User>().AddAsync(user);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
}
