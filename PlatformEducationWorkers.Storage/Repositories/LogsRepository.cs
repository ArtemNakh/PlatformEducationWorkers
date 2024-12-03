using PlatformEducationWorkers.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Storage.Repositories
{
    public class LogsRepository : ILogRepository
    {
        private readonly PlatformEducationContex _context;

        public LogsRepository(PlatformEducationContex context)
        {
            _context = context;
        }

        public async Task<T> AddLog<T>(T entity) where T : class
        {
            var obj = _context.Add(entity);
            await _context.SaveChangesAsync();
            return obj.Entity;
        }


    }
}