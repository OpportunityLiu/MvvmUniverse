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
    public abstract class IncrementalLoadingList<T> : ObservableList<T>, ISupportIncrementalLoading
    {
        protected IncrementalLoadingList() { }

        protected IncrementalLoadingList(IEnumerable<T> items) : base(items) { }

        public abstract bool HasMoreItems { get; }

        protected abstract IAsyncOperation<IEnumerable<T>> LoadMoreItemsImplementAsync(int count);

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
                var lp = LoadMoreItemsImplementAsync((int)count);
                var lc = 0;
                token.Register(lp.Cancel);
                try
                {
                    var re = await lp;
                    lc = this.AddRange(re);
                }
                catch (Exception ex)
                {
                    if (!await tryHandle(ex))
                        throw;
                }
                return new LoadMoreItemsResult() { Count = (uint)lc };
            });
        }

        public event LoadMoreItemsExceptionEventHadler<T> LoadMoreItemsException;

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
