using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
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
    [DebuggerDisplay("Count = {" + nameof(Count) + "} HasMoreItems = {" + nameof(HasMoreItems) + "}")]
    public abstract class IncrementalLoadingList<T> : LoadingListBase<T>, ISupportIncrementalLoading
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

        /// <inheritdoc/>
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (!this.HasMoreItems)
                return AsyncOperation<LoadMoreItemsResult>.CreateCompleted();

            async Task<LoadMoreItemsResult> loadCore(int n, CancellationToken t)
            {
                if (!this.HasMoreItems)
                    return default;

                var currentCount = Count;
                var re = await LoadItemsAsync(n).AsTask(t);
                if (Count != currentCount)
                    throw new InvalidOperationException("The collection has changed during loading.");
                var lc = 0u;
                if (re.Items is null)
                    return new LoadMoreItemsResult { Count = 0 };
                if (re.StartIndex > this.Count)
                {
                    var cc = -1;
                    try
                    {
                        cc = re.Items.Count();
                    }
                    catch { }
                    throw new InvalidOperationException($"Wrong range returned from implementation of LoadItemsAsync(int).\nExpacted range: {this.Count} -\nActual range: {re.StartIndex} - {(cc > 0 ? (re.StartIndex + cc - 1).ToString() : "")}");
                }
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
                        {
                            if (re.ReplaceLoadedItems)
                                this.SetItem(current, item);
                        }
                        else
                        {
                            this.Add(item);
                            lc++;
                        }
                        current++;
                    }
                }
                return new LoadMoreItemsResult { Count = lc };
            }

            var c = (int)count;
            if (c < 0) c = int.MaxValue;

            return BeginLoading(() => Run(async token => await loadCore(c, token)));
        }
    }
}
