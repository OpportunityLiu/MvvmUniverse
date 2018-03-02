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
    /// <summary>
    /// A list implements <see cref="ISupportIncrementalLoading"/>.
    /// </summary>
    /// <typeparam name="T">Type of items.</typeparam>
    public abstract class IncrementalLoadingList<T> : ObservableList<T>, ISupportIncrementalLoading, IList
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

        /// <inheritdoc/>
        public abstract bool HasMoreItems { get; }

        protected abstract IAsyncOperation<IEnumerable<T>> LoadItemsAsync(int count);

        public bool IsLoading { get; private set; }

        /// <inheritdoc/>
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (IsLoading)
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
                    var load = LoadMoreItemsAsync(count);
                    token.Register(load.Cancel);
                    return await load;
                });
            }
            if (!this.HasMoreItems)
                return AsyncOperation<LoadMoreItemsResult>.CreateCompleted(new LoadMoreItemsResult());
            IsLoading = true;
            OnPropertyChanged(nameof(IsLoading));
            return Run(async token =>
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
                    IsLoading = false;
                    OnPropertyChanged(nameof(IsLoading), nameof(HasMoreItems));
                }
            });
        }
    }
}
