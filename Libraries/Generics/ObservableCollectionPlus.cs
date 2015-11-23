namespace SBrickey.Libraries.Generics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Typed ObservableCollection with an AddRange implementation for quicker loading (fewer INPC firings)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableCollectionPlus<T> : ObservableCollection<T>, INotifyCollectionChanged
    {

        public void AddRange(IEnumerable<T> dataToAdd)
        {
            this.CheckReentrancy();

            // We need the starting index later
            int startingIndex = this.Count;

            // Add the items directly to the inner collection
            foreach (var data in dataToAdd)
            {
                this.Items.Add(data);
            }

            // Now raise the changed events
            this.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Count"));
            this.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsEmpty"));
            this.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Item[]"));

            // We have to change our input of new items into an IList since that is what the event args require.
            //var changedItems = new List<T>(dataToAdd);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)); //, changedItems, startingIndex));
        }

        public bool IsEmpty { get { return this.Count == 0; } }

    } // class
} // namespace