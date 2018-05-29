using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Opportunity.MvvmUniverse.Collections
{
    public partial class ObservableDictionary<TKey, TValue>
    {
        /// <summary>
        /// Enumerator of <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        public struct DictionaryEnumerator : IDictionaryEnumerator, IEnumerator<KeyValuePair<TKey, TValue>>
        {
            internal enum Type { Unknown = 0, IDictionaryEnumerator, IEnumeratorKVP }

            private List<TKey>.Enumerator keyEnumerator;
            private List<TValue>.Enumerator valueEnumerator;
            private readonly Type type;

            internal DictionaryEnumerator(ObservableDictionary<TKey, TValue> parent, Type type)
            {
                this.keyEnumerator = parent.KeyItems.GetEnumerator();
                this.valueEnumerator = parent.ValueItems.GetEnumerator();
                this.type = type;
            }

            DictionaryEntry IDictionaryEnumerator.Entry => new DictionaryEntry(Key, Value);

            /// <inheritdoc/>
            public TKey Key => this.keyEnumerator.Current;
            /// <inheritdoc/>
            public TValue Value => this.valueEnumerator.Current;

            object IDictionaryEnumerator.Key => Key;
            object IDictionaryEnumerator.Value => Value;

            /// <inheritdoc/>
            public KeyValuePair<TKey, TValue> Current => CreateKVP(Key, Value);
            object IEnumerator.Current
            {
                get
                {
                    switch (this.type)
                    {
                    case Type.IDictionaryEnumerator: return new DictionaryEntry(Key, Value);
                    case Type.IEnumeratorKVP: return Current;
                    default: throw new InvalidOperationException();
                    }
                }
            }

            void IDisposable.Dispose()
            {
                this.keyEnumerator.Dispose();
                this.valueEnumerator.Dispose();
            }

            /// <inheritdoc/>
            public bool MoveNext()
            {
                var kr = this.keyEnumerator.MoveNext();
                var vr = this.valueEnumerator.MoveNext();
                if (kr == vr)
                    return kr;
                throw new InvalidOperationException("Dictionary has been changed.");
            }

            private static void reset<T>(ref T enumerator)
                where T : IEnumerator
            {
                enumerator.Reset();
            }

            /// <inheritdoc/>
            public void Reset()
            {
                reset(ref this.keyEnumerator);
                reset(ref this.valueEnumerator);
            }
        }
    }
}
