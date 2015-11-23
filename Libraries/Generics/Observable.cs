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

        public event EventHandler<ChangedEventArgs> Changed;

        public T Value
        {
            get { return _value; }

            set
            {
                // if both null, do nothing
                if (value == null && _value == null)
                    return;

                // if one is null and other is not, or values differ, set and fire
                if ((value == null && _value != null) ||
                    (value != null && _value == null) ||
                    (!value.Equals(_value)))
                {
                    T oldValue = _value;
                    _value = value;

                    var handler = Changed;
                    if (handler != null)
                        handler(this, new ChangedEventArgs
                        {
                            OldValue = oldValue,
                            NewValue = value
                        });
                }
            }
        }

        // ctors
        public Observable() { }
        public Observable(Action<string> inpcDelegate, string name)
        { this.Changed += (s, e) => { inpcDelegate(name); }; }
        public Observable(Action<string> inpcDelegate, System.Linq.Expressions.Expression<Func<object>> member)
        { this.Changed += (s, e) => { inpcDelegate((member.Body as System.Linq.Expressions.MemberExpression).Member.Name); }; }
    } // class Observable


    public class ObservableNullableStruct<T> where T : struct
    {
        private T? _value;

        public class ChangedEventArgs : EventArgs
        {
            public T? OldValue { get; set; }
            public T? NewValue { get; set; }
        }

        public event EventHandler<ChangedEventArgs> Changed;

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
        } // Value

        // ctors
        public ObservableNullableStruct() { }
        public ObservableNullableStruct(Action<string> inpcDelegate, string name)
        { this.Changed += (s, e) => { inpcDelegate(name); }; }
        public ObservableNullableStruct(Action<string> inpcDelegate, System.Linq.Expressions.Expression<Func<object>> member)
        { this.Changed += (s, e) => { inpcDelegate((member.Body as System.Linq.Expressions.MemberExpression).Member.Name); }; }
        public ObservableNullableStruct(T DefaultValue, Action<string> inpcDelegate, System.Linq.Expressions.Expression<Func<object>> member)
        {
            this._value = DefaultValue;
            this.Changed += (s, e) => { inpcDelegate((member.Body as System.Linq.Expressions.MemberExpression).Member.Name); };
        }
    } // class ObservableNullableStruct


    public class ObservableStruct<T> where T : struct
    {
        private T _value;

        public class ChangedEventArgs : EventArgs
        {
            public T OldValue { get; set; }
            public T NewValue { get; set; }
        }

        public event EventHandler<ChangedEventArgs> Changed;

        public T Value
        {
            get { return _value; }

            set
            {
                if (!value.Equals(_value))
                {
                    T oldValue = _value;
                    _value = value;

                    var handler = Changed;
                    if (handler != null)
                        handler(this, new ChangedEventArgs
                        {
                            OldValue = oldValue,
                            NewValue = _value
                        });
                }
            }
        } // Value

        // ctors
        public ObservableStruct() { }

        public ObservableStruct(Action<string> inpcDelegate, string name)
        { this.Changed += (s, e) => { inpcDelegate(name); }; }
        public ObservableStruct(Action<string> inpcDelegate, string name, T DefaultValue = default(T))
        {
            this._value = DefaultValue;
            this.Changed += (s, e) => { inpcDelegate(name); };
        }

        public ObservableStruct(Action<string> inpcDelegate, System.Linq.Expressions.Expression<Func<object>> member)
        { this.Changed += (s, e) => { inpcDelegate((member.Body as System.Linq.Expressions.MemberExpression).Member.Name); }; }
        public ObservableStruct(Action<string> inpcDelegate, System.Linq.Expressions.Expression<Func<object>> member, T DefaultValue = default(T))
        {
            this._value = DefaultValue;
            this.Changed += (s, e) => { inpcDelegate((member.Body as System.Linq.Expressions.MemberExpression).Member.Name); };
        }
    } // class ObservableStruct
} // namespace