using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Action = Windows.Foundation.Collections.CollectionChange;
using Handler = Windows.UI.Xaml.Interop.BindableVectorChangedEventHandler;
using Args = Opportunity.MvvmUniverse.Collections.VectorChangedEventArgs;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Interop;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// Base class for observable collections. Derived class must implement <see cref="IList{T}"/> or <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of objects store in the collection</typeparam>
    public abstract class ObservableCollectionBase<T> : ObservableObject
        , IBindableObservableVector, IList, ICollection, IEnumerable, ICollectionViewFactory
    {
        /// <summary>
        /// Create new instance of <see cref="ObservableCollectionBase{T}"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Derived class does not implement <see cref="IList{T}"/> or <see cref="IReadOnlyList{T}"/>.
        /// </exception>
        protected ObservableCollectionBase()
        {
            if (!(this is IList<T>) && !(this is IReadOnlyList<T>))
                throw new InvalidOperationException("Derived class must implement IList<T> or IReadOnlyList<T>");
        }

        /// <summary>
        /// Tell caller of <see cref="OnVectorChanged(IVectorChangedEventArgs)"/> that whether this call can be skipped.
        /// <para></para>
        /// Returns <c><see cref="VectorChanged"/> != <see langword="null"/></c> by default.
        /// </summary>
        protected virtual bool NeedRaiseVectorChanged => this.vectorChanged.InvocationListLength != 0;

        private readonly DepedencyEvent<Handler, ObservableCollectionBase<T>, IVectorChangedEventArgs> vectorChanged
            = new DepedencyEvent<Handler, ObservableCollectionBase<T>, IVectorChangedEventArgs>((h, s, e) => h(s, e));
        /// <inheritdoc/>
        public event Handler VectorChanged
        {
            add => this.vectorChanged.Add(value);
            remove => this.vectorChanged.Remove(value);
        }

        /// <summary>
        /// Raise <see cref="VectorChanged"/> event.
        /// </summary>
        /// <param name="args">Event args.</param>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> is <see langword="null"/></exception>
        /// <remarks>Will use <see cref="DispatcherHelper"/> to raise event on UI thread
        /// if <see cref="DispatcherHelper.UseForNotification"/> is <see langword="true"/>.</remarks>
        protected virtual void OnVectorChanged(IVectorChangedEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (this.vectorChanged.InvocationListLength == 0)
                return;
            this.vectorChanged.Raise(this, args);
        }

        /// <summary>
        /// Raise <see cref="VectorChanged"/> event of <see cref="Action.Reset"/>.
        /// </summary>
        public void OnVectorReset()
        {
            if (!NeedRaiseVectorChanged)
                return;
            OnVectorChanged(Args.Reset);
        }

        /// <summary>
        /// Raise <see cref="VectorChanged"/> event of <see cref="Action.ItemInserted"/>.
        /// </summary>
        /// <param name="index">Index of inserted item.</param>
        protected void OnItemInserted(int index)
        {
            if (!NeedRaiseVectorChanged)
                return;
            OnVectorChanged(new Args(Action.ItemInserted, (uint)index));
        }

        /// <summary>
        /// Raise <see cref="VectorChanged"/> event of <see cref="Action.ItemRemoved"/>.
        /// </summary>
        /// <param name="index">Index of removed item.</param>
        protected void OnItemRemoved(int index)
        {
            if (!NeedRaiseVectorChanged)
                return;
            OnVectorChanged(new Args(Action.ItemRemoved, (uint)index));
        }

        /// <summary>
        /// Raise <see cref="VectorChanged"/> event of <see cref="Action.ItemChanged"/>.
        /// </summary>
        /// <param name="index">Index of changed item.</param>
        protected void OnItemChanged(int index)
        {
            if (!NeedRaiseVectorChanged)
                return;
            OnVectorChanged(new Args(Action.ItemChanged, (uint)index));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal bool IsReadOnlyInternal => ((IList)this).IsReadOnly;
        // Derived class can override this value.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsReadOnly => (this is ICollection<T> c) ? c.IsReadOnly : true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal bool IsFixedSizeInternal => ((IList)this).IsFixedSize;
        // Derived class can override this value.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsFixedSize => false;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal int CountInternal => ((ICollection)this).Count;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int ICollection.Count
        {
            get
            {
                if (this is ICollection<T> col)
                    return col.Count;
                return ((IReadOnlyCollection<T>)this).Count;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object IList.this[int index]
        {
            get
            {
                if (this is IList<T> list)
                    return list[index];
                return ((IReadOnlyList<T>)this)[index];
            }
            set
            {
                if (IsReadOnlyInternal) ThrowForReadOnlyCollection();
                ((IList<T>)this)[index] = CastValue<T>(value);
            }
        }

        int IList.Add(object value)
        {
            if (IsReadOnlyInternal) ThrowForReadOnlyCollection();
            if (IsFixedSizeInternal) ThrowForFixedSizeCollection();
            var that = (ICollection<T>)this;
            that.Add(CastValue<T>(value));
            return that.Count - 1;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (array is T[] tarr)
            {
                if (this is ICollection<T> col)
                {
                    col.CopyTo(tarr, index);
                }
                else
                {
                    var rcol = (IReadOnlyCollection<T>)this;
                    if (index + rcol.Count > tarr.Length)
                        throw new ArgumentException("Not enough space for copying.");
                    foreach (var item in rcol)
                    {
                        tarr[index] = item;
                        index++;
                    }
                }
            }
            else if (array is object[] oarr)
            {
                if (((ICollection)this).Count + index > array.Length)
                    throw new ArgumentException("Not enough space for copying.");
                foreach (var item in this)
                {
                    oarr[index] = item;
                    index++;
                }
            }
            else
                throw new ArgumentException("Wrong type of array.", nameof(array));
        }

        void IList.Clear()
        {
            if (IsReadOnlyInternal) ThrowForReadOnlyCollection();
            if (IsFixedSizeInternal) ThrowForFixedSizeCollection();
            ((ICollection<T>)this).Clear();
        }

        bool IList.Contains(object value)
        {
            try
            {
                var item = CastValue<T>(value);
                if (this is ICollection<T> col)
                    return col.Contains(item);
                var rcol = (IReadOnlyList<T>)this;
                return rcol.Contains(item);
            }
            catch (Exception)
            {
                return false;
            }
        }

        int IList.IndexOf(object value)
        {
            try
            {
                var item = CastValue<T>(value);
                if (this is IList<T> col)
                    return col.IndexOf(item);
                var rcol = (IReadOnlyList<T>)this;
                var ind = 0;
                foreach (var i in rcol)
                {
                    if (EqualityComparer<T>.Default.Equals(i, item))
                        return ind;
                    ind++;
                }
                return -1;
            }
            catch
            {
                return -1;
            }
        }

        void IList.Insert(int index, object value)
        {
            if (IsReadOnlyInternal) ThrowForReadOnlyCollection();
            if (IsFixedSizeInternal) ThrowForFixedSizeCollection();
            ((IList<T>)this).Insert(index, CastValue<T>(value));
        }

        internal bool Remove(object value)
        {
            if (IsReadOnlyInternal) ThrowForReadOnlyCollection();
            if (IsFixedSizeInternal) ThrowForFixedSizeCollection();
            return ((ICollection<T>)this).Remove(CastValue<T>(value));
        }
        void IList.Remove(object value) => Remove(value);

        void IList.RemoveAt(int index)
        {
            if (IsReadOnlyInternal) ThrowForReadOnlyCollection();
            if (IsFixedSizeInternal) ThrowForFixedSizeCollection();
            ((IList<T>)this).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => false;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object syncRoot;
        object ICollection.SyncRoot
        {
            get
            {
                if (this.syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange<object>(ref this.syncRoot, new object(), null);
                }
                return this.syncRoot;
            }
        }

        /// <summary>
        /// Create view of current collection.
        /// </summary>
        /// <returns>Default view of current collection.</returns>
        public virtual CollectionView<T> CreateView() => new CollectionView<T>(this);

        ICollectionView ICollectionViewFactory.CreateView() => CreateView();
    }
}
