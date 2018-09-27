using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Interop;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// View that supports current item, grouping and so on.
    /// </summary>
    /// <typeparam name="T">Type of elements.</typeparam>
    [DebuggerDisplay(@"Current = {CurrentPosition}/{Count}, IsCurrentPositionLocked = {IsCurrentPositionLocked}")]
    public class CollectionView<T> : ObservableObject, ICollectionView, ISupportIncrementalLoading
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ObservableCollectionBase<T> source;
        /// <summary>
        /// Source of data.
        /// </summary>
        public ObservableCollectionBase<T> Source => this.source;

        /// <summary>
        /// Create new instance of <see cref="CollectionView{T}"/>
        /// </summary>
        /// <param name="source">Source of data.</param>
        public CollectionView(ObservableCollectionBase<T> source)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            source.VectorChanged += WeakDelegate.Create<BindableVectorChangedEventHandler>(this.Source_VectorChanged);
            source.PropertyChanged += WeakDelegate.Create<PropertyChangedEventHandler>(this.Source_PropertyChanged);
        }

        /// <summary>
        /// Raise <see cref="VectorChanged"/> and <see cref="ObservableObject.OnObjectReset()"/>.
        /// </summary>
        public override void OnObjectReset()
        {
            if (NotificationSuspending)
                return;
            this.vectorChanged.Raise(this, VectorChangedEventArgs.Reset);
            base.OnObjectReset();
        }

        private readonly DepedencyEvent<VectorChangedEventHandler<object>, CollectionView<T>, IVectorChangedEventArgs> vectorChanged
            = new DepedencyEvent<VectorChangedEventHandler<object>, CollectionView<T>, IVectorChangedEventArgs>((h, s, e) => h(s, e));
        /// <inheritdoc/>
        public event VectorChangedEventHandler<object> VectorChanged
        {
            add => this.vectorChanged.Add(value);
            remove => this.vectorChanged.Remove(value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool isCurrentPositionLocked;
        /// <summary>
        /// Set to <see langword="true"/> to avoid modification by MoveCurrent* methods.
        /// </summary>
        public bool IsCurrentPositionLocked
        {
            get => this.isCurrentPositionLocked;
            set => Set(ref this.isCurrentPositionLocked, value);
        }

        private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnSourcePropertyChanged(e);
        }

        /// <summary>
        /// Event handler for <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/> of <see cref="Source"/>.
        /// </summary>
        /// <param name="e">Event args.</param>
        protected virtual void OnSourcePropertyChanged(PropertyChangedEventArgs e)
        {
            if (!NeedRaisePropertyChanged)
                return;
            if (e.PropertyName.IsNullOrWhiteSpace())
            {
                OnPropertyChanged(nameof(HasMoreItems), nameof(Count));
                return;
            }
            switch (e.PropertyName)
            {
            case nameof(Count):
            case nameof(HasMoreItems):
                OnPropertyChanged(e.PropertyName);
                break;
            }
        }

        private void Source_VectorChanged(IBindableObservableVector vector, object e)
        {
            var arg = (IVectorChangedEventArgs)e;
            OnSourceVectorChanged(arg);
        }

        /// <summary>
        /// Event handler for <see cref="IBindableObservableVector.VectorChanged"/> of <see cref="Source"/>.
        /// </summary>
        /// <param name="e">Event args.</param>
        protected virtual void OnSourceVectorChanged(IVectorChangedEventArgs e)
        {
            if (!NotificationSuspending)
                this.vectorChanged.Raise(this, e);
            switch (e.CollectionChange)
            {
            case CollectionChange.ItemInserted:
                if (e.Index <= this.currentPosition)
                    MoveCurrentToPosition(this.currentPosition + 1, false);
                break;
            case CollectionChange.ItemRemoved:
                if (e.Index <= this.currentPosition)
                    MoveCurrentToPosition(this.currentPosition - 1, false);
                break;
            case CollectionChange.ItemChanged:
                if (e.Index == this.currentPosition)
                    MoveCurrentToPosition(this.currentPosition, false);
                break;
            default:
                MoveCurrentToPosition(this.currentPosition, false);
                break;
            }
        }

        /// <inheritdoc/>
        public bool MoveCurrentTo(T item) => MoveCurrentToPosition(IndexOf(item));
        bool ICollectionView.MoveCurrentTo(object item) => MoveCurrentToPosition(((IList<object>)this).IndexOf(item));
        /// <inheritdoc/>
        public bool MoveCurrentToPosition(int index) => MoveCurrentToPosition(index, true);

        /// <summary>
        /// Move current item to position.
        /// </summary>
        /// <param name="index">Index of item to move to.</param>
        /// <param name="isCancelable">Can this operation be cancelled.</param>
        /// <returns><see langword="true"/> if the operation is not cancelled, and the <paramref name="index"/> is in the view.</returns>
        protected virtual bool MoveCurrentToPosition(int index, bool isCancelable)
        {
            var oldI = this.currentPosition;
            if (OnCurrentChanging(isCancelable, oldI, index))
                return false;
            if (index < 0)
            {
                CurrentPosition = -1;
                OnCurrentChanged(oldI, index);
                return false;
            }
            else if (index >= Count)
            {
                CurrentPosition = Count;
                OnCurrentChanged(oldI, index);
                return false;
            }
            else
            {
                CurrentPosition = index;
                OnCurrentChanged(oldI, index);
                return true;
            }
        }

        /// <inheritdoc/>
        public bool MoveCurrentToFirst() => MoveCurrentToPosition(0);
        /// <inheritdoc/>
        public bool MoveCurrentToLast() => MoveCurrentToPosition(this.Source.CountInternal - 1);
        /// <inheritdoc/>
        public bool MoveCurrentToNext() => MoveCurrentToPosition(this.currentPosition + 1);
        /// <inheritdoc/>
        public bool MoveCurrentToPrevious() => MoveCurrentToPosition(this.currentPosition - 1);

        /// <inheritdoc/>
        public bool HasMoreItems => (this.Source as ISupportIncrementalLoading)?.HasMoreItems == true;
        /// <inheritdoc/>
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (this.Source is ISupportIncrementalLoading ll)
                return ll.LoadMoreItemsAsync(count);
            else
                return AsyncOperation<LoadMoreItemsResult>.CreateCompleted();
        }

        /// <inheritdoc/>
        public virtual IObservableVector<object> CollectionGroups => null;

        /// <inheritdoc/>
        public bool IsCurrentAfterLast => this.currentPosition >= this.Source.CountInternal;
        /// <inheritdoc/>
        public bool IsCurrentBeforeFirst => this.currentPosition < 0;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int currentPosition;
        /// <inheritdoc/>
        public int CurrentPosition
        {
            get => this.currentPosition;
            private set => Set(currentAddtionalPropertyNames, ref this.currentPosition, value);
        }
        private static readonly string[] currentAddtionalPropertyNames
            = new[] { nameof(IsCurrentAfterLast), nameof(IsCurrentBeforeFirst), nameof(CurrentItem) };

        /// <summary>
        /// Get item at <paramref name="position"/>, will return default value if out of range.
        /// </summary>
        /// <param name="position">Index of item, can be out of range.</param>
        /// <returns>Item at <paramref name="position"/>, will return default value if out of range.</returns>
        protected T GetItemAt(int position)
        {
            if ((uint)position >= (uint)Count)
                return default;
            return this[position];
        }

        /// <inheritdoc/>
        public T CurrentItem => GetItemAt(this.currentPosition);
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollectionView.CurrentItem => (IsCurrentAfterLast || IsCurrentBeforeFirst) ? null : (object)this.CurrentItem;

        private readonly DepedencyEvent<EventHandler<object>, CollectionView<T>, object> currentChanged
            = new DepedencyEvent<EventHandler<object>, CollectionView<T>, object>((h, s, e) => h(s, e));
        /// <summary>
        /// Will be raised after current changed, event args is of type <see cref="CurrentChangedEventArgs{T}"/>.
        /// </summary>
        public event EventHandler<object> CurrentChanged
        {
            add => this.currentChanged.Add(value);
            remove => this.currentChanged.Remove(value);
        }

        /// <summary>
        /// Raise <see cref="CurrentChanged"/>.
        /// </summary>
        /// <param name="oldPosition">Old value of <see cref="CurrentPosition"/>.</param>
        /// <param name="newPosition">New value of <see cref="CurrentPosition"/>.</param>
        protected void OnCurrentChanged(int oldPosition, int newPosition)
        {
            if (this.currentChanged.InvocationListLength > 0)
                this.currentChanged.Raise(this, new CurrentChangedEventArgs<T>(oldPosition, GetItemAt(oldPosition), newPosition, GetItemAt(newPosition)));
        }

        private readonly DepedencyEvent<CurrentChangingEventHandler, CollectionView<T>, CurrentChangingEventArgs> currentChanging
            = new DepedencyEvent<CurrentChangingEventHandler, CollectionView<T>, CurrentChangingEventArgs>((h, s, e) => h(s, e));
        /// <summary>
        /// Will be raised before current changed, event args is of type <see cref="CurrentChangingEventArgs{T}"/>.
        /// </summary>
        public event CurrentChangingEventHandler CurrentChanging
        {
            add => this.currentChanging.Add(value);
            remove => this.currentChanging.Remove(value);
        }

        /// <summary>
        /// Raise <see cref="CurrentChanging"/>.
        /// </summary>
        /// <param name="isCancelable">Can this operation be cancelled.</param>
        /// <returns>Indicates this operation was cancelled or not.</returns>
        /// <param name="oldPosition">Old value of <see cref="CurrentPosition"/>.</param>
        /// <param name="newPosition">New value of <see cref="CurrentPosition"/>.</param>
        protected bool OnCurrentChanging(bool isCancelable, int oldPosition, int newPosition)
        {
            if (isCancelable && this.isCurrentPositionLocked)
                return true;
            if (this.currentChanging.InvocationListLength == 0)
                return false;
            var arg = new CurrentChangingEventArgs<T>(isCancelable, oldPosition, GetItemAt(oldPosition), newPosition, GetItemAt(newPosition));
            this.currentChanging.RaiseHasThreadAccessOnly(this, arg);
            if (!isCancelable)
                return false;
            return arg.Cancel;
        }

        /// <inheritdoc/>
        public void Add(T item)
        {
            if (IsReadOnly)
                ThrowForReadOnlyCollection(this.Source);
            ((IList<T>)this.Source).Add(item);
        }
        void ICollection<object>.Add(object item) => ((IList)this.Source).Add(item);
        /// <inheritdoc/>
        public void Clear() => ((IList)this.Source).Clear();
        /// <inheritdoc/>
        public bool Contains(T item) => ((IEnumerable<T>)this.Source).Contains(item);
        bool ICollection<object>.Contains(object item) => ((IList)this.Source).Contains(item);
        /// <inheritdoc/>
        public int IndexOf(T item) => ((IEnumerable<T>)this.Source).IndexOf(item);
        int IList<object>.IndexOf(object item) => ((IList)this.Source).IndexOf(item);
        /// <inheritdoc/>
        public void Insert(int index, T item)
        {
            if (IsReadOnly)
                ThrowForReadOnlyCollection(this.Source);
            ((IList<T>)this.Source).Insert(index, item);
        }
        void IList<object>.Insert(int index, object item) => ((IList)this.Source).Insert(index, item);
        /// <inheritdoc/>
        public void RemoveAt(int index) => ((IList)this.Source).RemoveAt(index);
        /// <inheritdoc/>
        public bool Remove(T item)
        {
            if (IsReadOnly) ThrowForReadOnlyCollection(this.Source);
            return ((IList<T>)this.Source).Remove(item);
        }
        bool ICollection<object>.Remove(object item) => TryCastValue(item, out T t) ? this.Remove(t) : false;

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex) => ((IEnumerable<T>)this.Source).CopyTo(array, arrayIndex);
        void ICollection<object>.CopyTo(object[] array, int arrayIndex) => ((IList)this.Source).CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public T this[int index]
        {
            get => (this.Source is IList<T> l) ? l[index] : ((IReadOnlyList<T>)this.Source)[index];
            set
            {
                if (IsReadOnly)
                    ThrowForReadOnlyCollection(this.Source);
                ((IList<T>)this.Source)[index] = value;
            }
        }
        object IList<object>.this[int index]
        {
            get => ((IList)this.Source)[index];
            set => ((IList)this.Source)[index] = value;
        }

        /// <inheritdoc/>
        public int Count => this.Source.CountInternal;
        /// <inheritdoc/>
        public bool IsReadOnly => this.Source.IsReadOnlyInternal;
        /// <inheritdoc/>
        public bool IsFixedSize => this.Source.IsFixedSizeInternal;

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)this.Source).GetEnumerator();
        IEnumerator<object> IEnumerable<object>.GetEnumerator() => this.Source.Cast<object>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IList)this.Source).GetEnumerator();
    }
}
