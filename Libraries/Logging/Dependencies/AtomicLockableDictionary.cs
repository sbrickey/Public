namespace SBrickey.Libraries.Logging.Dependencies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a Dictionary with multi-threaded atomic internal storage, and Ensure-pattern accessor
    /// </summary>
    /// <remarks>
    /// Uses a double LOCK implementation (per-dictionary for new keys, and per-key for lookup).
    /// Be aware of performance implications in write-heavy applications.
    /// Primary use case: Lookup heavy operations
    /// </remarks>
    public class AtomicLockableDictionary<TKey, TValue>
    {
        private readonly object _lockObj = new object();
        private readonly Dictionary<TKey, object> _keyLocks = new Dictionary<TKey, object>();
        private readonly Dictionary<TKey, TValue> _keyValues = new Dictionary<TKey, TValue>();
        
        public TValue GetOrAdd(TKey key, Func<TValue> valueFactory)
        {
            object valueLockObj = null;
            lock (this._lockObj)
            {
                var exists = _keyLocks.ContainsKey(key);

                if (exists)
                    valueLockObj = _keyLocks[key];
                else
                {
                    valueLockObj = new object();
                    _keyLocks.Add(key, valueLockObj);
                }
            } // lock

            if (valueLockObj == null)
                throw new KeyNotFoundException("Unable to find lock object!");

            lock (valueLockObj) // lock on the value
            {
                var exists = _keyValues.ContainsKey(key);
                if (exists)
                    return _keyValues[key];

                var value = valueFactory();
                _keyValues[key] = value;
                return value;
            }
        } // TValue GetOrAdd(...)

        public void Update(TKey key, Func<TValue> valueFactory)
        {
            object valueLockObj = null;
            lock (this._lockObj)
            {
                var exists = _keyLocks.ContainsKey(key);

                if (!exists)
                    return;

                valueLockObj = _keyLocks[key];
            } // lock

            if (valueLockObj == null)
                throw new KeyNotFoundException("Unable to find lock object!");

            lock (valueLockObj) // lock on the value
            {
                var value = valueFactory();
                _keyValues[key] = value;
                return;
            }
        } // void Update(...)
        
    } // class
} // namespace