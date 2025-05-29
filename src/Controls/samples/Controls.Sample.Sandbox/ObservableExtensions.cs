using System;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Controls.Sample
{

    public class ObservableCollectionFast<T> : ObservableCollection<T>
    {
        // If your Models expose an ObservableCollection, and you are adding lots of elements, you don't want this adding to fire a UI event every time.
        // You can use this ObservableCollection which only notifies the UI to update after all the elements are updated when AddRange is used.

        public ObservableCollectionFast() : base() { }

        public ObservableCollectionFast(IEnumerable<T> collection) : base(collection) { }

        public ObservableCollectionFast(List<T> list) : base(list) { }


        public void AddRange(IEnumerable<T> range)
        {
            int count = this.Count;
            foreach (var item in range)
            {
                Items.Add(item);
            }

            if (this.Count != count)
            {
                this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            }
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Reset(IEnumerable<T> range)
        {
            this.Items.Clear();

            AddRange(range);
        }

        //public void ClearAll()
        //{
        //    this.Items.Clear();

        //    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        //}


        //---------------------------------------------------------------------------------------------------------
        public void InsertRange(int index, IEnumerable<T> collection)
        //---------------------------------------------------------------------------------------------------------
        {
            //NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add;

            CheckReentrancy();

            int currentIndex = index;
            var changedItems = collection is List<T> ? (List<T>)collection : new List<T>(collection);
            int count = this.Count;
            foreach (var i in changedItems)
            {
                Items.Insert(currentIndex, i);
                currentIndex++;
            }
            if (this.Count != count)
            {
                this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            }
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            //Debug.WriteLine("before OnCollectionChanged()" + Utils.GetDateTimeString());
            //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems, index));
            if (changedItems.Count > 0)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems, index));
            }
            //Debug.WriteLine("after OnCollectionChanged()" + Utils.GetDateTimeString());
        }

        //-----------------------------------------------------------------------------------------------------------
        public void ReportItemChange(T item)
        //-----------------------------------------------------------------------------------------------------------
        {
            NotifyCollectionChangedEventArgs args =
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    item,
                    item,
                    IndexOf(item));
            OnCollectionChanged(args);
        }

        public void RemoveRange(Predicate<T> remove)
        {
            // How to use: LogEntries.RemoveRange(i => closeFileIndexes.Contains(i.fileIndex));
            // iterates backwards so can remove multiple items without invalidating indexes
            for (var i = Items.Count - 1; i > -1; i--)
            {
                if (remove(Items[i]))
                    Items.RemoveAt(i);
            }

            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

}
