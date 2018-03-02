using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.Foundation.Collections;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// An <see cref="IncrementalLoadingList{T}"/> with a previous known final length.
    /// </summary>
    /// <typeparam name="T">Type of record.</typeparam>
    public abstract class FixedIncrementalLoadingList<T> : IncrementalLoadingList<T>, IList
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

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException"><see cref="ObservableList{T}.Count"/> has reached <see cref="RecordCount"/>.</exception>
        protected override void InsertItem(int index, T item)
        {
            if (this.Count >= this.RecordCount)
                throw new InvalidOperationException("The list has reached RecordCount");
            base.InsertItem(index, item);
        }

        /// <summary>
        /// Total record count, <see cref="ObservableList{T}.Count"/> should reach this value after all data loaded.
        /// </summary>
        public int RecordCount { get; }

        /// <summary>
        /// Will be <see langword="true"/> if <see cref="ObservableList{T}.Count"/> less than <see cref="RecordCount"/>.
        /// </summary>
        public override sealed bool HasMoreItems => this.Count < this.RecordCount;
    }
}
