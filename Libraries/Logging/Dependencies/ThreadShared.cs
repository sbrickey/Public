namespace SBrickey.Libraries.Logging.Dependencies
{
    using Nito.AsyncEx.AsyncLocal;
    using System.Threading;

    public class ThreadShared<T> where T : class
    {
        // TODO : remove class requirement by converting private stores to objects (boxing for primitives). Refer to AsyncLocal for default(T) implementation.


        // lock object for ensuring thread-safe atomic operations (since we're specifically trying to keep two sources in sync)
        private static readonly object _lockObj = new object();

        // AsyncLocal enables passing context across .Net 4.5 Tasks.
        private static readonly AsyncLocal<T> _asyncLocalCtx = new AsyncLocal<T>();

        // ThreadLocal enables passing context across WCF async operations (OnCompleted delegates)
        private static readonly ThreadLocal<T> _threadLocalCtx = new ThreadLocal<T>();

        public T Value
        {
            get
            {
                lock (_lockObj)
                {
                    // The order of querying ThreadLocal FIRST is important
                    // Traditional ThreadLocal works more "correctly", but doesn't work with async at all (null)
                    //   but async occasionally restores invalid data, and thus doesn't necessarily work "correctly" all the time.
                    // By using ThreadLocal first, we prefer its correct behavior, but fail back to Async support
                    return _threadLocalCtx.Value ?? _asyncLocalCtx.Value;
                }
            }

            // the order here doesn't matter in the slightest
            set
            {
                lock (_lockObj)
                {
                    _asyncLocalCtx.Value = value; _threadLocalCtx.Value = value;
                }
            }
        }

    } // class
} // namespace