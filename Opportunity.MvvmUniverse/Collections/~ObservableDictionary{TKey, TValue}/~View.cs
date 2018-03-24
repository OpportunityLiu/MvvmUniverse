﻿using Opportunity.MvvmUniverse.Collections.Internal;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Interop;
using Windows.Foundation.Collections;
using System.Threading;
using System.ComponentModel;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// Read only view of <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">Type of keys.</typeparam>
    /// <typeparam name="TValue">Type of values.</typeparam>
    [DebuggerTypeProxy(typeof(DictionaryDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    public class ObservableDictionaryView<TKey, TValue> : ObservableCollectionBase<KeyValuePair<TKey, TValue>>
        , IReadOnlyDictionary<TKey, TValue>, IOrderedDictionary
        , IList<KeyValuePair<TKey, TValue>>, IReadOnlyList<KeyValuePair<TKey, TValue>>, IList
        , ICollection<KeyValuePair<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, ICollection
        , IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
        , IDisposable
    {
        private ObservableDictionary<TKey, TValue> dictionary;
        /// <summary>
        /// <see cref="ObservableDictionary{TKey, TValue}"/> of this view.
        /// </summary>
        protected internal ObservableDictionary<TKey, TValue> Dictionary
        {
            get
            {
                var dic = this.dictionary;
                if (dic == null)
                    throw new InvalidOperationException("Instance disposed.");
                return dic;
            }
        }

        /// <summary>
        /// Create new instance of <see cref="ObservableDictionaryView{TKey, TValue}"/>.
        /// </summary>
        /// <param name="dictionary"><see cref="ObservableDictionary{TKey, TValue}"/> of this view.</param>
        public ObservableDictionaryView(ObservableDictionary<TKey, TValue> dictionary)
        {
            this.dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
            dictionary.PropertyChanged += this.onDictionaryPropertyChanged;
            dictionary.VectorChanged += this.onDictionaryVectorChanged;
        }

        private void onDictionaryPropertyChanged(object _, PropertyChangedEventArgs e)
        {
            OnDictionaryPropertyChanged(e);
        }

        /// <summary>
        /// Event handler for <see cref="INotifyPropertyChanged.PropertyChanged"/> of <see cref="Dictionary"/>.
        /// </summary>
        /// <param name="e">Event args.</param>
        protected virtual void OnDictionaryPropertyChanged(PropertyChangedEventArgs e)
        {
            if (NeedRaisePropertyChanged)
                OnPropertyChanged(new SinglePropertyChangedEventArgsSource(e));
        }

        private void onDictionaryVectorChanged(IBindableObservableVector _, object e)
        {
            OnDictionaryVectorChanged((IVectorChangedEventArgs)e);
        }

        /// <summary>
        /// Event handler for <see cref="IBindableObservableVector.VectorChanged"/> of <see cref="Dictionary"/>.
        /// </summary>
        /// <param name="e">Event args.</param>
        protected virtual void OnDictionaryVectorChanged(IVectorChangedEventArgs e)
        {
            if (NeedRaiseVectorChanged)
                OnVectorChanged(e);
        }

        /// <summary>
        /// Set <see cref="Dictionary"/> to <see langword="null"/> and unsubscribe events.
        /// </summary>
        public virtual void Dispose()
        {
            var dic = Interlocked.Exchange(ref this.dictionary, null);
            if (dic == null)
                return;
            dic.VectorChanged -= this.onDictionaryVectorChanged;
            dic.PropertyChanged -= this.onDictionaryPropertyChanged;
        }

        /// <inheritdoc/>
        public TValue this[TKey key] => Dictionary[key];

        /// <summary>
        /// Get key-value pair at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of key-value pair.</param>
        /// <returns>Key-value pair at <paramref name="index"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> out of range of the dictionary.</exception>
        public KeyValuePair<TKey, TValue> ItemAt(int index) => Dictionary.ItemAt(index);

        KeyValuePair<TKey, TValue> IReadOnlyList<KeyValuePair<TKey, TValue>>.this[int index]
            => ((IList<KeyValuePair<TKey, TValue>>)Dictionary)[index];
        KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index]
        {
            get => ((IList<KeyValuePair<TKey, TValue>>)this.Dictionary)[index];
            set => ThrowForReadOnlyCollection(Dictionary);
        }

        object IDictionary.this[object key]
        {
            get => ((IDictionary)Dictionary)[key];
            set => ThrowForReadOnlyCollection(Dictionary);
        }
        object IOrderedDictionary.this[int index]
        {
            get => ((IOrderedDictionary)Dictionary)[index];
            set => ThrowForReadOnlyCollection(Dictionary);
        }

        /// <inheritdoc/>
        public int Count => Dictionary.Count;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IDictionary.IsFixedSize => ((IDictionary)Dictionary).IsFixedSize;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IDictionary.IsReadOnly => true;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => true;

        /// <inheritdoc/>
        public ObservableDictionary<TKey, TValue>.ObservableKeyCollection Keys => Dictionary.Keys;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection IDictionary.Keys => Dictionary.Keys;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Dictionary.Keys;

        /// <inheritdoc/>
        public ObservableDictionary<TKey, TValue>.ObservableValueCollection Values => Dictionary.Values;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection IDictionary.Values => Dictionary.Values;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Dictionary.Values;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => ((ICollection)Dictionary).IsSynchronized;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => ((ICollection)Dictionary).SyncRoot;

        /// <inheritdoc/>
        public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);
        bool IDictionary.Contains(object key) => ((IDictionary)Dictionary).Contains(key);
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
            => ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).Contains(item);

        int IList<KeyValuePair<TKey, TValue>>.IndexOf(KeyValuePair<TKey, TValue> item) => ((IList<KeyValuePair<TKey, TValue>>)this.Dictionary).IndexOf(item);

        /// <inheritdoc/>
        public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value);

        void IDictionary.Add(object key, object value) => ThrowForReadOnlyCollection(Dictionary);
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => ThrowForReadOnlyCollection(Dictionary);

        void IOrderedDictionary.Insert(int index, object key, object value) => ThrowForReadOnlyCollection(Dictionary);
        void IList<KeyValuePair<TKey, TValue>>.Insert(int index, KeyValuePair<TKey, TValue> item) => ThrowForReadOnlyCollection(Dictionary);

        void IDictionary.Clear() => ThrowForReadOnlyCollection(Dictionary);
        void ICollection<KeyValuePair<TKey, TValue>>.Clear() => ThrowForReadOnlyCollection(Dictionary);

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => Dictionary.CopyTo(array, arrayIndex);

        void IDictionary.Remove(object key) => ThrowForReadOnlyCollection(Dictionary);
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => ThrowForReadOnlyCollection<bool>(Dictionary);
        void IOrderedDictionary.RemoveAt(int index) => ThrowForReadOnlyCollection(Dictionary);
        void IList<KeyValuePair<TKey, TValue>>.RemoveAt(int index) => ThrowForReadOnlyCollection(Dictionary);

        /// <inheritdoc/>
        public ObservableDictionary<TKey, TValue>.DictionaryEnumerator GetEnumerator() => Dictionary.GetEnumerator();
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>)Dictionary).GetEnumerator();
        IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)Dictionary).GetEnumerator();
        IDictionaryEnumerator IOrderedDictionary.GetEnumerator() => ((IOrderedDictionary)Dictionary).GetEnumerator();

        /// <summary>
        /// Iterate all key-value pairs in the dictionary.
        /// </summary>
        /// <param name="action">Action for each key-value pair.</param>
        public void ForEach(Action<TKey, TValue> action) => Dictionary.ForEach(action);
        /// <summary>
        /// Iterate all key-value pairs and their index in the dictionary.
        /// </summary>
        /// <param name="action">Action for each key-value pair and its index.</param>
        public void ForEach(Action<int, TKey, TValue> action) => Dictionary.ForEach(action);
    }

    internal sealed class UndisposableObservableDictionaryView<TKey, TValue> : ObservableDictionaryView<TKey, TValue>
    {
        public UndisposableObservableDictionaryView(ObservableDictionary<TKey, TValue> dictionary)
            : base(dictionary) { }

        public override void Dispose() { }
    }
}
