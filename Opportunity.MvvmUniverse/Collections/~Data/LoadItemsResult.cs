using System;
using System.Collections.Generic;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// Load result of <see cref="FixedIncrementalLoadingList{T}.LoadItemAsync(int)"/>.
    /// </summary>
    /// <typeparam name="T">Type of items.</typeparam>
    public struct LoadItemsResult<T>
    {
        /// <summary>
        /// Create new instance of <see cref="LoadItemsResult{T}"/>.
        /// </summary>
        /// <param name="startIndex">Start index of loaded items.</param>
        /// <param name="items">Loaded items.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startIndex"/> is negitive.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/>.</exception>
        public LoadItemsResult(int startIndex, IEnumerable<T> items)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            this.StartIndex = startIndex;
            this.Items = items ?? throw new ArgumentNullException(nameof(items));
        }

        /// <summary>
        /// Start index of loaded items.
        /// </summary>
        public int StartIndex { get; }
        /// <summary>
        /// Loaded items.
        /// </summary>
        public IEnumerable<T> Items { get; }
    }
}
