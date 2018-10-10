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
    public abstract class PagingList<T> : ObservableList<T>, ISupportIncrementalLoading
    {
        protected PagingList() { }

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
        /// <exception cref="InvalidOperationException"><see cref="IsLoading"/> is <see langword="true"/>.</exception>
        public void JumpTo(int pageIndex)
        {
            if (this._FirstPage == pageIndex)
                return;
            if (pageIndex < 0 || pageIndex >= PageCount)
                throw new ArgumentOutOfRangeException(nameof(pageIndex));
            Clear();
            this.FirstPage = pageIndex;
        }

        private void throwIfIsLoading()
        {
            if (this.isLoading != 0)
                throw new InvalidOperationException("Loading in progress.");
        }

        /// <summary>
        /// Remove all items in the <see cref="PagingList{T}"/>, set <see cref="FirstPage"/> and <see cref="LoadedPageCount"/> to 0.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="IsLoading"/> is <see langword="true"/>.</exception>
        protected override void ClearItems()
        {
            throwIfIsLoading();
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

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int isLoading;
        /// <summary>
        /// Indicates <see cref="LoadPreviousPage()"/> or <see cref="LoadNextPage()"/> is running.
        /// </summary>
        public bool IsLoading => this.isLoading != 0;

        public IAsyncAction LoadPreviousPage()
        {
            if (this._FirstPage <= 0)
                return AsyncAction.CreateCompleted();

            if (Interlocked.CompareExchange(ref this.isLoading, 1, 0) == 0)
            {
                OnPropertyChanged(nameof(IsLoading));
                return Run(async token =>
                {
                    try
                    {
                        if (this._FirstPage <= 0)
                            return;

                        var re = await LoadItemsAsync(this._FirstPage - 1).AsTask(token);

                        if (this._FirstPage <= 0)
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
                    finally
                    {
                        Volatile.Write(ref this.isLoading, 0);
                        OnPropertyChanged(nameof(IsLoading), nameof(ISupportIncrementalLoading.HasMoreItems));
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
                    await LoadPreviousPage().AsTask(token);
                });
            }
        }

        public IAsyncAction LoadNextPage()
        {
            if (this._FirstPage + this._LoadedPageCount >= this._PageCount)
                return AsyncAction.CreateCompleted();

            if (Interlocked.CompareExchange(ref this.isLoading, 1, 0) == 0)
            {
                OnPropertyChanged(nameof(IsLoading));
                return Run(async token =>
                {
                    try
                    {
                        if (this._FirstPage + this._LoadedPageCount >= this._PageCount)
                            return;

                        var re = await this.LoadItemsAsync(this._FirstPage + this._LoadedPageCount).AsTask(token);

                        if (this._FirstPage + this._LoadedPageCount >= this._PageCount)
                            return;

                        foreach (var item in re)
                            this.Add(item);
                        this.LoadedPageCount++;
                    }
                    finally
                    {
                        Volatile.Write(ref this.isLoading, 0);
                        OnPropertyChanged(nameof(IsLoading), nameof(ISupportIncrementalLoading.HasMoreItems));
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
                    await LoadNextPage().AsTask(token);
                });
            }
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
