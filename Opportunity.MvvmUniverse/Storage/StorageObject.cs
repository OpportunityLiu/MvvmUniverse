using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace Opportunity.MvvmUniverse.Storage
{
    /// <summary>
    /// 
    /// </summary>
    [DebuggerTypeProxy(typeof(DebugProxy))]
    public abstract partial class StorageObject : ObservableObject
    {
        #region Static Factroy Methods

        public static LocalStorageObject CreateLocal(ApplicationDataContainer container)
            => new LocalStorageObject(container);
        public static LocalStorageObject CreateLocal(string containerName)
            => new LocalStorageObject(ApplicationData.Current.LocalSettings, containerName);
        public static LocalStorageObject CreateLocal(ApplicationDataContainer parent, string containerName)
            => new LocalStorageObject(parent, containerName);
        public static LocalStorageObject CreateLocal(LocalStorageObject parent, string containerName)
            => new LocalStorageObject(parent, containerName);

        public static RoamingStorageObject CreateRoaming(ApplicationDataContainer container)
            => new RoamingStorageObject(container);
        public static RoamingStorageObject CreateRoaming(string containerName)
            => new RoamingStorageObject(ApplicationData.Current.RoamingSettings, containerName);
        public static RoamingStorageObject CreateRoaming(ApplicationDataContainer parent, string containerName)
            => new RoamingStorageObject(parent, containerName);
        public static RoamingStorageObject CreateRoaming(RoamingStorageObject parent, string containerName)
            => new RoamingStorageObject(parent, containerName);

        #endregion Static Factroy Methods

        #region Constructors

        internal StorageObject(ApplicationDataContainer container)
        {
            this.Container = container;
        }

        private static ApplicationDataContainer create(ApplicationDataContainer parent, string containerName)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentNullException(nameof(containerName));
            return parent.CreateContainer(containerName, ApplicationDataCreateDisposition.Always);
        }

        internal StorageObject(ApplicationDataContainer parent, string containerName)
            : this(create(parent, containerName)) { }

        private static ApplicationDataContainer create(StorageObject parent, string containerName)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (parent.Container == null)
                throw new ArgumentException("Container of given SettingCollection is null", nameof(parent));
            return create(parent.Container, containerName);
        }

        internal StorageObject(StorageObject parent, string containerName)
            : this(create(parent, containerName)) { }

        #endregion Constructors

        protected ApplicationDataContainer Container { get; }

        protected T GetFromContainer<T>(StorageProperty<T> property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            try
            {
                if (this.Container.Values.TryGetValue(property.Name, out var v))
                {
                    return property.FromStorage(v);
                }
            }
            catch { }
            return property.DefaultValue;
        }

        protected object GetFromContainer(StorageProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            var p = (IStorageProperty)property;
            try
            {
                if (this.Container.Values.TryGetValue(p.Name, out var v))
                {
                    return p.FromStorage(v);
                }
            }
            catch { }
            return p.DefaultValue;
        }

        protected bool SetToContainer<T>(StorageProperty<T> property, T value)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            var old = GetFromContainer(property);
            if (this.Container.Values.ContainsKey(property.Name) && property.Equals(old, value))
            {
                return false;
            }
            setToContainerCore(property, old, value);
            return true;
        }

        protected bool SetToContainer(StorageProperty property, object value)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            var p = (IStorageProperty)property;
            if (!p.IsValueValid(value))
                throw new ArgumentException("Type of value doesn't match property.PropertyType.");
            var old = GetFromContainer(property);
            if (this.Container.Values.ContainsKey(p.Name) && p.Equals(old, value))
            {
                return false;
            }
            setToContainerCore(property, old, value);
            return true;
        }

        protected void ForceSetToContainer<T>(StorageProperty<T> property, T value)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            var old = GetFromContainer(property);
            setToContainerCore(property, old, value);
        }

        protected void ForceSetToContainer(StorageProperty property, object value)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            var p = (IStorageProperty)property;
            if (!p.IsValueValid(value))
                throw new ArgumentException("Type of value doesn't match property.PropertyType.");
            var old = GetFromContainer(property);
            setToContainerCore(property, old, value);
        }

        private void setToContainerCore<T>(StorageProperty<T> property, T old, T value)
        {
            this.Container.Values[property.Name] = property.ToStorage(value);
            OnPropertyChanged(property.Name);
            property.RaisePropertyChanged(this, old, value);
        }

        private void setToContainerCore(StorageProperty property, object old, object value)
        {
            var p = (IStorageProperty)property;
            this.Container.Values[p.Name] = p.ToStorage(value);
            OnPropertyChanged(p.Name);
            p.RaisePropertyChanged(this, old, value);
        }
    }
}
