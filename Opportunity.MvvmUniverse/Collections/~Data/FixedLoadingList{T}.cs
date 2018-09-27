using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// An <see cref="ObservableList{T}"/> with a previous known final length that supports incremental loading.
    /// </summary>
    /// <typeparam name="T">Type of record.</typeparam>
    public abstract partial class FixedLoadingList<T> : ObservableList<T>, IList
    {
        /// <summary>
        /// Create instance of <see cref="FixedLoadingList{T}"/>.
        /// </summary>
        /// <param name="recordCount">Final length of the list.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="recordCount"/> is negitive.</exception>
        protected FixedLoadingList(int recordCount)
            : this(Enumerable.Empty<T>(), recordCount) { }

        /// <summary>
        /// Create instance of <see cref="FixedLoadingList{T}"/>.
        /// </summary>
        /// <param name="recordCount">Final length of the list.</param>
        /// <param name="items">Items will be copied to the <see cref="FixedLoadingList{T}"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="recordCount"/> is less than length of <paramref name="items"/>.</exception>
        protected FixedLoadingList(IEnumerable<T> items, int recordCount) : base(items)
        {
            if (recordCount < Count)
                throw new ArgumentOutOfRangeException(nameof(recordCount));
            this.LoadedItems = new bool[recordCount];
            for (var i = 0; i < Count; i++)
            {
                this.LoadedItems[i] = true;
            }
            for (var i = Count; i < recordCount; i++)
            {
                this.Items.Add(CreatePlaceholder(i));
            }
        }

        /// <summary>
        /// Indicates the loaded status of <see cref="ObservableList{T}.Items"/>.
        /// </summary>
        protected bool[] LoadedItems { get; }

        bool IList.IsFixedSize => true;

        /// <summary>
        /// Create placeholders for unloaded items.
        /// </summary>
        /// <param name="index">Index of unloaded item.</param>
        /// <returns>A placeholder for unloaded item at <paramref name="index"/>.</returns>
        /// <remarks>Do not return the same value for each call, otherwise <see cref="IList{T}.IndexOf(T)"/> will not work properly.</remarks>
        protected abstract T CreatePlaceholder(int index);

        /// <summary>
        /// Set item of the <see cref="FixedLoadingList{T}"/> to placeholder value and mark that item as unloaded.
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
        /// Set item of the <see cref="FixedLoadingList{T}"/> and mark that item as loaded.
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
        protected sealed override void InsertItem(int index, T item)
            => Internal.Helpers.ThrowForFixedSizeCollection();

        /// <exception cref="InvalidOperationException">The list is fixed size.</exception>
        protected sealed override void RemoveItem(int index)
            => Internal.Helpers.ThrowForFixedSizeCollection();

        /// <exception cref="InvalidOperationException">The list is fixed size.</exception>
        protected sealed override void ClearItems()
            => Internal.Helpers.ThrowForFixedSizeCollection();

        /// <summary>
        /// Set item of the <see cref="FixedLoadingList{T}"/> to placeholder value and mark that item as unloaded.
        /// </summary>
        /// <param name="index">Index of item to set new value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> out of range of the list.
        /// </exception>
        public void UnloadAt(int index) => UnloadItem(index);

        /// <summary>
        /// Get loading status of item at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of item.</param>
        /// <returns>Item is loaded or not.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> out of range of the list.
        /// </exception>
        public bool IsLoaded(int index)
        {
            try
            {
                return this.LoadedItems[index];
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new ArgumentOutOfRangeException(nameof(index), ex);
            }
        }

        /// <summary>
        /// Load item at position <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Item need to be loaded.</param>
        /// <returns>Load result, must contains item at position <paramref name="index"/>.</returns>
        protected abstract IAsyncOperation<LoadItemsResult<T>> LoadItemAsync(int index);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int isLoading;
        /// <summary>
        /// Indicates <see cref="LoadItemsAsync(int,int)"/> is running.
        /// </summary>
        public bool IsLoading => this.isLoading != 0;

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
            var loadR = await this.LoadItemAsync(startIndex).AsTask(token);
            if (loadR.Items is null)
                throw new InvalidOperationException("Wrong result of LoadItemAsync(int), " + nameof(loadR.Items) + " is null.");
            startIndex = loadR.StartIndex;
            if (loadR.ReplaceLoadedItems)
            {
                foreach (var item in loadR.Items)
                {
                    SetItem(startIndex, item);
                    startIndex++;
                }
            }
            else
            {
                foreach (var item in loadR.Items)
                {
                    if (!this.LoadedItems[startIndex])
                        SetItem(startIndex, item);
                    startIndex++;
                }
            }
            if (endIndex <= startIndex)
                return;
            formatLoadRange(ref startIndex, ref endIndex);
            if (startIndex >= endIndex)
                return;

            token.ThrowIfCancellationRequested();
            // Need load another page.
            await loadItemsCoreAsync(startIndex, endIndex, token);
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
            if (startIndex < 0 || startIndex > Count) throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < 0 || startIndex + count > Count) throw new ArgumentOutOfRangeException(nameof(count));
            var endIndex = startIndex + count;
            formatLoadRange(ref startIndex, ref endIndex);
            if (startIndex >= endIndex)
                return AsyncAction.CreateCompleted();
            if (Interlocked.CompareExchange(ref this.isLoading, 1, 0) == 0)
            {
                OnPropertyChanged(nameof(IsLoading));
                return Run(async token =>
                {
                    try
                    {
                        await this.loadItemsCoreAsync(startIndex, endIndex, token);
                    }
                    finally
                    {
                        Volatile.Write(ref this.isLoading, 0);
                        OnPropertyChanged(nameof(IsLoading));
                    }
                });
            }
            else
            {
                return Run(async token =>
                {
                    while (!token.IsCancellationRequested && this.isLoading != 0)
                    {
                        await Task.Delay(500, token);
                    }
                    token.ThrowIfCancellationRequested();
                    await this.LoadItemsAsync(startIndex, endIndex - startIndex).AsTask(token);
                });
            }
        }

        /// <summary>
        /// Create a view implements <see cref="ICollectionView"/> and <see cref="IItemsRangeInfo"/>,
        /// which will call <see cref="LoadItemsAsync(int, int)"/> automatically.
        /// </summary>
        /// <returns>A view of this list.</returns>
        public override CollectionView<T> CreateView() => new FixedCollectionView(this);
    }
}
