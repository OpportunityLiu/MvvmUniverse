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

        /// <summary>
        /// Implementation of <see cref="LoadMoreItemsAsync(uint)"/>.
        /// </summary>
        /// <param name="count">Items need to be loaded.</param>
        /// <returns>Loaded items.</returns>
        protected abstract IAsyncOperation<LoadItemsResult<T>> LoadItemsAsync(int count);

        /// <summary>
        /// Indicates <see cref="LoadMoreItemsAsync(uint)"/> is running.
        /// </summary>
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
                    if (re.StartIndex > this.Count)
                        throw new InvalidOperationException("Wrong range returned from implementation of LoadItemsAsync(int).");
                    else if (re.StartIndex == this.Count)
                    {
                        foreach (var item in re.Items)
                        {
                            this.Add(item);
                            lc++;
                        }
                    }
                    else
                    {
                        var current = re.StartIndex;
                        foreach (var item in re.Items)
                        {
                            if (current < Count)
                                this.SetItem(current, item);
                            else
                                this.Add(item);
                            lc++;
                            current++;
                        }
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
