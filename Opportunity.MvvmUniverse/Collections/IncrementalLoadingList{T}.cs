using Opportunity.Helpers.Universal.AsyncHelpers;
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
        /// <summary>
        /// Create instance of <see cref="IncrementalLoadingList{T}"/>.
        /// </summary>
        protected IncrementalLoadingList() { }

        /// <summary>
        /// Create instance of <see cref="IncrementalLoadingList{T}"/>.
        /// </summary>
        /// <param name="items">Items will be copied to the <see cref="IncrementalLoadingList{T}"/>.</param>
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
            if (!this.HasMoreItems)
                return AsyncOperation<LoadMoreItemsResult>.CreateCompleted(new LoadMoreItemsResult());
            var task = Run(async token =>
            {
                try
                {
                    var lp = LoadMoreItemsImplementAsync((int)count);
                    token.Register(lp.Cancel);
                    var re = await lp;
                    var lc = this.AddRange(re);
                    return new LoadMoreItemsResult { Count = (uint)lc };
                }
                catch (Exception ex)
                {
                    if (!await tryHandle(ex))
                        throw;
                    return default;
                }
            });
            this.loading = task;
            return task;
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
