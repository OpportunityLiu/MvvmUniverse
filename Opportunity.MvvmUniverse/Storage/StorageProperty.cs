using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using Windows.Storage;
using System.Threading;

namespace Opportunity.MvvmUniverse.Storage
{
    public abstract class StorageProperty : ObservableObject
    {
        internal StorageProperty() { }

        public static StorageProperty<T> CreateLocal<T>(
            string path,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, default, null, null, equalityComparer, serializer);

        public static StorageProperty<T> CreateLocal<T>(
            string path,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, default, null, callback, equalityComparer, serializer);

        public static StorageProperty<T> CreateLocal<T>(
            string path,
            T defaultValue,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, defaultValue, null, null, equalityComparer, serializer);

        public static StorageProperty<T> CreateLocal<T>(
            string path,
            T defaultValue,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, defaultValue, null, callback, equalityComparer, serializer);

        public static StorageProperty<T> CreateLocal<T>(
            string path,
            Func<T> defaultValueCreator,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, default, defaultValueCreator ?? throw new ArgumentNullException(nameof(defaultValueCreator)), null, equalityComparer, serializer);

        public static StorageProperty<T> CreateLocal<T>(
            string path,
            Func<T> defaultValueCreator,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, default, defaultValueCreator ?? throw new ArgumentNullException(nameof(defaultValueCreator)), callback, equalityComparer, serializer);

        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, default, null, null, equalityComparer, serializer);

        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, default, null, callback, equalityComparer, serializer);

        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            T defaultValue,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, defaultValue, null, null, equalityComparer, serializer);

        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            T defaultValue,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, defaultValue, null, callback, equalityComparer, serializer);

        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            Func<T> defaultValueCreator,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, default, defaultValueCreator ?? throw new ArgumentNullException(nameof(defaultValueCreator)), null, equalityComparer, serializer);

        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            Func<T> defaultValueCreator,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, default, defaultValueCreator ?? throw new ArgumentNullException(nameof(defaultValueCreator)), callback, equalityComparer, serializer);

        public static StorageProperty<T> Create<T>(
            ApplicationDataLocality locality, string path,
            T defaultValue,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(locality, path, defaultValue, null, callback, equalityComparer, serializer);

        public static StorageProperty<T> Create<T>(
            ApplicationDataLocality locality, string path,
            Func<T> defaultValueCreator,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(locality, path, default, defaultValueCreator ?? throw new ArgumentNullException(nameof(defaultValueCreator)), callback, equalityComparer, serializer);

        internal static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
        internal static readonly ApplicationDataContainer RoamingSettings = ApplicationData.Current.RoamingSettings;
        internal static readonly char[] PathSep = "\\/".ToCharArray();
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
            T def,
            Func<T> defaultValueCreator,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer,
            ISerializer<T> serializer)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            var con = default(ApplicationDataContainer);
            switch (locality)
            {
            case ApplicationDataLocality.Local:
                con = LocalSettings;
                break;
            case ApplicationDataLocality.Roaming:
                con = RoamingSettings;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(locality), "Must be Roaming or Local");
            }
            var sec = path.Split(PathSep, StringSplitOptions.RemoveEmptyEntries);
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
            this.defaultValue = def;
            this.defaultValueCreator = defaultValueCreator;
            this.EqualityComparer = equalityComparer ?? EqualityComparer<T>.Default;
            if (!populateCore(true))
            {
                Flush();
            }
            this.PropertyChangedCallback = callback;
        }

        private readonly ApplicationDataContainer container;
        private readonly string name;

        public IEqualityComparer<T> EqualityComparer { get; }
        public ISerializer<T> Serializer { get; }

        private readonly Func<T> defaultValueCreator;
        private readonly T defaultValue;

        internal byte[] Data
        {
            get
            {
                this.container.Values.TryGetValue(this.name, out var storage);
                if (!(storage is byte[] s) || s.Length == 0)
                {
                    return null;
                }
                return s;
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
        private ref T Cache
        {
            get
            {
                if (this.cache == null)
                {
                    var c = this.defaultValueCreator;
                    if (c == null)
                    {
                        this.cache = this.defaultValue;
                    }
                    else
                    {
                        lock (c)
                        {
                            if (this.cache == null)
                                this.cache = c();
                        }
                    }
                }
                return ref this.cache;
            }
        }

        public T Value
        {
            get => this.Cache;
            set
            {
                ref var field = ref this.Cache;
                var old = field;
                field = value;
                Flush();
                raiseChanged(old, value);
            }
        }

        private void raiseChanged(T oldValue, T newValue)
        {
            if (typeof(T).IsValueType || !ReferenceEquals(oldValue, newValue))
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
            this.Serializer.Deserialize(storage, ref this.Cache);
            return true;
        }

        /// <summary>
        /// Load from storage.
        /// </summary>
        public void Populate()
        {
            ref var field = ref this.Cache;
            var old = field;
            populateCore(false);
            raiseChanged(old, field);
        }

        /// <summary>
        /// Write to storage.
        /// </summary>
        public void Flush()
        {
            ref var field = ref this.Cache;
            var ser = this.Serializer;
            var size = ser.CaculateSize(in field);
            if (size == 0)
            {
                Data = null;
                return;
            }
            var bytes = new byte[size];
            ser.Serialize(in field, bytes);
            Data = bytes;
        }

        public StoragePropertyChangedCallback<T> PropertyChangedCallback { get; }
    }
}
