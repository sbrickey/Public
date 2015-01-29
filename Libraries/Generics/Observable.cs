using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBrickey.Libraries.Generics
{
    // borrowed from http://smellegantcode.wordpress.com/2008/08/22/observable-property-pattern/
    public class Observable<T> where T : class
    {
        private T _value;

        public class ChangedEventArgs : EventArgs
        {
            public T OldValue { get; set; }
            public T NewValue { get; set; }
        }

        public EventHandler<ChangedEventArgs> Changed;

        public T Value
        {
            get { return _value; }

            set
            {
                if (!value.Equals(_value))
                {
                    T oldValue = _value;
                    _value = value;

                    EventHandler<ChangedEventArgs> handler = Changed;
                    if (handler != null)
                        handler(this, new ChangedEventArgs
                        {
                            OldValue = oldValue,
                            NewValue = value
                        });
                }
            }
        }
    } // class Observable


    public class ObservableStruct<T> where T : struct
    {
        private T? _value;

        public class ChangedEventArgs : EventArgs
        {
            public T? OldValue { get; set; }
            public T? NewValue { get; set; }
        }

        public EventHandler<ChangedEventArgs> Changed;

        public T? Value
        {
            get { return _value; }

            set
            {
                if (!value.Equals(_value))
                {
                    T? oldValue = _value;
                    _value = value;

                    EventHandler<ChangedEventArgs> handler = Changed;
                    if (handler != null)
                        handler(this, new ChangedEventArgs
                        {
                            OldValue = oldValue,
                            NewValue = value
                        });
                }
            }
        }
    } // class ObservableStruct
} // namespace