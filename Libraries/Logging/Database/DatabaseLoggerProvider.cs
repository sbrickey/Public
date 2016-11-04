namespace SBrickey.Libraries.Logging.Database
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DatabaseLoggerProvider : ILoggerProvider
    {
        private readonly DatabaseLoggerSettings _settings;

        public DatabaseLoggerProvider() : this(settings: new DatabaseLoggerSettings()) { }
        public DatabaseLoggerProvider(DatabaseLoggerSettings settings) { this._settings = settings; }

        ILogger ILoggerProvider.CreateLogger(string categoryName)
        {
            return new DatabaseLogger(
                categoryName: categoryName,
                settings: this._settings
            );
        }

        void IDisposable.Dispose() { /* no-op */ }
    } // class
} // namespace