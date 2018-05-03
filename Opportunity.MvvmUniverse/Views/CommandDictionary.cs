using Opportunity.MvvmUniverse.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

        /// <summary>
        /// Get command from <see cref="CommandDictionary"/>.
        /// </summary>
        /// <typeparam name="T">Type of command.</typeparam>
        /// <param name="key">Key of command.</param>
        /// <returns>Command with <paramref name="key"/> from <see cref="Commands"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="KeyNotFoundException"><paramref name="key"/> not found.</exception>
        /// <exception cref="InvalidCastException">Wrong type <typeparamref name="T"/> of command.</exception>
        public T Get<T>([CallerMemberName] string key = null)
            where T : ISysCommand => (T)this[key];

        /// <summary>
        /// Get command from <see cref="CommandDictionary"/>, if not added, create new command, add to the <see cref="CommandDictionary"/> and returns it.
        /// </summary>
        /// <typeparam name="T">Type of command.</typeparam>
        /// <param name="factory">Factory method to create the command.</param>
        /// <param name="key">Key of command.</param>
        /// <returns>Command with <paramref name="key"/> from <see cref="Commands"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="factory"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidCastException">Wrong type <typeparamref name="T"/> of command.</exception>
        public T GetOrAdd<T>(Func<T> factory, [CallerMemberName] string key = null)
            where T : ISysCommand
        {
            if (this.TryGetValue(key, out var c))
                return (T)c;
            var nc = (factory ?? throw new ArgumentNullException(nameof(factory)))();
            this[key] = nc;
            return nc;
        }

        /// <inheritdoc/>
        public ICollection<string> Keys => this.data.Keys;

        /// <inheritdoc/>
        public ICollection<ISysCommand> Values => this.data.Values;

        /// <inheritdoc/>
        public int Count => this.data.Count;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<KeyValuePair<string, ISysCommand>>.IsReadOnly => false;

        /// <summary>
        /// Add command.
        /// </summary>
        /// <typeparam name="T">Type of command.</typeparam>
        /// <param name="factory">Factory method to create the command.</param>
        /// <param name="key">Key of command.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="factory"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Command with same <paramref name="key"/> has been registered.</exception>
        public void Add<T>(Func<T> factory, [CallerMemberName] string key = null)
            where T : ISysCommand
        {
            if (this.ContainsKey(key ?? throw new ArgumentNullException(nameof(key))))
                throw new ArgumentException("Command with same key has been registered.", nameof(key));
            Add(key, (factory ?? throw new ArgumentNullException(nameof(factory)))());
        }

        /// <summary>
        /// Override registered command.
        /// </summary>
        /// <typeparam name="T">Type of command.</typeparam>
        /// <param name="factory">Factory method to create the command.</param>
        /// <param name="key">Key of command.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="factory"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Command with same <paramref name="key"/> has been registered.</exception>
        /// <exception cref="InvalidCastException">Wrong type <typeparamref name="T"/> of command.</exception>
        public void Override<T>(Func<T, T> factory, [CallerMemberName] string key = null)
            where T : ISysCommand
        {
            if (factory is null)
                throw new ArgumentNullException(nameof(factory));
            this.TryGetValue(key, out var r);
            this[key] = factory((T)r);
        }

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

        bool ICollection<KeyValuePair<string, ISysCommand>>.Contains(KeyValuePair<string, ISysCommand> item) => ((IDictionary<string, ISysCommand>)this.data).Contains(item);
        /// <inheritdoc/>
        public bool ContainsKey(string key) => ((IDictionary<string, ISysCommand>)this.data).ContainsKey(key);
        void ICollection<KeyValuePair<string, ISysCommand>>.CopyTo(KeyValuePair<string, ISysCommand>[] array, int arrayIndex) => ((IDictionary<string, ISysCommand>)this.data).CopyTo(array, arrayIndex);
        /// <inheritdoc/>
        public bool Remove([CallerMemberName] string key = null) => this.data.Remove(key);
        bool ICollection<KeyValuePair<string, ISysCommand>>.Remove(KeyValuePair<string, ISysCommand> item) => ((IDictionary<string, ISysCommand>)this.data).Remove(item);
        /// <inheritdoc/>
        public bool TryGetValue(string key, out ISysCommand value) => this.data.TryGetValue(key, out value);

        /// <inheritdoc/>
        public Dictionary<string, ISysCommand>.Enumerator GetEnumerator() => this.data.GetEnumerator();
        IEnumerator<KeyValuePair<string, ISysCommand>> IEnumerable<KeyValuePair<string, ISysCommand>>.GetEnumerator() => this.data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.data.GetEnumerator();
    }
}
