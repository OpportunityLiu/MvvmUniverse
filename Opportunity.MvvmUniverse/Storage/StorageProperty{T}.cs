using Opportunity.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Opportunity.MvvmUniverse.Storage
{
    /// <summary>
    /// Non-generic interface for <see cref="StorageProperty{T}"/>.
    /// </summary>
    public interface IStorageProperty : INotifyPropertyChanged
    {
        /// <summary>
        /// Write to storage.
        /// </summary>
        void Flush();
        /// <summary>
        /// Clear storage.
        /// </summary>
        void Delete();
        /// <summary>
        /// Load from storage.
        /// </summary>
        /// <returns>Whether data readed successfully.</returns>
        bool Populate();

        /// <summary>
        /// Locality of <see cref="Value"/>'s storage.
        /// </summary>
        ApplicationDataLocality Locality { get; }

        /// <summary>
        /// Value of the <see cref="IStorageProperty"/>.
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Defined type of <see cref="Value"/>.
        /// </summary>
        Type ValueType { get; }
    }

    internal static class DataContainerStorage
    {
        internal static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
        internal static readonly ApplicationDataContainer RoamingSettings = ApplicationData.Current.RoamingSettings;
        internal static readonly char[] PathSep = "\\/".ToCharArray();
    }

    /// <summary>
    /// Represents a value with backing field held by <see cref="ApplicationData"/>.
    /// </summary>
    /// <typeparam name="T">Type of value.</typeparam>
    [DebuggerDisplay(@"{Proxy.ToString(this),nq}")]
    [DebuggerTypeProxy(typeof(StorageProperty<>.Proxy))]
    public sealed class StorageProperty<T> : ObservableObject, IStorageProperty
    {
        private class Proxy
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly StorageProperty<T> parent;

            public Proxy(StorageProperty<T> value)
            {
                this.parent = value;
            }

            public static string ToString(StorageProperty<T> that)
            {
                var c = that.value;
                if (c == null)
                    return "{null}";
                return $"{{{c}}}";
            }

            public string Name => this.parent.name;
            public ApplicationDataContainer Container => this.parent.container;
            public ApplicationDataLocality Locality => this.parent.Locality;

            public Func<T> DefaultValueCreater => this.parent.defaultValueCreater;
            public bool ValueCreated => this.parent.valueCreated;

            public IEqualityComparer<T> EqualityComparer => this.parent.EqualityComparer;
            public ISerializer<T> Serializer => this.parent.Serializer;

            public byte[] Data => this.parent.Data;
            public T Value => this.parent.value;

            public StoragePropertyChangedCallback<T> PropertyChangedCallback => this.parent.propertyChangedCallback;
        }

        private static void getContainer(ApplicationDataLocality locality, string path, out ApplicationDataContainer container, out string name)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            switch (locality)
            {
            case ApplicationDataLocality.Local:
                container = DataContainerStorage.LocalSettings;
                break;
            case ApplicationDataLocality.Roaming:
                container = DataContainerStorage.RoamingSettings;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(locality), "Must be Roaming or Local");
            }
            var sec = path.Split(DataContainerStorage.PathSep, StringSplitOptions.RemoveEmptyEntries);
            if (sec.Length == 0)
                throw new ArgumentException("Not a valid path", nameof(path));
            name = sec[sec.Length - 1];
            for (var i = 0; i < sec.Length - 1; i++)
            {
                container = container.CreateContainer(sec[i], ApplicationDataCreateDisposition.Always);
            }
        }

        private static class ActivatorFactory
        {
            public readonly static Func<T> Activator = Default;

            private static string EmptyString() => "";

            private static T Default() => default;

            private static T UseActivator() => System.Activator.CreateInstance<T>();

            private static readonly ConstructorInfo constructor;
            private static T UseConstucterWithNoParameters() => (T)constructor.Invoke(Array.Empty<object>());

            static ActivatorFactory()
            {
                var tt = TypeTraits.Of<T>();
                if (tt.IsNullable || !tt.CanBeNull)
                {
                    return;
                }
                if (tt.Type.AsType() == typeof(string))
                {
                    Activator = (Func<T>)(object)new Func<string>(EmptyString);
                    return;
                }
                try
                {
                    UseActivator();
                    Activator = UseActivator;
                    return;
                }
                catch (Exception) { };
                try
                {
                    var c = typeof(T).GetInfo().Type.DeclaredConstructors.FirstOrDefault(i => i.GetParameters().Length == 0);
                    if (c != null)
                    {
                        c.Invoke(Array.Empty<object>());
                        constructor = c;
                        Activator = UseConstucterWithNoParameters;
                        return;
                    }
                }
                catch (Exception) { }
            }

            private class ValueActivator
            {
                private readonly T value;

                public T FromValue() => this.value;

                public ValueActivator(T v) => this.value = v;
            }

            public static Func<T> GetActivator(T def)
            {
                if (def != null)
                    return new ValueActivator(def).FromValue;
                return Activator;
            }
        }

        internal StorageProperty(
            ApplicationDataLocality locality,
            string path,
            T def,
            Func<T> defaultValueCreater,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer,
            ISerializer<T> serializer)
        {
            getContainer(locality, path, out this.container, out this.name);
            try
            {
                this.Serializer = serializer ?? Serializer<T>.Default;
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException($"Failed to generate default serializer by Serializer<{typeof(T)}>.Default, must specify by parameter.", ex);
            }
            this.EqualityComparer = equalityComparer ?? EqualityComparer<T>.Default;
            this.defaultValueCreater = defaultValueCreater ?? ActivatorFactory.GetActivator(def);
            this.propertyChangedCallback = callback;
            if (this.Locality == ApplicationDataLocality.Roaming)
                RoamingStoragePropertyManager.RoamingProperties.Add(new WeakReference<IStorageProperty>(this));
        }

        private readonly ApplicationDataContainer container;
        private readonly string name;

        /// <summary>
        /// Locality of <see cref="Value"/>'s storage.
        /// </summary>
        public ApplicationDataLocality Locality => this.container.Locality;

        /// <summary>
        /// Used to compare values to decide whether stored value should be updated or not.
        /// </summary>
        public IEqualityComparer<T> EqualityComparer { get; }
        /// <summary>
        /// Used to serialize and deserialize <see cref="Value"/>.
        /// </summary>
        public ISerializer<T> Serializer { get; }

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

        private readonly Func<T> defaultValueCreater;
        private bool valueCreated;

        private T value;
        /// <summary>
        /// Value of the <see cref="StorageProperty{T}"/>.
        /// </summary>
        public T Value
        {
            get
            {
                if (!this.valueCreated && !Populate())
                    Flush();
                return this.value;
            }
            set
            {
                if (this.valueCreated && EqualityComparer.Equals(this.value, value))
                    return;
                this.valueCreated = true;
                this.value = value;
                Flush();
                raiseChanged();
            }
        }
        object IStorageProperty.Value { get => Value; set => Value = (T)value; }
        Type IStorageProperty.ValueType => typeof(T);

        private void raiseChanged()
        {
            OnPropertyChanged(nameof(Value));
            this.propertyChangedCallback?.Invoke(this);
        }

        /// <summary>
        /// Load from storage.
        /// </summary>
        /// <returns>Whether data readed successfully.</returns>
        public bool Populate()
        {
            lock (this.defaultValueCreater)
            {
                var cv = false;
                if (!this.valueCreated)
                {
                    this.value = this.defaultValueCreater();
                    this.valueCreated = true;
                    cv = true;
                }
                try
                {
                    var storage = Data;
                    if (storage == null)
                    {
                        if (cv)
                            raiseChanged();
                        return false;
                    }
                    using (var reader = DataReader.FromBuffer(storage.AsBuffer()))
                        this.Serializer.Deserialize(reader, ref this.value);
                }
                catch
                {
                    if (cv)
                        raiseChanged();
                    return false;
                }
            }
            raiseChanged();
            return true;
        }

        /// <summary>
        /// Write to storage.
        /// </summary>
        public void Flush()
        {
            if (!this.valueCreated)
                Populate();
            var ser = this.Serializer;
            var data = default(byte[]);
            using (var dw = new DataWriter())
            {
                ser.Serialize(in this.value, dw);
                var b = dw.DetachBuffer();
                data = b.ToArray(0, (int)b.Length);
            }
            lock (this.defaultValueCreater)
            {
                Data = data;
                this.valueCreated = true;
            }
        }

        /// <summary>
        /// Clear storage.
        /// </summary>
        public void Delete()
        {
            lock (this.defaultValueCreater)
            {
                this.Data = null;
                this.value = default;
                this.valueCreated = false;
            }
            raiseChanged();
        }

        private readonly StoragePropertyChangedCallback<T> propertyChangedCallback;
    }

    /// <summary>
    /// Callback when value of <see cref="StorageProperty{T}"/> changed.
    /// </summary>
    /// <typeparam name="T">Type of value.</typeparam>
    /// <param name="sender">Changed <see cref="StorageProperty{T}"/>.</param>
    public delegate void StoragePropertyChangedCallback<T>(StorageProperty<T> sender);
}