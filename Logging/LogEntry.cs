using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Logger
{
    public class LogEntry
    {
        public int Id { get; set; }
        public LogType LogType { get; set; } // Тип лога через enum
        public string Message { get; set; } // Текст лога
        public DateTime Timestamp { get; set; } // Дата та час
        public int? UserId { get; set; } // Ідентифікатор людини, яка відправила лог (nullable)

    }
}
