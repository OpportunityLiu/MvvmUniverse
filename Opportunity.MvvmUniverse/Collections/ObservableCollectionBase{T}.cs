using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Action = System.Collections.Specialized.NotifyCollectionChangedAction;
using Handler = System.Collections.Specialized.NotifyCollectionChangedEventHandler;
using Args = System.Collections.Specialized.NotifyCollectionChangedEventArgs;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// Base class for observable collections.
    /// </summary>
    /// <typeparam name="T">type of objects store in the collection</typeparam>
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
                set => Internal.Helpers.ThrowForReadOnlyCollection(this.list);
            }

            public bool IsFixedSize => false;
            public bool IsReadOnly => true;

            public int Count => this.list.Count;

            public bool IsSynchronized => false;

            public object SyncRoot => this.list;

            public T this[int index] => this.list[index];

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

            public int Add(object value) => Internal.Helpers.ThrowForReadOnlyCollection<int>(this.list);
            public void Clear() => Internal.Helpers.ThrowForReadOnlyCollection(this.list);
            public void Insert(int index, object value) => Internal.Helpers.ThrowForReadOnlyCollection(this.list);
            public void Remove(object value) => Internal.Helpers.ThrowForReadOnlyCollection(this.list);
            public void RemoveAt(int index) => Internal.Helpers.ThrowForReadOnlyCollection(this.list);
        }

        /// <summary>
        /// Tell caller of <see cref="OnCollectionChanged(Args)"/> that whether this call can be skipped.
        /// Returns <c><see cref="CollectionChanged"/> != null</c> by default.
        /// </summary>
        protected virtual bool NeedRaiseCollectionChanged => CollectionChanged != null;

        /// <inheritdoc/>
        public event Handler CollectionChanged;

        /// <summary>
        /// Raise <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="args">event args</param>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> is <see langword="null"/></exception>
        /// <remarks>Will use <see cref="DispatcherHelper"/> to raise event on UI thread
        /// if <see cref="DispatcherHelper.UseForNotification"/> is <see langword="true"/>.</remarks>
        protected virtual void OnCollectionChanged(Args args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            var temp = CollectionChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() => temp(this, args));
        }

        /// <summary>
        /// Raise <see cref="CollectionChanged"/> event of <see cref="Action.Reset"/>.
        /// </summary>
        protected void OnCollectionReset()
        {
            if (!NeedRaiseCollectionChanged)
                return;
            OnCollectionChanged(new Args(Action.Reset));
        }

        /// <summary>
        /// Raise <see cref="CollectionChanged"/> event of <see cref="Action.Move"/>.
        /// </summary>
        /// <param name="item">moved item</param>
        /// <param name="newIndex">new index of <paramref name="item"/></param>
        /// <param name="oldIndex">old index of <paramref name="item"/></param>
        protected void OnCollectionMove(T item, int newIndex, int oldIndex)
        {
            if (!NeedRaiseCollectionChanged)
                return;
            OnCollectionChanged(new Args(Action.Move, item, newIndex, oldIndex));
        }

        /// <summary>
        /// Raise <see cref="CollectionChanged"/> event of <see cref="Action.Move"/>.
        /// </summary>
        /// <param name="items">moved items</param>
        /// <param name="newIndex">new index of <paramref name="items"/></param>
        /// <param name="oldIndex">old index of <paramref name="items"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/></exception>
        protected void OnCollectionMove(IReadOnlyList<T> items, int newIndex, int oldIndex)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Count == 1)
            {
                OnCollectionMove(items[0], newIndex, oldIndex);
                return;
            }
            if (!NeedRaiseCollectionChanged)
                return;
            OnCollectionChanged(new Args(Action.Move, ListWarpper.WarpIfNeeded(items), newIndex, oldIndex));
        }

        /// <summary>
        /// Raise <see cref="CollectionChanged"/> event of <see cref="Action.Add"/>.
        /// </summary>
        /// <param name="item">added item</param>
        /// <param name="index">index of <paramref name="item"/></param>
        protected void OnCollectionAdd(T item, int index)
        {
            if (!NeedRaiseCollectionChanged)
                return;
            OnCollectionChanged(new Args(Action.Add, item, index));
        }

        /// <summary>
        /// Raise <see cref="CollectionChanged"/> event of <see cref="Action.Add"/>.
        /// </summary>
        /// <param name="items">added items</param>
        /// <param name="index">index of <paramref name="items"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/></exception>
        protected void OnCollectionAdd(IReadOnlyList<T> items, int index)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Count == 1)
            {
                OnCollectionAdd(items[0], index);
                return;
            }
            if (!NeedRaiseCollectionChanged)
                return;
            OnCollectionChanged(new Args(Action.Add, ListWarpper.WarpIfNeeded(items), index));
        }

        /// <summary>
        /// Raise <see cref="CollectionChanged"/> event of <see cref="Action.Remove"/>.
        /// </summary>
        /// <param name="item">removed item</param>
        /// <param name="index">original index of <paramref name="item"/></param>
        protected void OnCollectionRemove(T item, int index)
        {
            if (!NeedRaiseCollectionChanged)
                return;
            OnCollectionChanged(new Args(Action.Remove, item, index));
        }

        /// <summary>
        /// Raise <see cref="CollectionChanged"/> event of <see cref="Action.Remove"/>.
        /// </summary>
        /// <param name="items">removed items</param>
        /// <param name="index">original index of <paramref name="items"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/></exception>
        protected void OnCollectionRemove(IReadOnlyList<T> items, int index)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Count == 1)
            {
                OnCollectionRemove(items[0], index);
                return;
            }
            if (!NeedRaiseCollectionChanged)
                return;
            OnCollectionChanged(new Args(Action.Remove, ListWarpper.WarpIfNeeded(items), index));
        }

        /// <summary>
        /// Raise <see cref="CollectionChanged"/> event of <see cref="Action.Replace"/>.
        /// </summary>
        /// <param name="newItem">new item</param>
        /// <param name="oldItem">replaced item</param>
        /// <param name="index">index of item</param>
        protected void OnCollectionReplace(T newItem, T oldItem, int index)
        {
            if (!NeedRaiseCollectionChanged)
                return;
            OnCollectionChanged(new Args(Action.Replace, newItem, oldItem, index));
        }

        /// <summary>
        /// Raise <see cref="CollectionChanged"/> event of <see cref="Action.Replace"/>.
        /// </summary>
        /// <param name="newItems">new items</param>
        /// <param name="oldItems">replaced items</param>
        /// <param name="index">index of items</param>
        /// <exception cref="ArgumentNullException"><paramref name="newItems"/> or <paramref name="oldItems"/> is <see langword="null"/></exception>
        protected void OnCollectionReplace(IReadOnlyList<T> newItems, IReadOnlyList<T> oldItems, int index)
        {
            if (newItems == null)
                throw new ArgumentNullException(nameof(newItems));
            if (oldItems == null)
                throw new ArgumentNullException(nameof(oldItems));
            var oldCount = oldItems.Count;
            var newCount = newItems.Count;
            if (oldCount <= 0)
            {
                OnCollectionAdd(newItems, index);
                return;
            }
            if (newCount <= 0)
            {
                OnCollectionRemove(oldItems, index);
                return;
            }
            if (newCount == 1 && oldCount == 1)
            {
                OnCollectionReplace(newItems[0], oldItems[0], index);
                return;
            }
            if (!NeedRaiseCollectionChanged)
                return;
            OnCollectionChanged(new Args(Action.Replace, ListWarpper.WarpIfNeeded(newItems), ListWarpper.WarpIfNeeded(oldItems), index));
        }
    }
}
