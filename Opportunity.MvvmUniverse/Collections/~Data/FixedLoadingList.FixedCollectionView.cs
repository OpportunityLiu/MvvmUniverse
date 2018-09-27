using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;

namespace Opportunity.MvvmUniverse.Collections
{
    public abstract partial class FixedLoadingList<T>
    {
        internal sealed class FixedCollectionView : CollectionView<T>, IItemsRangeInfo
        {
            public FixedCollectionView(FixedLoadingList<T> fixedIncrementalLoadingList)
                : base(fixedIncrementalLoadingList) { }

            protected override bool MoveCurrentToPosition(int index, bool isCancelable)
            {
                var r = base.MoveCurrentToPosition(index, isCancelable);
                if (r)
                {
                    var start = index - 2;
                    if (start < 0)
                        start = 0;
                    var end = index + 3;
                    if (end > this.Source.CountInternal)
                        end = this.Source.CountInternal;
                    var load = this.Source.LoadItemsAsync(start, end - start);
                    if (load.Status == AsyncStatus.Started)
                        load.Completed += (s, e) =>
                        {
                            var d = DispatcherHelper.Default;
                            if (d is null)
                                load.GetResults();
                            else
                                d.Begin(() => s.GetResults());
                        };
                    else
                        load.GetResults();
                }
                return r;
            }

            public new FixedLoadingList<T> Source => (FixedLoadingList<T>)base.Source;

            public async void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
            {
                await this.Source.LoadItemsAsync(visibleRange.FirstIndex, (int)visibleRange.Length);
            }

            void IDisposable.Dispose() { }
        }
    }
}
