using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBrickey.Libraries.Generics
{
    /// <summary>
    /// Provides a readonly dictionary with typed item selector
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class ReadOnlyKeyedCollection<TValue, TKey> : ReadOnlyCollection<TValue>
    {
        private readonly Dictionary<TKey, int> Index;
        public ReadOnlyKeyedCollection(IList<TValue> items, Func<TValue, TKey> keySelector) : base(items)
        {
            Index = items.ToDictionary(
                keySelector: i => keySelector(i),
                elementSelector: i => this.Items.IndexOf(i)
            );
        }
        
        public TValue this[TKey key]
        {
            get
            {
                return this.Items[this.Index[key]];
            }
        }

    } // class

    /// <summary>
    /// Provides a readonly dictionary with typed item selectors
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TKey1"></typeparam>
    /// <typeparam name="TKey2"></typeparam>
    public class ReadOnlyKeyedCollection<TValue, TKey1, TKey2> : ReadOnlyCollection<TValue>
    {
        private readonly Dictionary<TKey1, int> Key1Index;
        private readonly Dictionary<TKey2, int> Key2Index;
        public ReadOnlyKeyedCollection(IList<TValue> items, Func<TValue, TKey1> key1Selector, Func<TValue, TKey2> key2Selector)
            : base(items)
        {
            Key1Index = items.ToDictionary(
                keySelector: i => key1Selector(i),
                elementSelector: i => this.Items.IndexOf(i)
            );
            Key2Index = items.ToDictionary(
                keySelector: i => key2Selector(i),
                elementSelector: i => this.Items.IndexOf(i)
            );
        }

        public TValue this[TKey1 key]
        {
            get
            {
                return this.Items[this.Key1Index[key]];
            }
        }
        public TValue this[TKey2 key]
        {
            get
            {
                return this.Items[this.Key2Index[key]];
            }
        }

    } // class

} // namespace