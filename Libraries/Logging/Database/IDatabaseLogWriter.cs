namespace SBrickey.Libraries.Logging.Database
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IDatabaseLogWriter<T>
    {

        T WriteScope(DatabaseWriterLogScope<T> scope);
        void Write(LogEntry entry);

    } // class
} // namespace