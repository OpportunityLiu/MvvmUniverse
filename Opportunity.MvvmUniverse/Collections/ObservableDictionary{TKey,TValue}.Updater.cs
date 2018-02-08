using Opportunity.MvvmUniverse.Collections.Internal;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Collections
{
    public partial class ObservableDictionary<TKey, TValue>
    {
        private class Updater
        {
            private readonly ObservableDictionary<TKey, TValue> source;
            private readonly TKey[] targetKey;
            private readonly TValue[] targetValue;
            private readonly HashSet<TKey> targetKeySet;
            private readonly IEqualityComparer<TKey> comparer;
            private readonly ItemUpdater<TKey> keyUpdater;
            private readonly ItemUpdater<TValue> valueUpdater;
            private readonly int sourceCount, targetCount;
            private readonly int rowCount, columnCount;
            private int[] medMat;
            private int distance;

            public Updater(ObservableDictionary<TKey, TValue> source, IReadOnlyDictionary<TKey, TValue> target, ItemUpdater<TKey> keyUpdater, ItemUpdater<TValue> valueUpdater)
            {
                this.keyUpdater = keyUpdater;
                this.valueUpdater = valueUpdater;
                this.comparer = this.source.Comparer;
                this.sourceCount = source.Count;
                this.targetCount = target.Count;
                this.rowCount = this.sourceCount + 1;
                this.columnCount = this.targetCount + 1;
                this.source = source;
                this.targetKeySet = new HashSet<TKey>(target.Keys, this.comparer);
                this.targetKey = new TKey[this.targetCount];
                this.targetValue = new TValue[this.targetCount];
                var i = 0;
                foreach (var item in target)
                {
                    this.targetKey[i] = item.Key;
                    this.targetValue[i] = item.Value;
                    i++;
                }
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
                var rStep = remove();
                var sStep = sync();
                return rStep + sStep;
            }

            private int remove()
            {
                var edit = 0;
                for (var i = this.sourceCount - 1; i >= 0; i--)
                {
                    var key = this.source.KeyItems[i];
                    if (!this.targetKeySet.Contains(key))
                    {
                        this.source.RemoveItem(key);
                        edit++;
                    }
                }
                return edit;
            }

            private int sync()
            {
                var edit = 0;
                for (var i = 0; i < this.targetCount; i++)
                {
                    var sourceKey = this.source.KeyItems[i];
                    var targetKey = this.targetKey[i];
                    if (this.comparer.Equals(sourceKey, targetKey))
                    {
                        // Match at right posiiton.
                        syncData(in sourceKey, this.source.ValueItems[i], in targetKey, in this.targetValue[i]);
                    }
                    else if (this.source.ContainsKey(targetKey))
                    {
                        // Must be found after posiiton i. Move forward.
                        this.source.MoveItem(targetKey, i);

                        syncData(this.source.KeyItems[i], this.source.ValueItems[i], in targetKey, in this.targetValue[i]);
                    }
                    else
                    {
                        // Not found. Insert.
                        this.source.InsertItem(i, targetKey, this.targetValue[i]);
                    }
                }
                return edit;
            }

            private void syncData(in TKey sk, in TValue sv, in TKey tk, in TValue tv)
            {
                if (this.valueUpdater != null && sv != null && tv != null)
                {
                    this.valueUpdater.Invoke(sv, tv);
                }
                else
                {
                    this.source.SetItem(sk, tv);
                }
                this.keyUpdater?.Invoke(sk, tk);
            }

            private void swap()
            {
                this.source.ClearItems();
                for (var i = 0; i < this.targetKey.Length; i++)
                {
                    this.source.InsertItem(i, this.targetKey[i], this.targetValue[i]);
                }
            }
        }

        /// <summary>
        /// Change the content of this <see cref="ObservableDictionary{TKey, TValue}"/> to <paramref name="newDictionary"/> with minimum editing distance.
        /// </summary>
        /// <param name="newDictionary">The target content</param>
        /// <returns>The minimum editing distance of the edit</returns>
        public int Update(IReadOnlyDictionary<TKey, TValue> newDictionary) => Update(newDictionary, null, null);

        /// <summary>
        /// Change the content of this <see cref="ObservableDictionary{TKey, TValue}"/> to <paramref name="newDictionary"/> with minimum editing distance.
        /// </summary>
        /// <param name="newDictionary">The target content</param>
        /// <param name="valueUpdater">The delegate to move data from values in <paramref name="newDictionary"/> to values in this <see cref="ObservableDictionary{TKey,TValue}"/></param>
        /// <returns>The minimum editing distance of the edit</returns>
        public int Update(IReadOnlyDictionary<TKey, TValue> newDictionary, ItemUpdater<TValue> valueUpdater) => Update(newDictionary, null, valueUpdater);

        /// <summary>
        /// Change the content of this <see cref="ObservableDictionary{TKey, TValue}"/> to <paramref name="newDictionary"/> with minimum editing distance.
        /// </summary>
        /// <param name="newDictionary">The target content</param>
        /// <param name="keyUpdater">The delegate to move data from keys in <paramref name="newDictionary"/> to keys in this <see cref="ObservableDictionary{TKey,TValue}"/></param>
        /// <returns>The minimum editing distance of the edit</returns>
        public int Update(IReadOnlyDictionary<TKey, TValue> newDictionary, ItemUpdater<TKey> keyUpdater) => Update(newDictionary, keyUpdater, null);

        /// <summary>
        /// Change the content of this <see cref="ObservableDictionary{TKey, TValue}"/> to <paramref name="newDictionary"/> with minimum editing distance.
        /// </summary>
        /// <param name="newDictionary">The target content</param>
        /// <param name="keyUpdater">The delegate to move data from keys in <paramref name="newDictionary"/> to keys in this <see cref="ObservableDictionary{TKey,TValue}"/></param>
        /// <param name="valueUpdater">The delegate to move data from values in <paramref name="newDictionary"/> to values in this <see cref="ObservableDictionary{TKey,TValue}"/></param>
        /// <returns>The minimum editing distance of the edit</returns>
        public int Update(IReadOnlyDictionary<TKey, TValue> newDictionary, ItemUpdater<TKey> keyUpdater, ItemUpdater<TValue> valueUpdater)
        {
            if (newDictionary == null)
                throw new ArgumentNullException(nameof(newDictionary));
            if (isSameRef(newDictionary))
                return 0;
            return new Updater(this, newDictionary, keyUpdater, valueUpdater).Update();
        }

        private bool isSameRef(IReadOnlyDictionary<TKey, TValue> newDictionary)
        {
            if (newDictionary == null)
                return false;
            return ReferenceEquals(newDictionary, this)
                || (newDictionary is ObservableDictionaryView<TKey, TValue> view && ReferenceEquals(view.Dictionary, this));
        }
    }
}
