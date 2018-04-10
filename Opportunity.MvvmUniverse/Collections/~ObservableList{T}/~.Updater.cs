using System;
using System.Collections.Generic;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// Updater used to copy data from equals items.
    /// </summary>
    /// <typeparam name="T">Type of items.</typeparam>
    /// <param name="existItem">Item exist in the collection.</param>
    /// <param name="newItem">Item copy data from.</param>
    public delegate void ItemUpdater<in T>(T existItem, T newItem);

    public partial class ObservableList<T>
    {
        private class Updater
        {
            private readonly ObservableList<T> source;
            private readonly IReadOnlyList<T> target;
            private readonly IEqualityComparer<T> comparer;
            private readonly ItemUpdater<T> itemUpdater;
            private readonly int sourceCount, targetCount;
            private readonly int rowCount, columnCount;
            private int[] medMat;
            private int distance;

            public Updater(ObservableList<T> source, IReadOnlyList<T> target, IEqualityComparer<T> comparer, ItemUpdater<T> itemUpdater)
            {
                this.source = source;
                this.target = target;
                this.comparer = comparer;
                this.itemUpdater = itemUpdater;
                this.sourceCount = source.Count;
                this.targetCount = target.Count;
                this.rowCount = this.sourceCount + 1;
                this.columnCount = this.targetCount + 1;
            }

            public int Update()
            {
                if (this.targetCount <= 0)
                {
                    swap();
                    return this.sourceCount;
                }
                if (this.sourceCount <= 0)
                {
                    swap();
                    return this.targetCount;
                }
                if (this.sourceCount * this.targetCount > 1_000_000)
                {
                    // Too large
                    swap();
                    return -1;
                }
                computeMED();
                if (this.sourceCount > this.targetCount ? this.distance == this.sourceCount : this.distance == this.targetCount)
                {
                    // Irrelevant, no need to update step by step
                    swap();
                    return this.distance;
                }
                if (this.itemUpdater is null)
                    swapMEDFast();
                else
                    swapMEDFull();
                return this.distance;
            }

            private void computeMED()
            {
                var mat = new int[this.rowCount * this.columnCount];
                for (var i = 0; i < this.rowCount; i++)
                {
                    mat[i * this.columnCount] = i;
                }
                for (var j = 0; j < this.columnCount; j++)
                {
                    mat[j] = j;
                }
                // compute
                for (var i = 1; i <= this.sourceCount; i++)
                {
                    for (var j = 1; j <= this.targetCount; j++)
                    {
                        var left = mat[(i - 1) * this.columnCount + j];
                        var up = mat[i * this.columnCount + (j - 1)];
                        var diag = mat[(i - 1) * this.columnCount + (j - 1)];
                        var lu_1 = (left < up ? left : up);
                        lu_1++;
                        if (!this.comparer.Equals(this.source.Items[i - 1], this.target[j - 1]))
                            diag++;
                        mat[i * this.columnCount + j] = (lu_1 < diag) ? lu_1 : diag;
                    }
                }

                this.medMat = mat;
                this.distance = mat[mat.Length - 1];
            }

            private void swapMEDFast()
            {
                // init
                var i = this.sourceCount;
                var j = this.targetCount;
                var remainDistance = this.distance;
                while (remainDistance > 0)
                {
                    // if itemUpdater is null, we can finish iterate when remainDistance becomes 0
                    if (i == 0)
                        // Up only
                        goto INSERTION;
                    else if (j == 0)
                        // Left only
                        goto DELETION;
                    else
                    {
                        var left = this.medMat[(i - 1) * this.columnCount + j];
                        var up = this.medMat[i * this.columnCount + (j - 1)];
                        var diag = this.medMat[(i - 1) * this.columnCount + (j - 1)];
                        if (diag <= left && diag <= up)
                        {
                            if (remainDistance == diag)
                                goto DIAG_NO_OPERATION;
                            else
                                goto SUBSTITUTION;
                        }
                        else if (left == remainDistance - 1)
                        {
                            goto DELETION;
                        }
                        else
                        {
                            goto INSERTION;
                        }
                    }

                    DIAG_NO_OPERATION:
                    i--;
                    j--;
                    continue;

                    SUBSTITUTION:
                    i--;
                    j--;
                    this.source[i] = this.target[j];
                    remainDistance--;
                    continue;

                    DELETION:
                    i--;
                    this.source.RemoveAt(i);
                    remainDistance--;
                    continue;

                    INSERTION:
                    j--;
                    this.source.Insert(i, this.target[j]);
                    remainDistance--;
                    continue;
                }
            }

            private void swapMEDFull()
            {
                // init
                var i = this.sourceCount;
                var j = this.targetCount;
                var remainDistance = this.distance;
                while (i > 0 || j > 0)
                {
                    // if itemUpdater is not null, we must iterate all common elements regardless of the remainDistance
                    if (i == 0)
                        // Up only
                        goto INSERTION;
                    else if (j == 0)
                        // Left only
                        goto DELETION;
                    else
                    {
                        var left = this.medMat[(i - 1) * this.columnCount + j];
                        var up = this.medMat[i * this.columnCount + (j - 1)];
                        var diag = this.medMat[(i - 1) * this.columnCount + (j - 1)];
                        if (diag <= left && diag <= up)
                        {
                            if (remainDistance == diag)
                                goto DIAG_NO_OPERATION;
                            else
                                goto SUBSTITUTION;
                        }
                        else if (left == remainDistance - 1)
                        {
                            goto DELETION;
                        }
                        else
                        {
                            goto INSERTION;
                        }
                    }

                    DIAG_NO_OPERATION:
                    i--;
                    j--;
                    this.itemUpdater(this.source[i], this.target[j]);
                    continue;

                    SUBSTITUTION:
                    i--;
                    j--;
                    this.source[i] = this.target[j];
                    remainDistance--;
                    continue;

                    DELETION:
                    i--;
                    this.source.RemoveAt(i);
                    remainDistance--;
                    continue;

                    INSERTION:
                    j--;
                    this.source.Insert(i, this.target[j]);
                    remainDistance--;
                    continue;
                }
            }

            private void swap()
            {
                using (this.source.SuspendNotification(false))
                {
                    this.source.ClearItems();
                    foreach (var item in this.target)
                    {
                        this.source.Add(item);
                    }
                }
                this.source.OnVectorReset();
                this.source.OnPropertyChanged(EventArgsConst.CountPropertyChanged);
            }
        }

        /// <summary>
        /// Change the content of this <see cref="ObservableList{T}"/> to <paramref name="newList"/> with minimum editing distance.
        /// </summary>
        /// <param name="newList">The target content</param>
        /// <returns>The minimum editing distance of the edit</returns>
        /// <remarks>
        /// If <c><paramref name="newList"/>.<see cref="IReadOnlyCollection{T}.Count"/> * <see cref="Count"/> &gt; 1_000_000</c>,
        /// MED computing will not be executed and -1 will be returned.</remarks>
        public int Update(IReadOnlyList<T> newList)
            => Update(newList, default(IEqualityComparer<T>), null);
        /// <summary>
        /// Change the content of this <see cref="ObservableList{T}"/> to <paramref name="newList"/> with minimum editing distance.
        /// </summary>
        /// <param name="newList">The target content</param>
        /// <param name="comparer">The comparer to compare items in two lists</param>
        /// <returns>The minimum editing distance of the edit</returns>
        /// <remarks>
        /// If <c><paramref name="newList"/>.<see cref="IReadOnlyCollection{T}.Count"/> * <see cref="Count"/> &gt; 1_000_000</c>,
        /// MED computing will not be executed and -1 will be returned.</remarks>
        public int Update(IReadOnlyList<T> newList, IComparer<T> comparer)
            => Update(newList, comparer, null);
        /// <summary>
        /// Change the content of this <see cref="ObservableList{T}"/> to <paramref name="newList"/> with minimum editing distance.
        /// </summary>
        /// <param name="newList">The target content</param>
        /// <param name="comparer">The comparer to compare items in two lists</param>
        /// <returns>The minimum editing distance of the edit</returns>
        /// <remarks>
        /// If <c><paramref name="newList"/>.<see cref="IReadOnlyCollection{T}.Count"/> * <see cref="Count"/> &gt; 1_000_000</c>,
        /// MED computing will not be executed and -1 will be returned.</remarks>
        public int Update(IReadOnlyList<T> newList, IEqualityComparer<T> comparer)
            => Update(newList, comparer, null);
        /// <summary>
        /// Change the content of this <see cref="ObservableList{T}"/> to <paramref name="newList"/> with minimum editing distance.
        /// </summary>
        /// <param name="newList">The target content</param>
        /// <param name="comparison">The comparison to compare items in two lists</param>
        /// <returns>The minimum editing distance of the edit</returns>
        /// <remarks>
        /// If <c><paramref name="newList"/>.<see cref="IReadOnlyCollection{T}.Count"/> * <see cref="Count"/> &gt; 1_000_000</c>,
        /// MED computing will not be executed and -1 will be returned.</remarks>
        public int Update(IReadOnlyList<T> newList, Comparison<T> comparison)
            => Update(newList, Comparer<T>.Create(comparison), null);
        /// <summary>
        /// Change the content of this <see cref="ObservableList{T}"/> to <paramref name="newList"/> with minimum editing distance.
        /// </summary>
        /// <param name="newList">The target content</param>
        /// <param name="comparison">The comparison to compare items in two lists</param>
        /// <param name="itemUpdater">The delegate to move data from elements in <paramref name="newList"/> to elements in this <see cref="ObservableList{T}"/></param>
        /// <returns>The minimum editing distance of the edit</returns>
        /// <remarks>
        /// If <c><paramref name="newList"/>.<see cref="IReadOnlyCollection{T}.Count"/> * <see cref="Count"/> &gt; 1_000_000</c>,
        /// MED computing will not be executed and -1 will be returned.</remarks>
        public int Update(IReadOnlyList<T> newList, Comparison<T> comparison, ItemUpdater<T> itemUpdater)
            => Update(newList, Comparer<T>.Create(comparison), itemUpdater);
        /// <summary>
        /// Change the content of this <see cref="ObservableList{T}"/> to <paramref name="newList"/> with minimum editing distance.
        /// </summary>
        /// <param name="newList">The target content</param>
        /// <param name="comparer">The comparer to compare items in two lists</param>
        /// <param name="itemUpdater">The delegate to move data from elements in <paramref name="newList"/> to elements in this <see cref="ObservableList{T}"/></param>
        /// <returns>The minimum editing distance of the edit</returns>
        /// <remarks>
        /// If <c><paramref name="newList"/>.<see cref="IReadOnlyCollection{T}.Count"/> * <see cref="Count"/> &gt; 1_000_000</c>,
        /// MED computing will not be executed and -1 will be returned.</remarks>
        public int Update(IReadOnlyList<T> newList, IComparer<T> comparer, ItemUpdater<T> itemUpdater)
            => Update(newList, EqualityComparerAdapter.Create(comparer), itemUpdater);
        /// <summary>
        /// Change the content of this <see cref="ObservableList{T}"/> to <paramref name="newList"/> with minimum editing distance.
        /// </summary>
        /// <param name="newList">The target content</param>
        /// <param name="comparer">The comparer to compare items in two lists</param>
        /// <param name="itemUpdater">The delegate to move data from elements in <paramref name="newList"/> to elements in this <see cref="ObservableList{T}"/></param>
        /// <returns>The minimum editing distance of the edit</returns>
        /// <remarks>
        /// If <c><paramref name="newList"/>.<see cref="IReadOnlyCollection{T}.Count"/> * <see cref="Count"/> &gt; 1_000_000</c>,
        /// MED computing will not be executed and -1 will be returned.</remarks>
        public int Update(IReadOnlyList<T> newList, IEqualityComparer<T> comparer, ItemUpdater<T> itemUpdater)
        {
            if (newList == null)
                throw new ArgumentNullException(nameof(newList));
            if (isSameRef(newList))
                return 0;
            comparer = comparer ?? EqualityComparer<T>.Default;
            return new Updater(this, newList, comparer, itemUpdater).Update();
        }

        private bool isSameRef(object collection)
        {
            if (collection is null)
                return false;
            return ReferenceEquals(collection, this)
                || ReferenceEquals(collection, Items)
                || (collection is ObservableListView<T> view && ReferenceEquals(view.List, this));
        }
    }
}
