namespace SBrickey.Libraries.Logging.Database
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class DatabaseLoggerFactoryExtensions
    {

        /// <summary>
        /// Adds a database logger that is enabled for <see cref="LogLevel"/>.Information or higher.
        /// </summary>
        /// <param name="factory">The extension method argument.</param>
        public static ILoggerFactory AddDatabase(this ILoggerFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            return AddDatabase(factory, LogLevel.Information);
        }


        /// <summary>
        /// Adds a database logger that is enabled for <see cref="LogLevel"/>s of <paramref name="minLevel" /> or higher.
        /// </summary>
        /// <param name="factory">The extension method argument.</param>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged</param>
        public static ILoggerFactory AddDatabase(this ILoggerFactory factory, LogLevel minLevel)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            var providerSettings = new DatabaseLoggerSettings()
            {
                Filter = (_, logLevel) => logLevel >= minLevel
            };
            var provider = new DatabaseLoggerProvider(settings: providerSettings);
            factory.AddProvider(provider);
            return factory;
        }
    } // class
} // namespace