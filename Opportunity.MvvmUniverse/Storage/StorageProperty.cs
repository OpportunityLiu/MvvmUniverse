using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using Windows.Storage;

namespace Opportunity.MvvmUniverse.Storage
{
    public abstract class StorageProperty : ObservableObject
    {
        internal StorageProperty() { }

        public static StorageProperty<T> CreateLocal<T>(
            string path,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, default, null, equalityComparer, serializer);

        public static StorageProperty<T> CreateLocal<T>(
            string path,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, default, callback, equalityComparer, serializer);

        public static StorageProperty<T> CreateLocal<T>(
            string path,
            T defaultValue,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, defaultValue, null, equalityComparer, serializer);

        public static StorageProperty<T> CreateLocal<T>(
            string path,
            T defaultValue,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, defaultValue, callback, equalityComparer, serializer);

        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, default, null, equalityComparer, serializer);

        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, default, callback, equalityComparer, serializer);

        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            T defaultValue,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, defaultValue, null, equalityComparer, serializer);

        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            T defaultValue,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, defaultValue, callback, equalityComparer, serializer);

        public static StorageProperty<T> Create<T>(
            ApplicationDataLocality locality, string path,
            T defaultValue,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(locality, path, defaultValue, callback, equalityComparer, serializer);
    }

    internal interface IStorageProperty
    {
        void Flush();
        void Populate();
    }

    public sealed class StorageProperty<T> : StorageProperty, IStorageProperty
    {
        internal StorageProperty(
            ApplicationDataLocality locality,
            string path,
            T def = default,
            StoragePropertyChangedCallback<T> callback = null,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            var con = default(ApplicationDataContainer);
            switch (locality)
            {
            case ApplicationDataLocality.Local:
                con = ApplicationData.Current.LocalSettings;
                break;
            case ApplicationDataLocality.Roaming:
                con = ApplicationData.Current.RoamingSettings;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(locality), "Must be Roaming or Local");
            }
            var sec = path.Split(new[] { '\\', '/' }, options: StringSplitOptions.RemoveEmptyEntries);
            if (sec.Length == 0)
                throw new ArgumentException("Not a valid path", nameof(path));
            this.name = sec[sec.Length - 1];
            for (var i = 0; i < sec.Length - 1; i++)
            {
                con = con.CreateContainer(sec[i], ApplicationDataCreateDisposition.Always);
            }
            this.container = con;
            try
            {
                this.Serializer = serializer ?? Serializer<T>.Default;
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException($"Failed to generate default serializer by Serializer<{typeof(T)}>.Default, must specify by parameter.", ex);
            }
            this.DefaultValue = def;
            this.EqualityComparer = equalityComparer ?? EqualityComparer<T>.Default;
            if (!populateCore(true))
            {
                this.cache = def;
                Flush();
            }
            this.PropertyChangedCallback = callback;
        }

        private readonly ApplicationDataContainer container;
        private readonly string name;

        public IEqualityComparer<T> EqualityComparer { get; }
        public ISerializer<T> Serializer { get; }

        public T DefaultValue { get; }

        private byte[] Data
        {
            get
            {
                this.container.Values.TryGetValue(this.name, out var storage);
                return storage as byte[];
            }
            set
            {
                if (value == null || value.Length == 0)
                    this.container.Values.Remove(this.name);
                else
                    this.container.Values[this.name] = value;
            }
        }

        private T cache;
        public T Value
        {
            get => this.cache;
            set
            {
                var old = this.cache;
                this.cache = value;
                Flush();
                raiseChanged(old, value);
            }
        }

        private void raiseChanged(T oldValue, T newValue)
        {
            if (!ReferenceEquals(oldValue, newValue))
                OnPropertyChanged(nameof(Value));
            PropertyChangedCallback?.Invoke(this, new StoragePropertyChangedEventArgs<T>(oldValue, newValue));
        }

        private bool populateCore(bool ignoreEmpty)
        {
            var storage = Data;
            if (storage == null)
            {
                if (ignoreEmpty)
                    return false;
                storage = Array.Empty<byte>();
            }
            this.Serializer.Deserialize(storage, ref this.cache);
            return true;
        }

        /// <summary>
        /// Load from storage.
        /// </summary>
        public void Populate()
        {
            var old = this.cache;
            populateCore(false);
            raiseChanged(old, this.cache);
        }

        /// <summary>
        /// Write to storage.
        /// </summary>
        public void Flush()
        {
            var ser = this.Serializer;
            var size = ser.CaculateSize(in this.cache);
            if (size == 0)
            {
                this.container.Values[this.name] = null;
                return;
            }
            var bytes = new byte[size];
            ser.Serialize(in this.cache, bytes);
            Data = bytes;
        }

        public StoragePropertyChangedCallback<T> PropertyChangedCallback { get; }
    }
}
