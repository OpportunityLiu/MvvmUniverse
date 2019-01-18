using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;


namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// A list with paged data.
    /// </summary>
    /// <typeparam name="T">Type of items.</typeparam>
    [DebuggerDisplay("{DebuggerDisp}")]
    public abstract class PagingList<T> : LoadingListBase<T>, ISupportIncrementalLoading
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisp
        {
            get
            {
                var str = $"Count = {Count} Pages = ";
                if (this._LoadedPageCount <= 0)
                    str += this._PageCount;
                else
                    str += $"{this._FirstPage}-{this._FirstPage + this._LoadedPageCount - 1}/{this._PageCount}";
                return str;
            }
        }

        /// <summary>
        /// Create instance of <see cref="PagingList{T}"/>.
        /// </summary>
        protected PagingList() { }

        /// <summary>
        /// Create instance of <see cref="PagingList{T}"/>.
        /// </summary>
        /// <param name="items">Items will be copied to the <see cref="PagingList{T}"/>.</param>
        protected PagingList(IEnumerable<T> items) : base(items) { }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _PageCount = 1;
        /// <summary>
        /// Page count of the <see cref="PagingList{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException">New <paramref name="value"/> dosen't cover loaded range.</exception>
        public int PageCount
        {
            get => this._PageCount;
            set
            {
                if (value < this._FirstPage + this._LoadedPageCount)
                    throw new ArgumentException($"Wrong value, must >= FirstPage + LoadedPageCount ({this._FirstPage + this._LoadedPageCount})");
                Set(nameof(ISupportIncrementalLoading.HasMoreItems), ref this._PageCount, value);
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _FirstPage;
        /// <summary>
        /// Index of the first page that is loaded.
        /// </summary>
        public int FirstPage { get => this._FirstPage; private set => Set(nameof(ISupportIncrementalLoading.HasMoreItems), ref this._FirstPage, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _LoadedPageCount;
        /// <summary>
        /// Count of pages that is loaded.
        /// </summary>
        public int LoadedPageCount { get => this._LoadedPageCount; private set => Set(nameof(ISupportIncrementalLoading.HasMoreItems), ref this._LoadedPageCount, value); }

        /// <summary>
        /// Set <see cref="FirstPage"/> to <paramref name="pageIndex"/>, all items in the <see cref="PagingList{T}"/> will be removed.
        /// </summary>
        /// <param name="pageIndex">Index of page that will jump to.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pageIndex"/> is not in range of <see cref="PageCount"/>.</exception>
        public void JumpTo(int pageIndex)
        {
            if (this._FirstPage == pageIndex)
                return;
            if (pageIndex < 0 || pageIndex >= PageCount)
                throw new ArgumentOutOfRangeException(nameof(pageIndex));
            Clear();
            this.FirstPage = pageIndex;
        }

        /// <summary>
        /// Remove all items in the <see cref="PagingList{T}"/>, set <see cref="FirstPage"/> and <see cref="LoadedPageCount"/> to 0.
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();
            this.FirstPage = 0;
            this.LoadedPageCount = 0;
        }

        /// <summary>
        /// Load data of a page.
        /// </summary>
        /// <param name="pageIndex">Index of page to be loaded.</param>
        /// <returns>Loaded items.</returns>
        protected abstract IAsyncOperation<IEnumerable<T>> LoadItemsAsync(int pageIndex);

        /// <summary>
        /// Load previous page of currently loaded range.
        /// </summary>
        public IAsyncAction LoadPreviousPage()
        {
            if (this._FirstPage <= 0)
                return AsyncAction.CreateCompleted();

            async Task load(CancellationToken token)
            {
                if (this._FirstPage <= 0)
                    return;

                var toLoad = this._FirstPage - 1;
                var re = await LoadItemsAsync(toLoad).AsTask(token);

                if (this._FirstPage <= 0)
                    return;
                if (toLoad != this._FirstPage - 1)
                    return;

                var i = 0;
                foreach (var item in re)
                {
                    this.Insert(i, item);
                    i++;
                }
                LoadedPageCount++;
                FirstPage--;
            }

            return BeginLoading(() => Run(async token => await load(token)));
        }

        /// <summary>
        /// Load next page of currently loaded range.
        /// </summary>
        public IAsyncAction LoadNextPage()
        {
            if (this._FirstPage + this._LoadedPageCount >= this._PageCount)
                return AsyncAction.CreateCompleted();

            async Task load(CancellationToken token)
            {
                if (this._FirstPage + this._LoadedPageCount >= this._PageCount)
                    return;

                var toLoad = this._FirstPage + this._LoadedPageCount;
                var re = await this.LoadItemsAsync(toLoad).AsTask(token);

                if (this._FirstPage + this._LoadedPageCount >= this._PageCount)
                    return;
                if (toLoad != this._FirstPage + this._LoadedPageCount)
                    return;

                foreach (var item in re)
                    this.Add(item);
                this.LoadedPageCount++;
            }

            return BeginLoading(() => Run(async token => await load(token)));
        }

        bool ISupportIncrementalLoading.HasMoreItems => this._FirstPage + this._LoadedPageCount < this._PageCount;

        IAsyncOperation<LoadMoreItemsResult> ISupportIncrementalLoading.LoadMoreItemsAsync(uint count)
        {
            var il = (ISupportIncrementalLoading)this;
            if (!il.HasMoreItems)
                return AsyncOperation<LoadMoreItemsResult>.CreateCompleted();
            var cc = this.Count;
            return Run(async token =>
            {
                await LoadNextPage().AsTask(token);
                return new LoadMoreItemsResult { Count = (uint)(this.Count - cc) };
            });
        }
    }
}
