namespace SBrickey.Libraries.Logging.Database
{
    using Microsoft.Extensions.Logging;
    using Nito.AsyncEx.AsyncLocal;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TIdentifier">Data type of the persisted scope identifier</typeparam>
    public class DatabaseWriterLogScope<TIdentifier> : LogBase, ILogScope
    {
        #region Static

        private static readonly ThreadShared<DatabaseWriterLogScope<TIdentifier>> _context = new ThreadShared<DatabaseWriterLogScope<TIdentifier>>();
        public static DatabaseWriterLogScope<TIdentifier> Current { get { return _context.Value; } private set { _context.Value = value; } }

        public static ILogScope Push(IDatabaseLogWriter<TIdentifier> dbLogWriter, string name, object state)
        {
            var temp = Current;
            Current = new DatabaseWriterLogScope<TIdentifier>(dbLogWriter, name, state, temp);
            return Current;
        }

        #endregion


        private readonly object _state;
        public DatabaseWriterLogScope<TIdentifier> Parent { get; }
        public LazyPersisted<TIdentifier> PersistedId { get; }

        // ctor
        private DatabaseWriterLogScope(IDatabaseLogWriter<TIdentifier> dbWriter, string categoryName, object state, DatabaseWriterLogScope<TIdentifier> parent = null)
        {
            // base
            this.Category = categoryName;

            // local
            this._state = state;
            this.Parent = parent;
            this.PersistedId = new LazyPersisted<TIdentifier>(() => dbWriter.WriteScope(this));
        }

        

        public override string ToString()
        {
            return _state?.ToString();
        }

        string ILogScope.Name() { return this._state?.ToString(); }
        void IDisposable.Dispose() { Current = Current?.Parent; }
        
    } // class
} // namespace