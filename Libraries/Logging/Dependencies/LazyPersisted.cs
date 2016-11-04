namespace SBrickey.Libraries.Logging.Dependencies
{
    using System;

    /// <summary>
    /// Provides a Lazy implementation of atomic data persistence.
    /// </summary>
    /// <remarks>
    /// Because the persistence operation (execution of factory delegate) is atomic, its Value is exposed as a METHOD
    /// to remove attempted execution by the Debugger, and to remind users that it's more than just a lookup.
    /// 
    /// Internally, instances are tracked by GUID, to provide thread safety of persistence.
    /// </remarks>
    public class LazyPersisted<T>
    {
        private readonly static AtomicLockableDictionary<Guid, T> _persistedScopeIDs = new AtomicLockableDictionary<Guid, T>();

        private readonly Guid _internalIdentifier = Guid.NewGuid();
        private readonly Func<T> _persistFactory;
        private readonly Lazy<T> _persistedValue;

        public LazyPersisted(Func<T> persistFactory)
        {
            this._persistFactory = persistFactory;

            this._persistedValue = new Lazy<T>(() =>
                _persistedScopeIDs.GetOrAdd(
                    key: this._internalIdentifier,
                    valueFactory: () => this._persistFactory()
                ) // PersistedIDs.GetOrAdd
            ); // Lazy<>
        }

        /// <summary>
        /// If the value has already been persisted, this will simply retrieve its value.
        /// If the value has NOT been persisted, this will EXECUTE the factory delegate, retain the value for future lookups, and then return the value.
        /// </summary>
        public T Value() { return _persistedValue.Value; }
    } // class
} // namespace