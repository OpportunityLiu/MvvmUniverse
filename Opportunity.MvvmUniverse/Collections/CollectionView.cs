using System;
using System.Collections;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using Windows.Foundation.Collections;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;
using Opportunity.Helpers.Universal.AsyncHelpers;
using System.Linq;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// View that supports current item, grouping and so on.
    /// </summary>
    /// <typeparam name="T">Type of elements.</typeparam>
    public class CollectionView<T> : ObservableObject, ICollectionView, IDisposable, ISupportIncrementalLoading
    {
        /// <summary>
        /// Source of data.
        /// </summary>
        public ObservableCollectionBase<T> Source { get; }

        /// <summary>
        /// Create new instance of <see cref="CollectionView{T}"/>
        /// </summary>
        /// <param name="source">Source of data.</param>
        public CollectionView(ObservableCollectionBase<T> source)
        {
            this.Source = source;
            this.Source.VectorChanged += this.Source_VectorChanged;
            this.Source.PropertyChanged += this.Source_PropertyChanged;
            if (this.Source.CountInternal > 0)
                this.currentItem = ((IList)this.Source)[0];
        }

        private readonly DepedencyEvent<VectorChangedEventHandler<object>, CollectionView<T>, IVectorChangedEventArgs> vectorChanged
            = new DepedencyEvent<VectorChangedEventHandler<object>, CollectionView<T>, IVectorChangedEventArgs>((h, s, e) => h(s, e));
        /// <inheritdoc/>
        public event VectorChangedEventHandler<object> VectorChanged
        {
            add => this.vectorChanged.Add(value);
            remove => this.vectorChanged.Remove(value);
        }

        private void Source_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
            case nameof(Count):
            case nameof(HasMoreItems):
                OnPropertyChanged(e.PropertyName);
                break;
            case null:
            case "":
                OnPropertyChanged(nameof(HasMoreItems), nameof(Count));
                break;
            }
        }

        private void Source_VectorChanged(Windows.UI.Xaml.Interop.IBindableObservableVector vector, object e)
        {
            var arg = (IVectorChangedEventArgs)e;

            this.vectorChanged.Raise(this, arg);
            switch (arg.CollectionChange)
            {
            case CollectionChange.ItemInserted:
                if (arg.Index <= this.currentPosition)
                    MoveCurrentToPosition(this.currentPosition + 1, false);
                break;
            case CollectionChange.ItemRemoved:
                if (arg.Index <= this.currentPosition)
                    MoveCurrentToPosition(this.currentPosition - 1, false);
                break;
            case CollectionChange.ItemChanged:
                if (arg.Index == this.currentPosition)
                    MoveCurrentToPosition(this.currentPosition, false);
                break;
            default:
                MoveCurrentToPosition(this.currentPosition, false);
                break;
            }
        }

        /// <inheritdoc/>
        public bool MoveCurrentTo(object item) => MoveCurrentToPosition(IndexOf(item));
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
            if (OnCurrentChanging(isCancelable))
                return false;
            if (index < 0)
            {
                CurrentPosition = -1;
                CurrentItem = null;
                OnCurrentChanged();
                return false;
            }
            else if (index >= Count)
            {
                CurrentPosition = Count;
                CurrentItem = null;
                OnCurrentChanged();
                return false;
            }
            else
            {
                CurrentPosition = index;
                CurrentItem = ((IList)this.Source)[index];
                OnCurrentChanged();
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

        private int currentPosition;
        /// <inheritdoc/>
        public int CurrentPosition
        {
            get => this.currentPosition;
            private set => Set(nameof(IsCurrentAfterLast), nameof(IsCurrentBeforeFirst), ref this.currentPosition, value);
        }
        private object currentItem;
        /// <inheritdoc/>
        public object CurrentItem { get => this.currentItem; private set => Set(ref this.currentItem, value); }

        private readonly DepedencyEvent<EventHandler<object>, CollectionView<T>, object> currentChanged
            = new DepedencyEvent<EventHandler<object>, CollectionView<T>, object>((h, s, e) => h(s, e));
        /// <inheritdoc/>
        public event EventHandler<object> CurrentChanged
        {
            add => this.currentChanged.Add(value);
            remove => this.currentChanged.Remove(value);
        }

        /// <summary>
        /// Raise <see cref="CurrentChanged"/>.
        /// </summary>
        protected void OnCurrentChanged()
        {
            this.currentChanged.Raise(this, null);
        }

        private readonly DepedencyEvent<CurrentChangingEventHandler, CollectionView<T>, CurrentChangingEventArgs> currentChanging
            = new DepedencyEvent<CurrentChangingEventHandler, CollectionView<T>, CurrentChangingEventArgs>((h, s, e) => h(s, e));
        /// <inheritdoc/>
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
        protected bool OnCurrentChanging(bool isCancelable)
        {
            if (this.currentChanging.InvocationListLength == 0)
                return false;
            var arg = new CurrentChangingEventArgs(isCancelable);
            this.currentChanging.RaiseHasThreadAccessOnly(this, arg);
            if (!isCancelable)
                return false;
            return arg.Cancel;
        }

        /// <inheritdoc/>
        public void Add(object item) => ((IList)this.Source).Add(item);
        /// <inheritdoc/>
        public void Clear() => ((IList)this.Source).Clear();
        /// <inheritdoc/>
        public bool Contains(object item) => ((IList)this.Source).Contains(item);
        /// <inheritdoc/>
        public int IndexOf(object item) => ((IList)this.Source).IndexOf(item);
        /// <inheritdoc/>
        public void Insert(int index, object item) => ((IList)this.Source).Insert(index, item);
        /// <inheritdoc/>
        public void RemoveAt(int index) => ((IList)this.Source).RemoveAt(index);
        /// <inheritdoc/>
        public bool Remove(object item) => this.Source.Remove(item);

        /// <inheritdoc/>
        public void CopyTo(object[] array, int arrayIndex) => ((IList)this.Source).CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public object this[int index]
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
        public IEnumerator<object> GetEnumerator() => this.Source.Cast<object>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IList)this.Source).GetEnumerator();

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        /// <summary>
        /// Implement of <see cref="IDisposable"/>.
        /// </summary>
        /// <param name="disposing">Is <see cref="Dispose()"/> called.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // 释放托管状态(托管对象)。
                    this.Source.VectorChanged -= this.Source_VectorChanged;
                    this.Source.PropertyChanged -= this.Source_PropertyChanged;
                }

                // 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // 将大型字段设置为 null。

                this.disposedValue = true;
            }
        }

        // 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~CollectionView() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        /// <inheritdoc/>
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
