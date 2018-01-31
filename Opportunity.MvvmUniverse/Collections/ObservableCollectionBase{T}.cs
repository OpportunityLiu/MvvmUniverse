using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Action = System.Collections.Specialized.NotifyCollectionChangedAction;
using Handler = System.Collections.Specialized.NotifyCollectionChangedEventHandler;
using Args = System.Collections.Specialized.NotifyCollectionChangedEventArgs;

namespace Opportunity.MvvmUniverse.Collections
{
    // Make it strong typed to add type check and decrease boxing
    public abstract class ObservableCollectionBase<T> : ObservableObject, System.Collections.Specialized.INotifyCollectionChanged
    {
        private sealed class ListWarpper : IList, IReadOnlyList<T>, ICollection, IReadOnlyCollection<T>
        {
            public static IList WarpIfNeeded(IReadOnlyList<T> listObj) => (listObj as IList) ?? new ListWarpper(listObj);

            private readonly IReadOnlyList<T> list;
            public ListWarpper(IReadOnlyList<T> toWarp) => this.list = toWarp;

            object IList.this[int index]
            {
                get => this.list[index];
                set => throw new NotSupportedException("This is a read only list");
            }

            public bool IsFixedSize => false;
            public bool IsReadOnly => true;

            public int Count => this.list.Count;

            public bool IsSynchronized => false;

            private object syncRoot;
            public object SyncRoot
            {
                get
                {
                    if (this.syncRoot == null)
                        System.Threading.Interlocked.CompareExchange(ref this.syncRoot, new object(), null);
                    return this.syncRoot;
                }
            }

            public T this[int index] => this.list[index];

            public int Add(object value) => throw new NotSupportedException("This is a read only list");

            public void Clear() => throw new NotSupportedException("This is a read only list");

            public bool Contains(object value) => this.list.Contains((T)value);

            public void CopyTo(Array array, int index)
            {
                if (array is T[] a)
                {
                    if (this.list is IList<T> l)
                    {
                        l.CopyTo(a, index);
                        return;
                    }
                    if (this.list.Count + index > a.Length)
                        throw new ArgumentException("Not enough space in array");
                    for (var i = 0; i < this.list.Count; i++)
                    {
                        a[index + i] = this.list[i];
                    }
                    return;
                }
                throw new ArgumentException("Unsupported array", nameof(array));
            }

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.list).GetEnumerator();
            public IEnumerator<T> GetEnumerator() => this.list.GetEnumerator();

            public int IndexOf(object value)
            {
                var v = (T)value;
                if (this.list is IList<T> l)
                    return l.IndexOf(v);
                var c = EqualityComparer<T>.Default;
                for (var i = 0; i < this.list.Count; i++)
                {
                    if (c.Equals(v, this.list[i]))
                        return i;
                }
                return -1;
            }

            public void Insert(int index, object value) => Internal.Helpers.ThrowForReadOnlyCollection(this.list);
            public void Remove(object value) => Internal.Helpers.ThrowForReadOnlyCollection(this.list);
            public void RemoveAt(int index) => Internal.Helpers.ThrowForReadOnlyCollection(this.list);
        }

        public event Handler CollectionChanged;

        protected virtual void OnCollectionChanged(Args args)
        {
            var temp = CollectionChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() =>
            {
                temp.Invoke(this, args);
            });
        }

        protected void OnCollectionReset()
        {
            if (CollectionChanged == null)
                return;
            OnCollectionChanged(new Args(Action.Reset));
        }

        protected void OnCollectionMove(T item, int newIndex, int oldIndex)
        {
            if (CollectionChanged == null)
                return;
            OnCollectionChanged(new Args(Action.Move, item, newIndex, oldIndex));
        }

        protected void OnCollectionMove(IReadOnlyList<T> items, int newIndex, int oldIndex)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Count == 1)
                OnCollectionMove(items[0], newIndex, oldIndex);
            if (CollectionChanged == null)
                return;
            OnCollectionChanged(new Args(Action.Move, ListWarpper.WarpIfNeeded(items), newIndex, oldIndex));
        }

        protected void OnCollectionAdd(T item, int index)
        {
            if (CollectionChanged == null)
                return;
            OnCollectionChanged(new Args(Action.Add, item, index));
        }

        protected void OnCollectionAdd(IReadOnlyList<T> items, int index)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Count == 1)
                OnCollectionAdd(items[0], index);
            if (CollectionChanged == null)
                return;
            OnCollectionChanged(new Args(Action.Add, ListWarpper.WarpIfNeeded(items), index));
        }

        protected void OnCollectionRemove(T item, int index)
        {
            if (CollectionChanged == null)
                return;
            OnCollectionChanged(new Args(Action.Remove, item, index));
        }

        protected void OnCollectionRemove(IReadOnlyList<T> items, int index)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Count == 1)
                OnCollectionRemove(items[0], index);
            if (CollectionChanged == null)
                return;
            OnCollectionChanged(new Args(Action.Remove, ListWarpper.WarpIfNeeded(items), index));
        }

        protected void OnCollectionReplace(T newItem, T oldItem, int index)
        {
            if (CollectionChanged == null)
                return;
            OnCollectionChanged(new Args(Action.Replace, newItem, oldItem, index));
        }

        protected void OnCollectionReplace(IReadOnlyList<T> newItems, IReadOnlyList<T> oldItems, int index)
        {
            if (newItems == null)
                throw new ArgumentNullException(nameof(newItems));
            if (oldItems == null)
                throw new ArgumentNullException(nameof(oldItems));
            if (newItems.Count == 1 && oldItems.Count == 1)
                OnCollectionReplace(newItems[0], oldItems[0], index);
            if (CollectionChanged == null)
                return;
            OnCollectionChanged(new Args(Action.Replace, ListWarpper.WarpIfNeeded(newItems), ListWarpper.WarpIfNeeded(oldItems), index));
        }
    }
}
