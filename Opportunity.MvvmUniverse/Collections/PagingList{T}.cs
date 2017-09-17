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
    public abstract class PagingList<T> : IncrementalLoadingList<T>
    {
        protected PagingList() { }

        protected PagingList(IEnumerable<T> items) : base(items) { }

        private int recordCount;
        public int RecordCount
        {
            get => this.recordCount;
            protected set => Set(nameof(IsEmpty), ref this.recordCount, value);
        }

        private int pageCount;
        public int PageCount
        {
            get => this.pageCount;
            protected set => Set(nameof(HasMoreItems), ref this.pageCount, value);
        }

        protected abstract IAsyncOperation<IEnumerable<T>> LoadPageAsync(int pageIndex);

        public bool IsEmpty => this.RecordCount == 0;

        private int loadedPageCount;
        public int LoadedPageCount
        {
            get => this.loadedPageCount;
            protected set => Set(nameof(HasMoreItems), ref this.loadedPageCount, value);
        }

        public override sealed bool HasMoreItems => this.loadedPageCount < this.pageCount;

        protected void ResetAll()
        {
            this.loadedPageCount = 0;
            this.pageCount = 0;
            this.recordCount = 0;
            Clear();
            OnPropertyChanged(nameof(LoadedPageCount), nameof(PageCount), nameof(RecordCount), nameof(IsEmpty), nameof(HasMoreItems));
        }

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
