using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Opportunity.MvvmUniverse.Helpers;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// Implementation of a dynamic data collection based on generic Collection&lt;T&gt;,
    /// implementing INotifyCollectionChanged to notify listeners
    /// when items get added, removed or the whole list is refreshed.
    /// </summary>
    public class ObservableCollection<T> : System.Collections.ObjectModel.Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        //------------------------------------------------------
        //
        //  Constructors
        //
        //------------------------------------------------------

        #region Constructors
        /// <summary>
        /// Initializes a new instance of ObservableCollection that is empty and has default initial capacity.
        /// </summary>
        public ObservableCollection() : base() { }

        /// <summary>
        /// Initializes a new instance of the ObservableCollection class
        /// that contains elements copied from the specified list
        /// </summary>
        /// <param name="list">The list whose elements are copied to the new list.</param>
        /// <remarks>
        /// The elements are copied onto the ObservableCollection in the
        /// same order they are read by the enumerator of the list.
        /// </remarks>
        /// <exception cref="ArgumentNullException"> list is a null reference </exception>
        public ObservableCollection(List<T> list)
            : base((list != null) ? new List<T>(list.Count) : list)
        {
            // Workaround for VSWhidbey bug 562681 (tracked by Windows bug 1369339).
            // We should be able to simply call the base(list) ctor.  But Collection<T>
            // doesn't copy the list (contrary to the documentation) - it uses the
            // list directly as its storage.  So we do the copying here.
            // 
            CopyFrom(list);
        }

        /// <summary>
        /// Initializes a new instance of the ObservableCollection class that contains
        /// elements copied from the specified collection and has sufficient capacity
        /// to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        /// <remarks>
        /// The elements are copied onto the ObservableCollection in the
        /// same order they are read by the enumerator of the collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException"> collection is a null reference </exception>
        public ObservableCollection(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            CopyFrom(collection);
        }

        private void CopyFrom(IEnumerable<T> collection)
        {
            IList<T> items = Items;
            if (collection != null && items != null)
            {
                using (IEnumerator<T> enumerator = collection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        items.Add(enumerator.Current);
                    }
                }
            }
        }

        #endregion Constructors


        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        /// <summary>
        /// Move item at oldIndex to newIndex.
        /// </summary>
        public void Move(int oldIndex, int newIndex)
        {
            MoveItem(oldIndex, newIndex);
        }

        #endregion Public Methods


        //------------------------------------------------------
        //
        //  Public Events
        //
        //------------------------------------------------------

        #region Public Events


        //------------------------------------------------------
        /// <summary>
        /// Occurs when the collection changes, either by adding or removing an item.
        /// </summary>
        /// <remarks>
        /// see <seealso cref="INotifyCollectionChanged"/>
        /// </remarks>
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion Public Events


        //------------------------------------------------------
        //
        //  Protected Methods
        //
        //------------------------------------------------------

        #region Protected Methods

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when the list is being cleared;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void ClearItems()
        {
            CheckReentrancy();
            base.ClearItems();
            RaisePropertyChanged(CountString);
            RaisePropertyChanged(IndexerName);
            RaiseCollectionReset();
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is removed from list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void RemoveItem(int index)
        {
            CheckReentrancy();
            T removedItem = this[index];

            base.RemoveItem(index);

            RaisePropertyChanged(CountString);
            RaisePropertyChanged(IndexerName);
            RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, removedItem, index);
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is added to list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void InsertItem(int index, T item)
        {
            CheckReentrancy();
            base.InsertItem(index, item);

            RaisePropertyChanged(CountString);
            RaisePropertyChanged(IndexerName);
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is set in list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void SetItem(int index, T item)
        {
            CheckReentrancy();
            var originalItem = this[index];
            base.SetItem(index, item);

            RaisePropertyChanged(IndexerName);
            RaiseCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, item, index);
        }

        /// <summary>
        /// Called by base class ObservableCollection&lt;T&gt; when an item is to be moved within the list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            CheckReentrancy();

            var removedItem = this[oldIndex];

            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, removedItem);

            RaisePropertyChanged(IndexerName);
            RaiseCollectionChanged(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex);
        }


        /// <summary>
        /// Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        protected void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            var temp = this.PropertyChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() => temp(this, e));
        }

        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise CollectionChanged event to any listeners.
        /// Properties/methods modifying this ObservableCollection will raise
        /// a collection changed event through this virtual method.
        /// </summary>
        /// <remarks>
        /// When overriding this method, either call its base implementation
        /// or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
        /// </remarks>
        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var temp = this.CollectionChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() =>
            {
                using (BlockReentrancy())
                {
                    temp(this, e);
                }
            });
        }

        /// <summary>
        /// Disallow reentrant attempts to change this collection. E.g. a event handler
        /// of the CollectionChanged event is not allowed to make changes to this collection.
        /// </summary>
        /// <remarks>
        /// typical usage is to wrap e.g. a OnCollectionChanged call with a using() scope:
        /// <code>
        ///         using (BlockReentrancy())
        ///         {
        ///             CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));
        ///         }
        /// </code>
        /// </remarks>
        protected IDisposable BlockReentrancy()
        {
            _monitor.Enter();
            return _monitor;
        }

        /// <summary> Check and assert for reentrant attempts to change this collection. </summary>
        /// <exception cref="InvalidOperationException"> raised when changing the collection
        /// while another collection change is still being notified to other listeners </exception>
        protected void CheckReentrancy()
        {
            if (_monitor.Busy)
            {
                // we can allow changes if there's only one listener - the problem
                // only arises if reentrant changes make the original event args
                // invalid for later listeners.  This keeps existing code working
                // (e.g. Selector.SelectedItems).
                if ((CollectionChanged != null) && (CollectionChanged.GetInvocationList().Length > 1))
                    throw new InvalidOperationException("ObservableCollectionReentrancyNotAllowed");
            }
        }

        #endregion Protected Methods


        //------------------------------------------------------
        //
        //  Private Methods
        //
        //------------------------------------------------------

        #region Private Methods

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void RaiseCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void RaiseCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void RaiseCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event with action == Reset to any listeners
        /// </summary>
        private void RaiseCollectionReset()
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion Private Methods

        //------------------------------------------------------
        //
        //  Private Types
        //
        //------------------------------------------------------

        #region Private Types

        // this class helps prevent reentrant calls
        private class SimpleMonitor : IDisposable
        {
            public void Enter()
            {
                ++_busyCount;
            }

            public void Dispose()
            {
                --_busyCount;
            }

            public bool Busy { get { return _busyCount > 0; } }

            int _busyCount;
        }

        #endregion Private Types

        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields

        private const string CountString = "Count";

        // This must agree with Binding.IndexerName.  It is declared separately
        // here so as to avoid a dependency on PresentationFramework.dll.
        private const string IndexerName = "Item[]";

        private SimpleMonitor _monitor = new SimpleMonitor();

        #endregion Private Fields

        protected bool Set<TProp>(ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(field, value))
                return false;
            ForceSet(ref field, value, propertyName);
            return true;
        }

        protected bool Set<TProp>(string addtionalPropertyName, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(field, value))
                return false;
            ForceSet(addtionalPropertyName, ref field, value, propertyName);
            return true;
        }

        protected bool Set<TProp>(string addtionalPropertyName0, string addtionalPropertyName1, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(field, value))
                return false;
            ForceSet(addtionalPropertyName0, addtionalPropertyName1, ref field, value, propertyName);
            return true;
        }

        protected bool Set<TProp>(IEnumerable<string> addtionalPropertyNames, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(field, value))
                return false;
            ForceSet(addtionalPropertyNames, ref field, value, propertyName);
            return true;
        }

        protected void ForceSet<TProp>(ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            RaisePropertyChanged(propertyName);
        }

        protected void ForceSet<TProp>(string addtionalPropertyName, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            RaisePropertyChanged(propertyName, addtionalPropertyName);
        }

        protected void ForceSet<TProp>(string addtionalPropertyName0, string addtionalPropertyName1, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            RaisePropertyChanged(propertyName, addtionalPropertyName0, addtionalPropertyName1);
        }

        protected void ForceSet<TProp>(IEnumerable<string> addtionalPropertyNames, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            DispatcherHelper.BeginInvoke(() =>
            {
                RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
                foreach (var item in addtionalPropertyNames)
                {
                    RaisePropertyChanged(new PropertyChangedEventArgs(item));
                }
            });
        }

        /// <summary>
        /// Add items into collection.
        /// </summary>
        /// <param name="items">Items to add.</param>
        /// <returns>Count of added items.</returns>
        public int AddRange(IEnumerable<T> items)
        {
            CheckReentrancy();
            var count = 0;
            if (this.Items is List<T> list && items is ICollection<T> collection)
            {
                list.AddRange(collection);
                count = collection.Count;
            }
            else
            {
                foreach (var item in items)
                {
                    this.Items.Add(item);
                    count++;
                }
            }
            if (count == 0)
                return 0;
            var startingIndex = this.Count - count;
            this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new AddRangeInfo(this, startingIndex, count), startingIndex));
            RaisePropertyChanged(nameof(Count), "Item[]");
            return count;
        }

        private class AddRangeInfo : IList
        {
            private int count;
            private ObservableCollection<T> parent;
            private int startingIndex;

            public AddRangeInfo(ObservableCollection<T> parent, int startingIndex, int count)
            {
                this.parent = parent;
                this.startingIndex = startingIndex;
                this.count = count;
            }

            public object this[int index]
            {
                get
                {
                    if ((uint)index > (uint)this.count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    return this.parent[this.startingIndex + index];
                }
                set => throw new InvalidOperationException();
            }

            public int Count => this.count;

            public bool IsFixedSize => true;

            public bool IsReadOnly => true;

            public bool IsSynchronized => false;

            public object SyncRoot => null;

            public int Add(object value)
            {
                throw new InvalidOperationException();
            }

            public void Clear()
            {
                throw new InvalidOperationException();
            }

            public bool Contains(object value)
            {
                foreach (var item in this)
                {
                    if (item == value)
                        return true;
                }
                return false;
            }

            public void CopyTo(Array array, int index)
            {
                for (var i = 0; i < this.count; i++)
                {
                    array.SetValue(this[i], i);
                }
            }

            public IEnumerator GetEnumerator()
            {
                for (var i = 0; i < this.count; i++)
                {
                    yield return this[i];
                }
            }

            public int IndexOf(object value)
            {
                for (var i = 0; i < this.count; i++)
                {
                    if (this[i] == value)
                        return i;
                }
                return -1;
            }

            public void Insert(int index, object value)
            {
                throw new InvalidOperationException();
            }

            public void Remove(object value)
            {
                throw new InvalidOperationException();
            }

            public void RemoveAt(int index)
            {
                throw new InvalidOperationException();
            }
        }

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void RaisePropertyChanged(string propertyName0, string propertyName1)
        {
            DispatcherHelper.BeginInvoke(() =>
            {
                RaisePropertyChanged(new PropertyChangedEventArgs(propertyName0));
                RaisePropertyChanged(new PropertyChangedEventArgs(propertyName1));
            });
        }

        protected void RaisePropertyChanged(string propertyName0, string propertyName1, string propertyName2)
        {
            DispatcherHelper.BeginInvoke(() =>
            {
                RaisePropertyChanged(new PropertyChangedEventArgs(propertyName0));
                RaisePropertyChanged(new PropertyChangedEventArgs(propertyName1));
                RaisePropertyChanged(new PropertyChangedEventArgs(propertyName2));
            });
        }

        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            this.RaisePropertyChanged((IEnumerable<string>)propertyNames);
        }

        protected void RaisePropertyChanged(IEnumerable<string> propertyNames)
        {
            DispatcherHelper.BeginInvoke(() =>
            {
                foreach (var item in propertyNames)
                {
                    RaisePropertyChanged(new PropertyChangedEventArgs(item));
                }
            });
        }
    }
}
