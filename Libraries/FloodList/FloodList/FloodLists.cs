using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBrickey.Libraries.FloodList
{
    public interface iFloodList
    {
        void Drop(object value);
        T Catch<T>();
    }
    public interface iFloodList<T>
    {
        void Drop(T value);
        T Catch();
    }


    public class FloodList_Generic_Queue_SizeLimited<T> : iFloodList<T>
    {
        private T defaultT = default(T);
        private readonly Queue<T> _data = new Queue<T>();
        public int Max { get; set; }

        public FloodList_Generic_Queue_SizeLimited()
        {
            Max = 1;
        }

        public void Drop(T value)
        {
            lock (this)
            {
                if (_data.Count > Max)
                    return;

                _data.Enqueue(value);
            }
        }

        public T Catch()
        {
            lock (this)
            {
                if (_data.Any())
                    return _data.Dequeue();
                else
                    return defaultT;
            }
        }
    }
    public class FloodList_Generic_Queue_TryCatch<T> : iFloodList<T>
    {
        private T defaultT = default(T);
        private readonly Queue<T> _data = new Queue<T>();

        public void Drop(T value)
        {
            _data.Enqueue(value);
        }

        public T Catch()
        {
            try
            {
                return _data.Dequeue();
            }
            catch (InvalidOperationException ex)
            {
                return defaultT;
            }
        }
    }
    public class FloodList_Generic_Queue_TryCatch_PublicStatic<T> : iFloodList<T>
    {
        private T defaultT = default(T);
        public static readonly Queue<T> _data = new Queue<T>();

        public void Drop(T value)
        {
            _data.Enqueue(value);
        }

        public T Catch()
        {
            try
            {
                return _data.Dequeue();
            }
            catch (InvalidOperationException)   // queue is empty
            {
                return defaultT;
            }
        }
    }
    public class FloodList_Generic_Queue_WithLocks<T> : iFloodList<T>
    {
        private T defaultT = default(T);
        private readonly Queue<T> _data = new Queue<T>();

        private object lockObj = new object();

        public void Drop(T value)
        {
            lock (lockObj)
            {
                _data.Enqueue(value);
            }
        }

        public T Catch()
        {
            lock (lockObj)
            {
                if (_data.Count > 0)
                    return _data.Dequeue();
                else
                    return defaultT;
            }
        }
    }
    public class FloodList_Generic_Queue_WithLocks_TryCatch<T> : iFloodList<T>
    {
        private T defaultT = default(T);
        private readonly Queue<T> _data = new Queue<T>();

        private object lockObj = new object();

        public void Drop(T value)
        {
            lock (lockObj)
            {
                _data.Enqueue(value);
            }
        }

        public T Catch()
        {
            lock (lockObj)
            {
                try
                {
                    return _data.Dequeue();
                }
                catch (InvalidOperationException) // empty queue
                {
                    return defaultT;
                }
            }
        }
    }

    /// <summary>
    /// simple approach to a floodlist
    /// </summary>
    /// <remarks>
    /// seems that the locks cause the Drop calls to queue up faster than the Catch calls.
    /// Results in ~50% catch : drop ratio (multi-threaded)
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class FloodList_Generic<T> : iFloodList<T>
    {
        private T defaultT = default(T);
        private T value;
        private bool HasValue = false;

        private object LockObj = new object();

        public void Drop(T value)
        {
            lock (LockObj)
            {
                this.value = value;
                this.HasValue = true;
            }
        }

        public T Catch()
        {
            lock (LockObj)
            {
                if (this.HasValue)
                {
                    this.HasValue = false;
                    return this.value;
                }
            } // lock

            return defaultT;    // faster here than as an else inside the lock
        } // T Catch()
    }

    /// <summary>
    /// floodlist using light locking
    /// </summary>
    /// <remarks>
    /// faster than any other approach (locked queues, locked variables, etc).
    ///     fastest provider data drop; consumer data catch seems to be dependent on data type
    /// results in approx 60-80% catch : throw ratio (multi-threaded, 2 providers : 1 consumer)
    ///     (60's for int32, 80's for string)
    /// </remarks>
    public class FloodList_Generic2<T> : iFloodList<T>
    {
        private T valueR = default(T);
        private T valueW = default(T);
        private T defaultT = default(T);

        private object ReadLock = new object();
        private object WriteLock = new object();

        public void Drop(T value)
        {
            lock (WriteLock)
            {
                this.valueW = value;
            }
        }

        public T Catch()
        {
            T outval = defaultT;

            lock (ReadLock)
            {

                if (this.valueR == null || this.valueR.Equals(defaultT))
                {
                    this.valueR = this.valueW;
                    this.valueW = defaultT;
                    //this.HasValue = this.valueR != null && !this.valueR.Equals(defaultT);
                }

                if (this.valueR != null && !this.valueR.Equals(defaultT))
                {
                    outval = this.valueR;
                    this.valueR = defaultT;
                    //HasValue = false;
                }

            }

            return outval;
        }
    }

    public class FloodList_ConcurrentQueue<T> : iFloodList<T>
    {
        private System.Collections.Concurrent.ConcurrentQueue<T> _data = new System.Collections.Concurrent.ConcurrentQueue<T>();
        private T defaultT = default(T);

        public void Drop(T value)
        {
            _data.Enqueue(value);
        }

        public T Catch()
        {
            T outval = defaultT;
            
            _data.TryDequeue(out outval);
            
            return outval;

        }
    }



    public class FloodList_DualDict : iFloodList
    {
        private readonly Dictionary<Type, object> _default = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> _data = new Dictionary<Type, object>();

        public void Drop(object value)
        {
            Type t = value.GetType();

            lock (this)
            {
                if (_data.ContainsKey(t))
                    _data[t] = value;
                else
                    _data.Add(t, value);
            }
        }

        public T Catch<T>()
        {
            Type t = typeof(T);

            if (!_default.ContainsKey(t))
                _default.Add(t, default(T));

            T outval = (T)_default[t];
            lock (this)
            {
                if (_data.ContainsKey(t))
                {
                    outval = (T)_data[t];
                    _data.Remove(t);
                }
            }
            return outval;
        }
    }

    public class FloodList_Dict_DeleteOnCatch : iFloodList
    {
        private readonly Dictionary<Type, object> _data = new Dictionary<Type, object>();
        private object datalock = new object();

        public void Drop(object value)
        {
            Type t = value.GetType();
            lock (datalock)
            {
                if (_data.ContainsKey(t))
                    _data[t] = value;
                else
                    _data.Add(t, value);
            }
        }

        public T Catch<T>()
        {
            Type t = typeof(T);

            T outval = default(T);
            lock (datalock)
            {
                if (_data.ContainsKey(t))
                {
                    outval = (T)_data[t];
                    _data.Remove(t);
                }
            }
            return outval;
        }
    }
    public class FloodList_Dict_ReplaceWithDefault : iFloodList
    {
        private readonly Dictionary<Type, object> _data = new Dictionary<Type, object>();

        public void Drop(object value)
        {
            Type t = value.GetType();
            lock (this)
            {
                if (_data.ContainsKey(t))
                    _data[t] = value;
                else
                    _data.Add(t, value);
            }
        }

        public T Catch<T>()
        {
            Type t = typeof(T);

            T outval = default(T);
            lock (this)
            {
                if (_data.ContainsKey(t))
                {
                    outval = (T)_data[t];
                    _data[t] = default(T);
                }
            }
            return outval;
        }
    }
    public class FloodList_Dict_ReplaceWithDefault2 : iFloodList
    {
        private readonly Dictionary<Type, object> _data = new Dictionary<Type, object>();

        public void Drop(object value)
        {
            Type t = value.GetType();
            lock (this)
            {
                if (_data.ContainsKey(t))
                    _data[t] = value;
                else
                    _data.Add(t, value);
            }
        }

        public T Catch<T>()
        {
            Type t = typeof(T);

            T T_Default = default(T);
            T outval = T_Default;
            lock (this)
            {
                if (_data.ContainsKey(t))
                {
                    outval = (T)_data[t];
                    _data[t] = T_Default;
                }
            }
            return outval;
        }
    }

    public class FloodList_List_Linq : iFloodList
    {
        private readonly List<object> _data = new List<object>();

        public void Drop(object value)
        {
            _data.Add(value);
        }

        public T Catch<T>()
        {
            Type t = typeof(T);
            lock (this)
            {
                
            }

            return default(T);
        }
    }

}
