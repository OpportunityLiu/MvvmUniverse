using Opportunity.MvvmUniverse.AsyncHelpers;
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
    public abstract class IncrementalLoadingCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
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

        protected abstract IAsyncOperation<IList<T>> LoadPageAsync(int pageIndex);

        public bool IsEmpty => this.RecordCount == 0;

        private int loadedPageCount;
        public int LoadedPageCount
        {
            get => this.loadedPageCount;
            protected set => Set(nameof(HasMoreItems), ref this.loadedPageCount, value);
        }

        public bool HasMoreItems => this.loadedPageCount < this.pageCount;

        protected void ResetAll()
        {
            this.loadedPageCount = 0;
            this.pageCount = 0;
            this.recordCount = 0;
            Clear();
            RaisePropertyChanged(nameof(LoadedPageCount), nameof(PageCount), nameof(RecordCount), nameof(IsEmpty), nameof(HasMoreItems));
        }

        private IAsyncOperation<LoadMoreItemsResult> loading;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (this.loading?.Status == AsyncStatus.Started)
            {
                return PollingAsyncWrapper.Wrap(this.loading);
            }
            return this.loading = Run(async token =>
            {
                if (!this.HasMoreItems)
                    return new LoadMoreItemsResult();
                var lp = LoadPageAsync(this.loadedPageCount);
                IList<T> re = null;
                token.Register(lp.Cancel);
                try
                {
                    re = await lp;
                    this.AddRange(re);
                    this.LoadedPageCount++;
                }
                catch (Exception ex)
                {
                    if (!await tryHandle(ex))
                        throw;
                }
                return new LoadMoreItemsResult() { Count = re == null ? 0u : (uint)re.Count };
            });
        }

        public event TypedEventHandler<IncrementalLoadingCollection<T>, LoadMoreItemsExceptionEventArgs> LoadMoreItemsException;

        private async Task<bool> tryHandle(Exception ex)
        {
            var temp = LoadMoreItemsException;
            if (temp == null)
                return false;
            var h = false;
            await DispatcherHelper.RunAsync(() =>
            {
                var args = new LoadMoreItemsExceptionEventArgs(ex);
                temp(this, args);
                h = args.Handled;
            });
            return h;
        }
    }
}
