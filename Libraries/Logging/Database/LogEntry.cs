namespace SBrickey.Libraries.Logging.Database
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class LogEntry : LogBase
    {
        
        public LogLevel Level { get; set; } = LogLevel.Information;

        public EventId EventId { get; set; }

        public string Message { get; set; }

        public string Exception { get; set; }

    } // class
    
    public class LogBase
    {

        public DateTime TimeStamp { get; set; } = DateTime.Now;

        public string ComputerName { get; set; } = System.Environment.MachineName;

        public string UserName { get; set; } = System.Security.Principal.WindowsIdentity.GetCurrent().Name; //System.Security.Principal.WindowsPrincipal.Current.Identity.Name;

        public string ProcessName { get; set; } = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

        public int ProcessId { get; set; } = System.Diagnostics.Process.GetCurrentProcess().Id;

        public int ThreadId { get; set; } = System.Threading.Thread.CurrentThread.ManagedThreadId;

        /// <summary>
        /// Within the scope of ILogger, this is the Name of the TYPE of logger.
        /// </summary>
        public string Category { get; set; }

    }

} // namespace