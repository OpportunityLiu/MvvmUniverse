using System;
using System.Collections;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using Windows.Foundation.Collections;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;
using Opportunity.Helpers.Universal.AsyncHelpers;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Opportunity.MvvmUniverse.Collections
{
    public abstract partial class FixedIncrementalLoadingList<T>
    {
        internal class FixedCollectionView : CollectionView<T>, ICollectionView, IDisposable, IItemsRangeInfo
        {
            public FixedCollectionView(FixedIncrementalLoadingList<T> fixedIncrementalLoadingList)
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
                    var load = ((FixedIncrementalLoadingList<T>)this.Source).LoadItemsAsync(start, end - start);
                    if (load.Status == AsyncStatus.Started)
                        load.Completed += (s, e) =>
                        {
                            var d = CoreApplication.MainView?.Dispatcher;
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

            public async void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
            {
                await ((FixedIncrementalLoadingList<T>)this.Source).LoadItemsAsync(visibleRange.FirstIndex, (int)visibleRange.Length);
            }
        }
    }
}
