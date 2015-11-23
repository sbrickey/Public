namespace SBrickey.Libraries.Generics
{
    using System;

    public class Cached<T> where T : class
    {
        private T cachedResponse = null;
        private Func<T> factory;

        // ctor
        public Cached(Func<T> Factory)
        {
            if (Factory == null)
                throw new ArgumentNullException("Factory");

            this.factory = Factory;
        }

        public T Value
        {
            get
            {
                if (this.cachedResponse == null)
                {
                    cachedResponse = factory();
                }
                return cachedResponse;
            }
        }
        
        public void Invalidate() { this.cachedResponse = null; }
        
        public static explicit operator T(Cached<T> o) { return o.Value; }
    } // class Cached

    public class CachedAsync<T> where T : class
    {
        private T cachedResponse = null;
        private readonly Action<T> AsyncDelegate = null;
        private readonly Action<Action<T>> factory;

        public CachedAsync(Action<Action<T>> FactoryInvokeAsync)
        {
            this.AsyncDelegate = new Action<T>((value) => { this.cachedResponse = value; });
            this.factory = FactoryInvokeAsync;
            this.factory(AsyncDelegate);
        }


        public T Value { get { return cachedResponse; } }

        public void Invalidate() { this.cachedResponse = null; this.factory(this.AsyncDelegate); }

        public static explicit operator T(CachedAsync<T> o) { return o.Value; }

    } // class CachedAsync
} // namespace