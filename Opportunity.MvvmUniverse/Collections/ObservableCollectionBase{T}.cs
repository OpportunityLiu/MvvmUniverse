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
        /// Call <see cref="ObservableObject.OnObjectReset()"/> and <see cref="OnVectorReset()"/>.
        /// </summary>
        public override void OnObjectReset()
        {
            base.OnObjectReset();
            OnVectorReset();
        }

        /// <summary>
        /// Tell caller of <see cref="OnVectorChanged(IVectorChangedEventArgs)"/> that whether this call can be skipped.
        /// <para></para>
        /// Returns <see langword="false"/> if <see cref="ObservableObject.SuspendNotification(bool)"/> has been called
        /// or <see cref="VectorChanged"/> is not registed.
        /// </summary>
        protected virtual bool NeedRaiseVectorChanged => this.vectorChanged.InvocationListLength != 0 && !NotificationSuspending;

        private readonly DepedencyEvent<Handler, ObservableCollectionBase<T>, IVectorChangedEventArgs> vectorChanged
            = new DepedencyEvent<Handler, ObservableCollectionBase<T>, IVectorChangedEventArgs>((h, s, e) => h(s, e));
        /// <inheritdoc/>
        public event Handler VectorChanged
        {
            add => this.vectorChanged.Add(value);
            remove => this.vectorChanged.Remove(value);
        }

        /// <summary>
        /// Raise <see cref="VectorChanged"/> event
        /// if <see cref="NeedRaiseVectorChanged"/> is <see langword="true"/>.
        /// </summary>
        /// <param name="args">Event args.</param>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> is <see langword="null"/></exception>
        protected virtual void OnVectorChanged(IVectorChangedEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (!NeedRaiseVectorChanged)
                return;
            this.vectorChanged.Raise(this, args);
        }

        /// <summary>
        /// Raise <see cref="VectorChanged"/> event of <see cref="Action.Reset"/>.
        /// </summary>
        public void OnVectorReset()
        {
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
        internal int CountInternal
        {
            get
            {
                if (this is ICollection<T> col)
                    return col.Count;
                return ((IReadOnlyCollection<T>)this).Count;
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int ICollection.Count => CountInternal;

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
                ((IEnumerable<T>)this).CopyTo(tarr, index);
            else if (array is object[] oarr)
                ((IEnumerable<T>)this).Cast<object>().CopyTo(oarr, index);
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
            if (TryCastValue(value, out T item))
                return ((IEnumerable<T>)this).Contains(item);
            return false;
        }

        int IList.IndexOf(object value)
        {
            if (TryCastValue(value, out T item))
                return ((IEnumerable<T>)this).IndexOf(item);
            return -1;
        }

        void IList.Insert(int index, object value)
        {
            if (IsReadOnlyInternal) ThrowForReadOnlyCollection();
            if (IsFixedSizeInternal) ThrowForFixedSizeCollection();
            ((IList<T>)this).Insert(index, CastValue<T>(value));
        }

        void IList.Remove(object value)
        {
            if (IsReadOnlyInternal) ThrowForReadOnlyCollection();
            if (IsFixedSizeInternal) ThrowForFixedSizeCollection();
            ((ICollection<T>)this).Remove(CastValue<T>(value));
        }

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
        object ICollection.SyncRoot => this.vectorChanged;

        /// <summary>
        /// Create view of current collection.
        /// </summary>
        /// <returns>Default view of current collection.</returns>
        public virtual CollectionView<T> CreateView() => new CollectionView<T>(this);

        ICollectionView ICollectionViewFactory.CreateView() => CreateView();
    }
}
