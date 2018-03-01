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
using Windows.UI.Xaml.Data;
using Windows.Foundation.Collections;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// An <see cref="IncrementalLoadingList{T}"/> with a previous known final length.
    /// </summary>
    /// <typeparam name="T">Type of record.</typeparam>
    public abstract class FixedIncrementalLoadingList<T> : IncrementalLoadingList<T>, ICollectionViewFactory
    {
        /// <summary>
        /// Create instance of <see cref="FixedIncrementalLoadingList{T}"/>.
        /// </summary>
        /// <param name="recordCount">Final length of the list.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="recordCount"/> is negitive.</exception>
        protected FixedIncrementalLoadingList(int recordCount)
        {
            if (recordCount < 0)
                throw new ArgumentOutOfRangeException(nameof(recordCount));
            this.RecordCount = recordCount;
        }

        /// <summary>
        /// Create instance of <see cref="FixedIncrementalLoadingList{T}"/>.
        /// </summary>
        /// <param name="recordCount">Final length of the list.</param>
        /// <param name="items">Items will be copied to the <see cref="FixedIncrementalLoadingList{T}"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="recordCount"/> is less than length of <paramref name="items"/>.</exception>
        protected FixedIncrementalLoadingList(IEnumerable<T> items, int recordCount) : base(items)
        {
            if (recordCount < Count)
                throw new ArgumentOutOfRangeException(nameof(recordCount));
            this.RecordCount = recordCount;
        }

        /// <summary>
        /// Total record count, <see cref="ObservableList{T}.Count"/> should reach this value after all data loaded.
        /// </summary>
        public int RecordCount { get; }

        /// <summary>
        /// Will be <see langword="true"/> if <see cref="ObservableList{T}.Count"/> less than <see cref="RecordCount"/>.
        /// </summary>
        public override sealed bool HasMoreItems => this.Count < this.RecordCount;

        ICollectionView ICollectionViewFactory.CreateView() => throw new NotImplementedException();
    }

    public sealed class FixedIncrementalLoadingListView<T> : ObservableListView<T>, ICollectionView
    {
        public FixedIncrementalLoadingListView(FixedIncrementalLoadingList<T> list)
            : base(list)
        {
        }

        public bool MoveCurrentTo(object item) => throw new NotImplementedException();
        public bool MoveCurrentToPosition(int index) => throw new NotImplementedException();
        public bool MoveCurrentToFirst() => throw new NotImplementedException();
        public bool MoveCurrentToLast() => throw new NotImplementedException();
        public bool MoveCurrentToNext() => throw new NotImplementedException();
        public bool MoveCurrentToPrevious() => throw new NotImplementedException();
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) => throw new NotImplementedException();

        public IObservableVector<object> CollectionGroups => throw new NotImplementedException();

        public object CurrentItem => throw new NotImplementedException();

        public int CurrentPosition => throw new NotImplementedException();

        public bool HasMoreItems => throw new NotImplementedException();

        public bool IsCurrentAfterLast => throw new NotImplementedException();

        public bool IsCurrentBeforeFirst => throw new NotImplementedException();

        public event EventHandler<object> CurrentChanged;
        public event CurrentChangingEventHandler CurrentChanging;
        public event VectorChangedEventHandler<object> VectorChanged;

        public int IndexOf(object item) => throw new NotImplementedException();
        public void Insert(int index, object item) => throw new NotImplementedException();
        public void RemoveAt(int index) => throw new NotImplementedException();

        object IList<object>.this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Add(object item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool Contains(object item) => throw new NotImplementedException();
        public void CopyTo(object[] array, int arrayIndex) => throw new NotImplementedException();
        public bool Remove(object item) => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        IEnumerator<object> IEnumerable<object>.GetEnumerator() => throw new NotImplementedException();
    }
}
