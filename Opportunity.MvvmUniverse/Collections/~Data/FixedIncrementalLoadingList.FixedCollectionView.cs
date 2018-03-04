using System;
using System.Collections;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using Windows.Foundation.Collections;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;
using Opportunity.Helpers.Universal.AsyncHelpers;
using System.Linq;

namespace Opportunity.MvvmUniverse.Collections
{
    public abstract partial class FixedIncrementalLoadingList<T>
    {
        internal class FixedCollectionView : ObservableObject, ICollectionView, IDisposable, IItemsRangeInfo
        {
            private readonly FixedIncrementalLoadingList<T> parent;

            public FixedCollectionView(FixedIncrementalLoadingList<T> fixedIncrementalLoadingList)
            {
                this.parent = fixedIncrementalLoadingList;
                this.parent.VectorChanged += this.Parent_VectorChanged;
                if (this.parent.Count > 0)
                    this.currentItem = this.parent[0];
            }

            public event VectorChangedEventHandler<object> VectorChanged;
            private void Parent_VectorChanged(Windows.UI.Xaml.Interop.IBindableObservableVector vector, object e)
            {
                var arg = (IVectorChangedEventArgs)e;
                var temp = this.VectorChanged;
                if (temp != null)
                    DispatcherHelper.BeginInvoke(() => temp(this, arg));
                var i = (int)arg.Index;
                if (i == this.currentPosition)
                {
                    ForceCurrnetChange(this.parent[i]);
                }
            }

            public void Dispose()
            {
                this.parent.VectorChanged -= this.Parent_VectorChanged;
            }

            public bool MoveCurrentTo(object item) => MoveCurrentToPosition(IndexOf(item));
            public bool MoveCurrentToPosition(int index)
            {
                if (OnCurrentChanging())
                    return false;
                if (index < 0)
                {
                    CurrentPosition = -1;
                    CurrentItem = null;
                    OnCurrentChanged();
                    return false;
                }
                else if (index >= Count)
                {
                    CurrentPosition = Count;
                    CurrentItem = null;
                    OnCurrentChanged();
                    return false;
                }
                else
                {
                    CurrentPosition = index;
                    CurrentItem = this.parent[index];
                    OnCurrentChanged();
                    var start = index - 2;
                    if (start < 0)
                        start = 0;
                    var end = index + 3;
                    if (end > this.parent.Count)
                        end = this.parent.Count;
                    var load = this.parent.LoadItemsAsync(start, end - start);
                    if (load.Status == AsyncStatus.Started)
                        load.Completed += (s, e) => DispatcherHelper.BeginInvoke(() => s.GetResults());
                    return true;
                }
            }

            public bool MoveCurrentToFirst() => MoveCurrentToPosition(0);
            public bool MoveCurrentToLast() => MoveCurrentToPosition(this.parent.Count - 1);
            public bool MoveCurrentToNext() => MoveCurrentToPosition(this.currentPosition + 1);
            public bool MoveCurrentToPrevious() => MoveCurrentToPosition(this.currentPosition - 1);

            public bool HasMoreItems => false;
            public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) => AsyncOperation<LoadMoreItemsResult>.CreateCompleted();

            public IObservableVector<object> CollectionGroups => null;

            public bool IsCurrentAfterLast => this.currentPosition >= this.parent.Count;
            public bool IsCurrentBeforeFirst => this.currentPosition < 0;

            private int currentPosition;
            public int CurrentPosition
            {
                get => this.currentPosition;
                private set => Set(nameof(IsCurrentAfterLast), nameof(IsCurrentBeforeFirst), ref this.currentPosition, value);
            }
            private object currentItem;
            public object CurrentItem { get => this.currentItem; private set => Set(ref this.currentItem, value); }

            private void OnCurrentChanged()
            {
                var temp = CurrentChanged;
                if (temp != null)
                    DispatcherHelper.BeginInvoke(() => temp(this, EventArgs.Empty));
            }

            private void ForceCurrnetChange(object newCurrent)
            {
                var temp1 = CurrentChanging;
                if (temp1 is null)
                {
                    CurrentItem = newCurrent;
                    OnCurrentChanged();
                    return;
                }
                var temp2 = CurrentChanged;
                DispatcherHelper.BeginInvoke(() =>
                {
                    temp1(this, new CurrentChangingEventArgs(false));
                    CurrentItem = newCurrent;
                    temp2?.Invoke(this, EventArgs.Empty);
                });
            }
            public event EventHandler<object> CurrentChanged;
            private bool OnCurrentChanging()
            {
                var temp = CurrentChanging;
                if (temp == null)
                    return false;
                var arg = new CurrentChangingEventArgs(true);
                temp(this, arg);
                return arg.Cancel;
            }
            public event CurrentChangingEventHandler CurrentChanging;

            public int IndexOf(object item) => ((IList)this.parent).IndexOf(item);
            public void Insert(int index, object item) => ThrowForFixedSizeCollection();
            public void RemoveAt(int index) => ThrowForFixedSizeCollection();

            public object this[int index]
            {
                get => this.parent[index];
                set => ((IList)this.parent)[index] = value;
            }

            public void Add(object item) => ThrowForFixedSizeCollection();
            public void Clear() => ThrowForFixedSizeCollection();
            public bool Contains(object item) => ((IList)this.parent).Contains(item);

            public void CopyTo(object[] array, int arrayIndex) => ((IList)this.parent).CopyTo(array, arrayIndex);

            public bool Remove(object item) => ThrowForFixedSizeCollection<bool>();

            public int Count => this.parent.Count;

            public bool IsReadOnly => false;

            public IEnumerator<object> GetEnumerator() => this.parent.Cast<object>().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public async void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
            {
                await this.parent.LoadItemsAsync(visibleRange.FirstIndex, (int)visibleRange.Length);
            }
        }
    }
}
