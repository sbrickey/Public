namespace SBrickey.Libraries.Logging.Database
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DatabaseLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly DatabaseLoggerSettings _settings;

        public DatabaseLogger(string categoryName) : this(categoryName: categoryName, settings : new DatabaseLoggerSettings()) { }
        public DatabaseLogger(string categoryName, DatabaseLoggerSettings settings)
        {
            this._categoryName = categoryName;
            this._settings = settings;
        }


        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return this._settings.Filter(this._categoryName, logLevel);
        }

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!(this as ILogger).IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            var msg = formatter(state, exception);

            if (String.IsNullOrWhiteSpace(msg))
                return;

            var logEntry = new LogEntry() {
                Category = this._categoryName,
                EventId = eventId,
                Level = logLevel,
                Message = msg,
                Exception = exception?.ToString()
            };
            _settings.DBLogWriter.Write(logEntry);
        }

        ILogScope ILogger.BeginScope<TState>(TState state)
        {
            return DatabaseWriterLogScope<int>.Push(_settings.DBLogWriter, this._categoryName, state);
        }

    } // class
} // namespace