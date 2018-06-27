using Opportunity.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Opportunity.MvvmUniverse.Storage
{
    /// <summary>
    /// <see cref="ObservableObject"/> with <see cref="ApplicationData"/> as backend storage.
    /// </summary>
    public abstract class StorageObject : ObservableObject
    {
        /// <summary>
        /// Create new instance of <see cref="StorageObject"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Instance of same container has been created.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="containerPath"/> is not valid.</exception>
        protected StorageObject(string containerPath)
        {
            this.StorageData = new StorageObjectData(this, containerPath);
            try
            {
                RegisterDictionary.Add(containerPath, this.StorageData);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Instance of same container '{containerPath}' has been created.", ex);
            }
        }

        internal readonly StorageObjectData StorageData;

        private static readonly Dictionary<string, StorageObjectData> RegisterDictionary
            = new Dictionary<string, StorageObjectData>();

        internal sealed class StorageObjectData
        {
            static StorageObjectData()
            {
                ApplicationData.Current.DataChanged += applicationDataChanged;
            }

            private static void applicationDataChanged(ApplicationData sender, object args)
            {
                foreach (var item in RegisterDictionary.Values)
                {
                    foreach (var prop in item.Data)
                    {
                        if (prop.Value.Locality == ApplicationDataLocality.Roaming)
                        {
                            prop.Value.State = StorageState.MemoryDirty;
                            item.Instance.OnPropertyChanged(prop.Key);
                        }
                    }
                }
            }

            static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
            static readonly ApplicationDataContainer RoamingSettings = ApplicationData.Current.RoamingSettings;
            internal static readonly char[] PathSep = "\\/".ToCharArray();

            public readonly ApplicationDataContainer LocalContainer;
            public readonly ApplicationDataContainer RoamingContainer;

            public readonly StorageObject Instance;

            public readonly Dictionary<string, IStoragePropertyData> Data = new Dictionary<string, IStoragePropertyData>();

            public StorageObjectData(StorageObject instance, string containerPath)
            {
                if (string.IsNullOrEmpty(containerPath))
                    throw new ArgumentNullException(nameof(containerPath));
                var sec = containerPath.Split(PathSep, StringSplitOptions.RemoveEmptyEntries);
                if (sec.Length == 0)
                    throw new ArgumentException("Not a valid path", nameof(containerPath));
                this.Instance = instance;
                this.LocalContainer = LocalSettings;
                this.RoamingContainer = RoamingSettings;
                for (var i = 0; i < sec.Length; i++)
                {
                    this.LocalContainer = LocalSettings.CreateContainer(sec[i], ApplicationDataCreateDisposition.Always);
                    this.RoamingContainer = RoamingSettings.CreateContainer(sec[i], ApplicationDataCreateDisposition.Always);
                }
            }

            public ApplicationDataContainer GetContainer(ApplicationDataLocality locality)
            {
                if (locality == ApplicationDataLocality.Roaming)
                    return this.RoamingContainer;
                else
                    return this.LocalContainer;
            }

            public static byte[] GetData(ApplicationDataContainer container, string name)
            {
                container.Values.TryGetValue(name, out var storage);
                if (!(storage is byte[] s) || s.Length == 0)
                {
                    return null;
                }
                return s;
            }

            public static void SetData(ApplicationDataContainer container, string name, byte[] value)
            {
                if (value == null || value.Length == 0)
                    container.Values.Remove(name);
                else
                    container.Values[name] = value;
            }

            public PropertyInfo GetPropertyInfo(string name)
                => this.Instance.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            public T CreateDefault<T>(string name)
            {
                var prop = GetPropertyInfo(name);
                var defa = prop.GetCustomAttribute<DefaultValueAttribute>();
                if (defa != null)
                    return (T)Convert.ChangeType(defa.Value, typeof(T));
                return default;
            }

            /// <summary>
            /// <see cref="StorageState.MemoryDirty"/> -> <see cref="StorageState.Synced"/>;
            /// </summary>
            public void Populate<T>(string name, StoragePropertyData<T> property)
            {
                var container = GetContainer(property.Locality);
                lock (property)
                {
                    var storage = GetData(container, name);
                    if (storage is null)
                    {
                        property.Value = CreateDefault<T>(name);
                        return;
                    }
                    using (var reader = DataReader.FromBuffer(storage.AsBuffer()))
                        property.Serializer.Deserialize(reader, ref property.Value);
                }
            }

            /// <summary>
            /// <see cref="StorageState.StorageDirty"/> -> <see cref="StorageState.Synced"/>;
            /// </summary>
            public void Flush<T>(string name, StoragePropertyData<T> property)
            {
                var data = default(byte[]);
                using (var dw = new DataWriter())
                {
                    property.Serializer.Serialize(in property.Value, dw);
                    var b = dw.DetachBuffer();
                    data = b.ToArray(0, (int)b.Length);
                }
                var container = GetContainer(property.Locality);
                lock (property)
                {
                    SetData(container, name, data);
                    property.State = StorageState.Synced;
                }
            }

            public StoragePropertyData<T> GetProperty<T>(string name)
            {
                if (this.Data.TryGetValue(name, out var r))
                    return (StoragePropertyData<T>)r;
                var prop = GetPropertyInfo(name);
                var def = prop.GetCustomAttribute<ApplicationSettingAttribute>();
                if (def is null)
                    throw new InvalidOperationException($"ApplicationSetting attribute not found on property '{name}'");

                var p = new StoragePropertyData<T>(def.GetSerializer<T>(), def.Locality);
                this.Data[name] = p;
                return p;
            }

            public T GetValue<T>(string name)
            {
                var property = GetProperty<T>(name);
                if (property.State == StorageState.MemoryDirty)
                    Populate(name, property);
                return property.Value;
            }

            public bool SetValue<T>(string name, T value)
            {
                var property = GetProperty<T>(name);
                if (EqualityComparer<T>.Default.Equals(value, property.Value) && property.State != StorageState.MemoryDirty)
                    return false;

                property.Value = value;
                property.State = StorageState.StorageDirty;
                Flush(name, property);
                return true;
            }

            public enum StorageState
            {
                /// <summary>
                /// Mem - oldvalue;
                /// Sto - value
                /// </summary>
                MemoryDirty = 0,
                /// <summary>
                /// Mem - value;
                /// Sto - oldvalue
                /// </summary>
                StorageDirty = 1,
                /// <summary>
                /// Mem - value;
                /// Sto - value
                /// </summary>
                Synced = 2,
            }

            /// <summary>
            /// Represents a value with backing field held by <see cref="ApplicationData"/>.
            /// </summary>
            /// <typeparam name="T">Type of value.</typeparam>
            [DebuggerDisplay(@"\{{Value}\}")]
            public sealed class StoragePropertyData<T> : IStoragePropertyData
            {
                internal StoragePropertyData(ISerializer<T> serializer, ApplicationDataLocality locality)
                {
                    this.Serializer = serializer ?? Serializer<T>.Default;
                    this.Locality = locality;
                }

                /// <summary>
                /// Used to serialize and deserialize <see cref="Value"/>.
                /// </summary>
                public readonly ISerializer<T> Serializer;
                /// <summary>
                /// State of storage.
                /// </summary>
                public StorageState State;

                public readonly ApplicationDataLocality Locality;

                public T Value;

                StorageState IStoragePropertyData.State { get => this.State; set => this.State = value; }

                ApplicationDataLocality IStoragePropertyData.Locality => this.Locality;
            }

            public interface IStoragePropertyData
            {
                StorageState State { get; set; }
                ApplicationDataLocality Locality { get; }
            }
        }

        /// <summary>
        /// Set value to storage.
        /// </summary>
        /// <typeparam name="TProp">Type of property</typeparam>
        /// <param name="value">New value</param>
        /// <param name="propertyName">Name of property.</param>
        /// <returns>Whether <paramref name="value"/> has changed or not.</returns>
        protected bool SetStorage<TProp>(TProp value, [CallerMemberName]string propertyName = null)
        {
            if (this.StorageData.SetValue(propertyName, value))
            {
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get value from storage.
        /// </summary>
        /// <typeparam name="TProp">Type of property</typeparam>
        /// <param name="propertyName">Name of property.</param>
        /// <returns>Value of property.</returns>
        protected TProp GetStorage<TProp>([CallerMemberName]string propertyName = null)
        {
            return this.StorageData.GetValue<TProp>(propertyName);
        }
    }
}
