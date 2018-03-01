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

        protected abstract IAsyncOperation<IEnumerable<T>> LoadItemsAsync(int count);

        public bool IsLoading => this.loading?.Status == AsyncStatus.Started;

        private IAsyncOperation<LoadMoreItemsResult> loading;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            var currentLoading = this.loading;
            if (currentLoading?.Status == AsyncStatus.Started)
            {
                return PollingAsyncWrapper.Wrap(currentLoading);
            }
            if (!this.HasMoreItems)
                return AsyncOperation<LoadMoreItemsResult>.CreateCompleted(new LoadMoreItemsResult());
            currentLoading = Run(async token =>
            {
                try
                {
                    var lp = LoadItemsAsync((int)count);
                    token.Register(lp.Cancel);
                    var re = await lp;
                    token.ThrowIfCancellationRequested();
                    var lc = 0u;
                    foreach (var item in re)
                    {
                        this.Add(item);
                        lc++;
                    }
                    return new LoadMoreItemsResult { Count = lc };
                }
                finally
                {
                    OnPropertyChanged(nameof(IsLoading), nameof(HasMoreItems));
                }
            });
            this.loading = currentLoading;
            OnPropertyChanged(nameof(IsLoading));
            return currentLoading;
        }
    }
}
