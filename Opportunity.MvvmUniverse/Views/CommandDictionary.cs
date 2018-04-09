using Opportunity.MvvmUniverse.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISysCommand = System.Windows.Input.ICommand;


namespace Opportunity.MvvmUniverse.Views
{
    /// <summary>
    /// Dictionay of <see cref="ISysCommand"/>.
    /// </summary>
    public sealed class CommandDictionary : IDictionary<string, ISysCommand>
    {
        private Dictionary<string, ISysCommand> data = new Dictionary<string, ISysCommand>();
        private readonly object tag;

        internal CommandDictionary(object tag)
        {
            this.tag = tag;
        }

        /// <inheritdoc/>
        public ISysCommand this[string key]
        {
            get => this.data[key];
            set
            {
                if (value is IControllable c)
                    c.Tag = this.tag;
                this.data[key] = value;
            }
        }

        /// <inheritdoc/>
        public ICollection<string> Keys => this.data.Keys;

        /// <inheritdoc/>
        public ICollection<ISysCommand> Values => this.data.Values;

        /// <inheritdoc/>
        public int Count => this.data.Count;

        bool ICollection<KeyValuePair<string, ISysCommand>>.IsReadOnly => false;

        /// <inheritdoc/>
        public void Add(string key, ISysCommand value)
        {
            if (value is IControllable c)
                c.Tag = this.tag;
            this.data.Add(key, value);
        }

        void ICollection<KeyValuePair<string, ISysCommand>>.Add(KeyValuePair<string, ISysCommand> item)
        {
            if (item.Value is IControllable c)
                c.Tag = this.tag;
            ((IDictionary<string, ISysCommand>)this.data).Add(item);
        }

        /// <inheritdoc/>
        public void Clear() => this.data.Clear();
        /// <inheritdoc/>
        public bool Contains(KeyValuePair<string, ISysCommand> item) => ((IDictionary<string, ISysCommand>)this.data).Contains(item);
        /// <inheritdoc/>
        public bool ContainsKey(string key) => ((IDictionary<string, ISysCommand>)this.data).ContainsKey(key);
        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<string, ISysCommand>[] array, int arrayIndex) => ((IDictionary<string, ISysCommand>)this.data).CopyTo(array, arrayIndex);
        /// <inheritdoc/>
        public bool Remove(string key) => this.data.Remove(key);
        bool ICollection<KeyValuePair<string, ISysCommand>>.Remove(KeyValuePair<string, ISysCommand> item) => ((IDictionary<string, ISysCommand>)this.data).Remove(item);
        /// <inheritdoc/>
        public bool TryGetValue(string key, out ISysCommand value) => this.data.TryGetValue(key, out value);

        /// <inheritdoc/>
        public Dictionary<string, ISysCommand>.Enumerator GetEnumerator() => this.data.GetEnumerator();
        IEnumerator<KeyValuePair<string, ISysCommand>> IEnumerable<KeyValuePair<string, ISysCommand>>.GetEnumerator() => this.data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.data.GetEnumerator();
    }
}
