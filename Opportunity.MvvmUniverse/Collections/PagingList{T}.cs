using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// An <see cref="IncrementalLoadingList{T}"/> load one page once.
    /// </summary>
    /// <typeparam name="T">Type of record</typeparam>
    public abstract class PagingList<T> : IncrementalLoadingList<T>
    {
        /// <summary>
        /// Create instance of <see cref="PagingList{T}"/>.
        /// </summary>
        protected PagingList() { }

        /// <summary>
        /// Create instance of <see cref="PagingList{T}"/>.
        /// </summary>
        /// <param name="items">Items will be copied to the <see cref="PagingList{T}"/>.</param>
        protected PagingList(IEnumerable<T> items) : base(items) { }

        private int recordCount;
        /// <summary>
        /// Total record count, <see cref="ObservableList{T}.Count"/> should reach this value after all pages loaded.
        /// </summary>
        public int RecordCount
        {
            get => this.recordCount;
            protected set => Set(nameof(IsEmpty), ref this.recordCount, value);
        }

        private int pageCount;
        /// <summary>
        /// Page count, <see cref="HasMoreItems"/> will be set to false
        /// when <see cref="LoadedPageCount"/> reaches this value.
        /// </summary>
        public int PageCount
        {
            get => this.pageCount;
            protected set => Set(nameof(HasMoreItems), ref this.pageCount, value);
        }

        /// <summary>
        /// Load records in page <paramref name="pageIndex"/>.
        /// </summary>
        /// <param name="pageIndex">Index of page to load</param>
        /// <returns>Records in page <paramref name="pageIndex"/></returns>
        protected abstract IAsyncOperation<IEnumerable<T>> LoadPageAsync(int pageIndex);

        /// <summary>
        /// Will be true if <see cref="RecordCount"/> is 0.
        /// </summary>
        public bool IsEmpty => this.RecordCount == 0;

        private int loadedPageCount;
        /// <summary>
        /// Loaded page count, the next page will be loaded when <see cref="IncrementalLoadingList{T}.LoadMoreItemsAsync(uint)"/> be called.
        /// Will be increased automatically after loading.
        /// </summary>
        public int LoadedPageCount
        {
            get => this.loadedPageCount;
            protected set => Set(nameof(HasMoreItems), ref this.loadedPageCount, value);
        }

        /// <summary>
        /// Will be ture if <see cref="LoadedPageCount"/> less than <see cref="PageCount"/>.
        /// </summary>
        public override sealed bool HasMoreItems => this.loadedPageCount < this.pageCount;

        /// <summary>
        /// Reset this collection.
        /// Will set <see cref="LoadedPageCount"/>, <see cref="PageCount"/> and <see cref="RecordCount"/> to 0, and clear the collection.
        /// </summary>
        protected void ResetAll()
        {
            this.loadedPageCount = 0;
            this.pageCount = 0;
            this.recordCount = 0;
            Clear();
            OnPropertyChanged(nameof(LoadedPageCount), nameof(PageCount), nameof(RecordCount), nameof(IsEmpty), nameof(HasMoreItems));
        }

        /// <summary>
        /// Use <see cref="LoadPageAsync(int)"/> to load data.
        /// </summary>
        /// <param name="count">Ignored.</param>
        /// <returns>Loaded data.</returns>
        protected override sealed IAsyncOperation<IEnumerable<T>> LoadMoreItemsImplementAsync(int count)
        {
            return Run(async token =>
            {
                var lp = LoadPageAsync(this.loadedPageCount);
                token.Register(lp.Cancel);
                var re = await lp;
                this.LoadedPageCount++;
                return re;
            });
        }
    }
}
