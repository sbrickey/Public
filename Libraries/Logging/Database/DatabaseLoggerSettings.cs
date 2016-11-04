namespace SBrickey.Libraries.Logging.Database
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DatabaseLoggerSettings
    {

        public IDatabaseLogWriter<int> DBLogWriter { get; set; } = new DatabaseLogWriter();

        /// <summary>
        /// Name of the application generating the logs. The default is the current application process (determined via reflection).
        /// </summary>
        public string Application { get; set; } = System.Reflection.Assembly.GetEntryAssembly().FullName;

        /// <summary>
        /// The function used to filter events based on the log level.
        /// </summary>
        public Func<string, LogLevel, bool> Filter { get; set; }

    } // class
} // namespace