namespace SBrickey.Libraries.Generics
{
    using System;

    public sealed class ObservableWatchDispatcherIdle<T>
    {
        private T _value;

        private bool dispatcherWatch_Stop = true;
        private DateTime lastIdleRunTime = DateTime.Now;
        private readonly GetValueDelegate dispatcherWatch_GetValueDelegate = null;
        private readonly TimeSpan dispatcherWatch_Delay = TimeSpan.FromSeconds(1);

        public class ChangedEventArgs : EventArgs
        {
            public T OldValue { get; set; }
            public T NewValue { get; set; }
        }
        public event EventHandler<ChangedEventArgs> ValueChanged;

        public delegate T GetValueDelegate();


        // CTor
        public ObservableWatchDispatcherIdle(GetValueDelegate GetValueDelegate, EventHandler<ChangedEventArgs> OnValueChangedDelegate = null, TimeSpan? Delay = null, bool AutoStart = false)
        {
            // required parameter(s)
            this.dispatcherWatch_GetValueDelegate = GetValueDelegate;

            // optional parameter(s)
            if (OnValueChangedDelegate != null)
                this.ValueChanged += OnValueChangedDelegate;

            if (Delay.HasValue)
                this.dispatcherWatch_Delay = Delay.Value;

            if (AutoStart)
                this.Start();

            // private variables
            this._value = GetValueDelegate.Invoke();

            System.Windows.Interop.ComponentDispatcher.ThreadIdle += ComponentDispatcher_ThreadIdle;
        }

        public void Start() { this.dispatcherWatch_Stop = false; }

        public void Stop() { this.dispatcherWatch_Stop = true; }



        private void ComponentDispatcher_ThreadIdle(object sender, EventArgs e)
        {
            if (!this.dispatcherWatch_Stop &&
                DateTime.Now > this.lastIdleRunTime.Add(this.dispatcherWatch_Delay))
            {
                this.threadWatch_CheckValue();

                this.lastIdleRunTime = DateTime.Now;
            }

        }

        private void threadWatch_CheckValue()
        {
            var newVal = this.dispatcherWatch_GetValueDelegate.Invoke();

            if (!this._value.Equals(newVal))
            {
                T oldValue = _value;
                _value = newVal;

                var handler = this.ValueChanged;
                if (handler != null)
                    handler(this, new ChangedEventArgs() { OldValue = oldValue, NewValue = newVal });
            } // value hasn't changed
        } // void threadWatch_GetValue()

    } // class
} // namespace