using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace Opportunity.MvvmUniverse.Storage
{
    public abstract class StorageProperty
    {
        internal StorageProperty() { }

        public static StorageProperty<T> Create<T>(string propertyName)
            => new StorageProperty<T>(propertyName);
        public static StorageProperty<T> Create<T>(string propertyName, T defaultValue)
            => new StorageProperty<T>(propertyName, defaultValue);
        public static StorageProperty<T> Create<T>(string propertyName, T defaultValue, StoragePropertyChangedCallback<T> callback) => new StorageProperty<T>(propertyName, defaultValue, callback);
    }

    internal interface IStorageProperty
    {
        string Name { get; }
        Type PropertyType { get; }
        object ToStorage(object value);
        object FromStorage(object value);
        object DefaultValue { get; }
        object GetTypeDefault();
        bool IsValueValid(object value);
        void RaisePropertyChanged(StorageObject sender, object oldValue, object newValue);
        bool Equals(object value1, object value2);
    }

    public sealed class StorageProperty<T> : StorageProperty, IStorageProperty
    {
        public StorageProperty(string propertyName)
            : this(propertyName, default, null) { }

        internal StorageProperty(string propertyName, T def)
            : this(propertyName, def, null) { }

        internal StorageProperty(string propertyName, StoragePropertyChangedCallback<T> callback)
            : this(propertyName, default, callback) { }

        internal StorageProperty(string propertyName, T def, StoragePropertyChangedCallback<T> callback)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException(nameof(propertyName));
            this.Name = propertyName;
            this.DefaultValue = def;
            this.PropertyChangedCallback = callback;
        }

        public T DefaultValue { get; }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object IStorageProperty.DefaultValue => DefaultValue;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IEqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
        public IEqualityComparer<T> EqualityComparer
        {
            get => this.equalityComparer;
            set => this.equalityComparer = value ?? EqualityComparer<T>.Default;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ISerializer<T> serializer = Serializer<T>.Default;
        public ISerializer<T> Serializer
        {
            get => this.serializer;
            set => this.serializer = value ?? Serializer<T>.Default;
        }

        public StoragePropertyChangedCallback<T> PropertyChangedCallback { get; }

        public string Name { get; }

        public Type PropertyType => typeof(T);
        object IStorageProperty.GetTypeDefault() => default(T);
        bool IStorageProperty.IsValueValid(object value)
        {
            if (value is T)
                return true;
            return (value == null && default(T) == null);
        }

        internal void RaisePropertyChanged(StorageObject sender, T oldValue, T newValue)
        {
            var cb = PropertyChangedCallback;
            if (cb == null)
                return;
            var arg = new StoragePropertyChangedEventArgs<T>(this, oldValue, newValue);
            DispatcherHelper.BeginInvoke(() => cb(sender, arg));
        }
        void IStorageProperty.RaisePropertyChanged(StorageObject sender, object oldValue, object newValue)
             => RaisePropertyChanged(sender, (T)oldValue, (T)newValue);

        bool IStorageProperty.Equals(object value1, object value2) => this.equalityComparer.Equals((T)value1, (T)value2);
        internal bool Equals(T value1, T value2) => this.equalityComparer.Equals(value1, value2);

        object IStorageProperty.ToStorage(object value) => this.serializer.Serialize((T)value);
        internal object ToStorage(T value) => this.serializer.Serialize(value);

        object IStorageProperty.FromStorage(object value) => FromStorage(value);
        internal T FromStorage(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            return this.serializer.Deserialize(value);
        }
    }
}
