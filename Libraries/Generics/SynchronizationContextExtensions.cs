namespace SBrickey.Libraries.Generics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides typed Post/Send delegate execution on a specific synchronization context
    /// </summary>
    public static class SynchronizationContextExtensions
    {

        public delegate void SendOrPostCallback<T>(T inp);

        /// <summary>
        /// Asynchronous execution of a delegate, on the context's thread
        /// </summary>
        public static void Post<T>(this SynchronizationContext ctx, SendOrPostCallback<T> delegateCode, T typedObj)
        {
            ctx.Post(
                d: (objParam) => { delegateCode((T)objParam); },
                state: typedObj
            );
        }
        /// <summary>
        /// Synchronous execution of a delegate, on the context's thread
        /// </summary>
        public static void Send<T>(this SynchronizationContext ctx, SendOrPostCallback<T> delegateCode, T typedObj)
        {
            ctx.Send(
                d: (objParam) => { delegateCode((T)objParam); },
                state: typedObj
            );
        }

    } // class
} // namespace