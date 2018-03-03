using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.Foundation.Collections;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;
using Opportunity.Helpers.Universal.AsyncHelpers;
using System.Threading;
using System.Diagnostics;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// An <see cref="IncrementalLoadingList{T}"/> with a previous known final length.
    /// </summary>
    /// <typeparam name="T">Type of record.</typeparam>
    public abstract class FixedIncrementalLoadingList<T> : ObservableList<T>, IList, ICollectionViewFactory
    {
        /// <summary>
        /// Create instance of <see cref="FixedIncrementalLoadingList{T}"/>.
        /// </summary>
        /// <param name="recordCount">Final length of the list.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="recordCount"/> is negitive.</exception>
        protected FixedIncrementalLoadingList(int recordCount)
            : this(Enumerable.Empty<T>(), recordCount) { }

        /// <summary>
        /// Create instance of <see cref="FixedIncrementalLoadingList{T}"/>.
        /// </summary>
        /// <param name="recordCount">Final length of the list.</param>
        /// <param name="items">Items will be copied to the <see cref="FixedIncrementalLoadingList{T}"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="recordCount"/> is less than length of <paramref name="items"/>.</exception>
        protected FixedIncrementalLoadingList(IEnumerable<T> items, int recordCount) : base(items)
        {
            if (recordCount < Count)
                throw new ArgumentOutOfRangeException(nameof(recordCount));
            this.LoadedItems = new BitArray(recordCount, false);
            for (var i = 0; i < this.Count; i++)
            {
                this.LoadedItems[i] = true;
            }
            this.Items.AddRange(Enumerable.Range(Count, recordCount - Count).Select(i => CreatePlaceholder(i)));
        }

        /// <summary>
        /// Indicates the loaded status of <see cref="ObservableList{T}.Items"/>.
        /// </summary>
        protected BitArray LoadedItems { get; }

        bool IList.IsFixedSize => true;

        /// <summary>
        /// Create placeholders for unloaded items.
        /// </summary>
        /// <param name="index">Index of unloaded item.</param>
        /// <returns>A placeholder for unloaded item at <paramref name="index"/>.</returns>
        /// <remarks>Do not return the same value for each call, otherwise <see cref="IList{T}.IndexOf(T)"/> will not work properly.</remarks>
        protected abstract T CreatePlaceholder(int index);

        /// <summary>
        /// Set item of the <see cref="FixedIncrementalLoadingList{T}"/> to placeholder value and mark that item as unloaded.
        /// </summary>
        /// <param name="index">Index of item to set new value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> out of range of the list.
        /// </exception>
        public void UnloadAt(int index) => UnloadItem(index);

        /// <summary>
        /// Set item of the <see cref="FixedIncrementalLoadingList{T}"/> to placeholder value and mark that item as unloaded.
        /// </summary>
        /// <param name="index">Index of item to set new value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> out of range of the list.
        /// </exception>
        protected virtual void UnloadItem(int index)
        {
            SetItem(index, CreatePlaceholder(index));
            this.LoadedItems[index] = false;
        }

        /// <summary>
        /// Set item of the <see cref="FixedIncrementalLoadingList{T}"/> and mark that item as loaded.
        /// </summary>
        /// <param name="index">Index of item to set new value.</param>
        /// <param name="item">New value of item.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> out of range of the list.
        /// </exception>
        protected override void SetItem(int index, T item)
        {
            base.SetItem(index, item);
            this.LoadedItems[index] = true;
        }

        /// <exception cref="InvalidOperationException">The list is fixed size.</exception>
        protected override void InsertItem(int index, T item)
        {
            throw new InvalidOperationException("The list is fixed size.");
        }

        /// <exception cref="InvalidOperationException">The list is fixed size.</exception>
        protected override void RemoveItem(int index)
        {
            throw new InvalidOperationException("The list is fixed size.");
        }

        /// <exception cref="InvalidOperationException">The list is fixed size.</exception>
        protected override void ClearItems()
        {
            throw new InvalidOperationException("The list is fixed size.");
        }

        /// <summary>
        /// Load item at position <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Item need to be loaded.</param>
        /// <returns>Load result, must contains item at position <paramref name="index"/>.</returns>
        protected abstract IAsyncOperation<LoadItemsResult<T>> LoadItemAsync(int index);

        private bool isLoading;
        /// <summary>
        /// Indicates <see cref="LoadItemsAsync(int,int)"/> is running.
        /// </summary>
        public bool IsLoading { get => this.isLoading; set => Set(ref this.isLoading, value); }

        private void formatLoadRange(ref int start, ref int end)
        {
            Debug.Assert(end >= start);
            if (start < 0)
                start = 0;
            if (end > Count)
                end = Count;
            for (; start < end; start++)
            {
                if (!this.LoadedItems[start])
                    break;
            }
            for (; end > start; end--)
            {
                if (!this.LoadedItems[end - 1])
                    break;
            }
        }

        private async Task loadItemsCoreAsync(int startIndex, int endIndex, CancellationToken token)
        {
            var loadOp = this.LoadItemAsync(startIndex);
            token.Register(loadOp.Cancel);
            var loadR = await loadOp;
            startIndex = loadR.StartIndex;
            token.ThrowIfCancellationRequested();
            foreach (var item in loadR.Items)
            {
                SetItem(startIndex, item);
                startIndex++;
            }
            if (endIndex <= startIndex)
                return;
            formatLoadRange(ref startIndex, ref endIndex);
            if (startIndex == endIndex)
                return;
            // Need load another page.
            var loadConOp = loadItemsCoreAsync(startIndex, endIndex, token);
            await loadConOp;
            token.ThrowIfCancellationRequested();
            return;
        }

        /// <summary>
        /// Load items in range.
        /// </summary>
        /// <param name="startIndex">Start index of range need to load.</param>
        /// <param name="count">Count of items need to load.</param>
        /// <exception cref="ArgumentOutOfRangeException">The range out of the list.</exception>
        public IAsyncAction LoadItemsAsync(int startIndex, int count)
        {
            if (this.isLoading)
            {
                return Run(async token =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                        token.ThrowIfCancellationRequested();
                        if (!IsLoading)
                            break;
                    }
                    var load = LoadItemsAsync(startIndex, count);
                    token.Register(load.Cancel);
                    await load;
                });
            }
            if (startIndex < 0 || startIndex > Count) throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < 0 || startIndex + count > Count) throw new ArgumentOutOfRangeException(nameof(count));
            var endIndex = startIndex + count;
            formatLoadRange(ref startIndex, ref endIndex);
            if (startIndex == endIndex)
                return AsyncAction.CreateCompleted();
            IsLoading = true;
            return Run(async token =>
            {
                try
                {
                    await this.loadItemsCoreAsync(startIndex, endIndex, token);
                }
                finally
                {
                    IsLoading = false;
                }
            });
        }

        /// <summary>
        /// Create a view implements <see cref="ICollectionView"/> and <see cref="IItemsRangeInfo"/>,
        /// which will call <see cref="LoadItemsAsync(int, int)"/> automatically.
        /// </summary>
        /// <returns>A view of this list.</returns>
        public ICollectionView CreateView() => new FixedCollectionView(this);

        internal class FixedCollectionView : ObservableObject, ICollectionView, IDisposable, IItemsRangeInfo
        {
            private readonly FixedIncrementalLoadingList<T> parent;

            public FixedCollectionView(FixedIncrementalLoadingList<T> fixedIncrementalLoadingList)
            {
                this.parent = fixedIncrementalLoadingList;
                this.parent.VectorChanged += this.Parent_VectorChanged;
            }

            public event VectorChangedEventHandler<object> VectorChanged;
            private void Parent_VectorChanged(Windows.UI.Xaml.Interop.IBindableObservableVector vector, object e)
            {
                var arg = (IVectorChangedEventArgs)e;
                var i = (int)arg.Index;
                if (i == this.currentPosition)
                {
                    ForceCurrnetChange(this.parent[i]);
                }
                var temp = this.VectorChanged;
                if (temp != null)
                    DispatcherHelper.BeginInvoke(() => temp(this, arg));
            }

            public void Dispose()
            {
                this.parent.VectorChanged -= this.Parent_VectorChanged;
            }

            public bool MoveCurrentTo(object item) => MoveCurrentToPosition(IndexOf(item));
            public bool MoveCurrentToPosition(int index)
            {
                if (OnCurrentChanging())
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
                    CurrentItem = this.parent[index];
                    CurrentItem = this.parent[index];
                    OnCurrentChanged();
                    var load = this.parent.LoadItemsAsync(index, 1);
                    load.Completed += (s, e) => s.GetResults();
                    return true;
                }
            }

            public bool MoveCurrentToFirst() => MoveCurrentToPosition(0);
            public bool MoveCurrentToLast() => MoveCurrentToPosition(this.Count - 1);
            public bool MoveCurrentToNext() => MoveCurrentToPosition(this.currentPosition + 1);
            public bool MoveCurrentToPrevious() => MoveCurrentToPosition(this.currentPosition - 1);

            public bool HasMoreItems => false;
            public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) => AsyncOperation<LoadMoreItemsResult>.CreateCompleted();

            public IObservableVector<object> CollectionGroups => null;

            public bool IsCurrentAfterLast => this.currentPosition >= this.Count;
            public bool IsCurrentBeforeFirst => this.currentPosition < 0;

            private int currentPosition;
            public int CurrentPosition
            {
                get => this.currentPosition;
                private set => Set(nameof(IsCurrentAfterLast), nameof(IsCurrentBeforeFirst), ref this.currentPosition, value);
            }
            private object currentItem;
            public object CurrentItem { get => this.currentItem; private set => Set(ref this.currentItem, value); }

            private void OnCurrentChanged()
            {
                var temp = CurrentChanged;
                if (temp != null)
                    DispatcherHelper.BeginInvoke(() => temp(this, EventArgs.Empty));
            }

            private void ForceCurrnetChange(object newCurrent)
            {
                var temp1 = CurrentChanging;
                if (temp1 is null)
                {
                    CurrentItem = newCurrent;
                    OnCurrentChanged();
                    return;
                }
                var temp2 = CurrentChanged;
                DispatcherHelper.BeginInvoke(() =>
                {
                    temp1(this, new CurrentChangingEventArgs(false));
                    CurrentItem = newCurrent;
                    temp2?.Invoke(this, EventArgs.Empty);
                });
            }
            public event EventHandler<object> CurrentChanged;
            private bool OnCurrentChanging()
            {
                var temp = CurrentChanging;
                if (temp == null)
                    return false;
                var arg = new CurrentChangingEventArgs(true);
                temp(this, arg);
                return arg.Cancel;
            }
            public event CurrentChangingEventHandler CurrentChanging;

            public int IndexOf(object item) => ((IList)this.parent).IndexOf(item);
            public void Insert(int index, object item) => ThrowForFixedSizeCollection();
            public void RemoveAt(int index) => ThrowForFixedSizeCollection();

            public object this[int index]
            {
                get => this.parent[index];
                set => ((IList)this.parent)[index] = value;
            }

            public void Add(object item) => ThrowForFixedSizeCollection();
            public void Clear() => ThrowForFixedSizeCollection();
            public bool Contains(object item) => ((IList)this.parent).Contains(item);

            public void CopyTo(object[] array, int arrayIndex) => ((IList)this.parent).CopyTo(array, arrayIndex);

            public bool Remove(object item) => ThrowForFixedSizeCollection<bool>();

            public int Count => this.parent.Count;

            public bool IsReadOnly => false;

            public IEnumerator<object> GetEnumerator() => this.parent.Cast<object>().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public async void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
            {
                await this.parent.LoadItemsAsync(visibleRange.FirstIndex, (int)visibleRange.Length);
            }
        }
    }
}
