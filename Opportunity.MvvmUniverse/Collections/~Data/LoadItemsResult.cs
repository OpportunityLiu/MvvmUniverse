using System;
using System.Collections.Generic;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// Factory methods for <see cref="LoadItemsResult{T}"/>/
    /// </summary>
    public static class LoadItemsResult
    {
        /// <summary>
        /// Get an instance of <see cref="LoadItemsResult{T}"/> represents empty result.
        /// </summary>
        /// <typeparam name="T">Type of results.</typeparam>
        /// <returns>An instance of <see cref="LoadItemsResult{T}"/> represents empty result.</returns>
        public static LoadItemsResult<T> Empty<T>() => default;

        /// <summary>
        /// Create new instance of <see cref="LoadItemsResult{T}"/>.
        /// </summary>
        /// <param name="startIndex">Start index of loaded items.</param>
        /// <param name="items">Loaded items.</param>
        /// <param name="replaceLoadedItems"> Should <paramref name="items"/> replace items that already in the collection or not.</param>
        /// <typeparam name="T">Type of results.</typeparam>
        /// <returns>An instance of <see cref="LoadItemsResult{T}"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startIndex"/> is negitive.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/>.</exception>
        public static LoadItemsResult<T> Create<T>(int startIndex, IEnumerable<T> items, bool replaceLoadedItems)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            return new LoadItemsResult<T>(startIndex, items, replaceLoadedItems);
        }

        /// <summary>
        /// Create new instance of <see cref="LoadItemsResult{T}"/>.
        /// </summary>
        /// <param name="startIndex">Start index of loaded items.</param>
        /// <param name="items">Loaded items.</param>
        /// <typeparam name="T">Type of results.</typeparam>
        /// <returns>An instance of <see cref="LoadItemsResult{T}"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startIndex"/> is negitive.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/>.</exception>
        public static LoadItemsResult<T> Create<T>(int startIndex, IEnumerable<T> items)
            => Create(startIndex, items, true);
    }

    /// <summary>
    /// Load result of <see cref="FixedIncrementalLoadingList{T}.LoadItemAsync(int)"/>.
    /// </summary>
    /// <typeparam name="T">Type of items.</typeparam>
    public readonly struct LoadItemsResult<T>
    {
        internal LoadItemsResult(int startIndex, IEnumerable<T> items, bool replaceLoadedItems)
        {
            this.StartIndex = startIndex;
            this.Items = items;
            this.ReplaceLoadedItems = replaceLoadedItems;
        }

        /// <summary>
        /// Start index of loaded items.
        /// </summary>
        public int StartIndex { get; }
        /// <summary>
        /// Loaded items.
        /// </summary>
        public IEnumerable<T> Items { get; }

        /// <summary>
        /// Should <see cref="Items"/> replace items in the collection or not.
        /// </summary>
        public bool ReplaceLoadedItems { get; }
    }
}
