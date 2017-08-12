﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Interop;

namespace Opportunity.MvvmUniverse.Collections
{
    // Make it strong typed to add type check and decrease boxing
    public abstract class ObservableCollectionBase<T> : ObservableObject, INotifyCollectionChanged
    {
        private sealed class ListWarpper : IList
        {
            private readonly IReadOnlyList<T> list;
            public ListWarpper(IReadOnlyList<T> toWarp)
            {
                this.list = toWarp;
            }

            public object this[int index]
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

            public IEnumerator GetEnumerator() => this.list.GetEnumerator();

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

            public void Insert(int index, object value) => throw new NotSupportedException("This is a read only list");

            public void Remove(object value) => throw new NotSupportedException("This is a read only list");

            public void RemoveAt(int index) => throw new NotSupportedException("This is a read only list");
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            var temp = CollectionChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() =>
            {
                temp.Invoke(this, args);
            });
        }

        protected void RaiseCollectionReset()
        {
            if (CollectionChanged == null)
                return;
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected void RaiseCollectionMove(T item, int newIndex, int oldIndex)
        {
            if (CollectionChanged == null)
                return;
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }

        protected void RaiseCollectionMove(IReadOnlyList<T> items, int newIndex, int oldIndex)
        {
            if (CollectionChanged == null)
                return;
            if (!(items is IList list))
                list = new ListWarpper(items);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, list, newIndex, oldIndex));
        }

        protected void RaiseCollectionAdd(T item, int index)
        {
            if (CollectionChanged == null)
                return;
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected void RaiseCollectionAdd(IReadOnlyList<T> items, int index)
        {
            if (CollectionChanged == null)
                return;
            if (!(items is IList list))
                list = new ListWarpper(items);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list, index));
        }

        protected void RaiseCollectionRemove(T item, int index)
        {
            if (CollectionChanged == null)
                return;
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        protected void RaiseCollectionRemove(IReadOnlyList<T> items, int index)
        {
            if (CollectionChanged == null)
                return;
            if (!(items is IList list))
                list = new ListWarpper(items);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, list, index));
        }

        protected void RaiseCollectionReplace(T newItem, T oldItem, int index)
        {
            if (CollectionChanged == null)
                return;
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
        }

        protected void RaiseCollectionReplace(IReadOnlyList<T> newItems, IReadOnlyList<T> oldItems, int index)
        {
            if (CollectionChanged == null)
                return;
            if (!(newItems is IList newList))
                newList = new ListWarpper(newItems);
            if (!(oldItems is IList oldList))
                oldList = new ListWarpper(oldItems);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newList, oldList, index));
        }
    }
}
