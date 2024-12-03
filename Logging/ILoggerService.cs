using PlatformEducationWorkers.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Core.Interfaces
{
    public interface ILoggerService
    {
        Task LogAsync(LogType logType, string message, int? userId = null);
    }
}
