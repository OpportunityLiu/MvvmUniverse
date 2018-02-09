using Opportunity.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Windows.Storage;

namespace Opportunity.MvvmUniverse.Storage
{
    public interface IStorageProperty : INotifyPropertyChanged
    {
        /// <summary>
        /// Write to storage.
        /// </summary>
        void Flush();
        /// <summary>
        /// Load from storage.
        /// </summary>
        void Populate();

        ApplicationDataLocality Locality { get; }

        object Value { get; set; }

        Type ValueType { get; }
    }

    internal static class DataContainerStorage
    {
        internal static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
        internal static readonly ApplicationDataContainer RoamingSettings = ApplicationData.Current.RoamingSettings;
        internal static readonly char[] PathSep = "\\/".ToCharArray();
    }

    [DebuggerDisplay(@"{Proxy.ToString(this),nq}")]
    [DebuggerTypeProxy(typeof(StorageProperty<>.Proxy))]
    public sealed class StorageProperty<T> : StorageProperty, IStorageProperty
    {
        private class Proxy
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly StorageProperty<T> parent;

            public Proxy(StorageProperty<T> value)
            {
                this.parent = value;
            }

            private static bool defaultLoaded(StorageProperty<T> that)
            {
                if (that.cache != null)
                    return true;
                var c = that.defaultValueCreator;
                if (c != null)
                    return false;
                return that.defaultValue == null;
            }

            public static string ToString(StorageProperty<T> that)
            {
                if (!defaultLoaded(that))
                    return "{value not initialized}";
                var c = that.cache;
                if (c == null)
                    return "{null}";
                return $"{{{c}}}";
            }

            public ApplicationDataLocality Locality => this.parent.Locality;

            public IEqualityComparer<T> EqualityComparer => this.parent.EqualityComparer;
            public ISerializer<T> Serializer => this.parent.Serializer;

            public object DefaultValue => (object)this.parent.defaultValueCreator ?? this.parent.defaultValue;

            public byte[] Data => this.parent.Data;

            public T Value { get => this.parent.cache; set => this.parent.Value = value; }

            public StoragePropertyChangedCallback<T> PropertyChangedCallback => this.parent.PropertyChangedCallback;

            public string Name => this.parent.name;
            public ApplicationDataContainer Container => this.parent.container;
        }

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
                con = DataContainerStorage.LocalSettings;
                break;
            case ApplicationDataLocality.Roaming:
                con = DataContainerStorage.RoamingSettings;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(locality), "Must be Roaming or Local");
            }
            var sec = path.Split(DataContainerStorage.PathSep, StringSplitOptions.RemoveEmptyEntries);
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
            if (this.Locality == ApplicationDataLocality.Roaming)
                RoamingStoragePropertyManager.RoamingProperties.Add(new WeakReference<IStorageProperty>(this));
        }

        private readonly ApplicationDataContainer container;
        private readonly string name;

        public ApplicationDataLocality Locality => this.container.Locality;

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

        object IStorageProperty.Value { get => Value; set => Value = (T)value; }

        Type IStorageProperty.ValueType => typeof(T);

        private void raiseChanged(T oldValue, T newValue)
        {
            if (TypeTraits.Of<T>().Type.IsValueType || !ReferenceEquals(oldValue, newValue))
                OnPropertyChanged(nameof(Value));
            PropertyChangedCallback?.Invoke(this, new StoragePropertyChangedEventArgs<T>(oldValue, newValue));
        }

        private bool populateCore(bool ignoreEmpty)
        {
            var storage = Data;
            var span = default(Span<byte>);
            if (storage == null)
            {
                if (ignoreEmpty)
                    return false;
                span = Span<byte>.Empty;
            }
            else
                span = new Span<byte>(storage);
            this.Serializer.Deserialize(span, ref this.Cache);
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
            if (size <= 0)
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