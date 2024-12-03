using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Core.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly ILogRepository _context;

        public LoggerService(ILogRepository context)
        {
            _context = context;
        }

        public async Task LogAsync(LogType logType, string message, int? userId = null)
        {
            var logEntry = new LogEntry
            {
                LogType = logType,
                Message = message,
                Timestamp = DateTime.UtcNow,
                UserId = userId
            };

            _context.AddLog(logEntry);
        }

    }
}
