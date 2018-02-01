using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace Opportunity.MvvmUniverse.Settings
{
    [DebuggerTypeProxy(typeof(DebugProxy))]
    public partial class SettingCollection : ObservableObject
    {
        static SettingCollection()
        {
            ApplicationData.Current.DataChanged += applicationDataChanged;
        }

        private static void applicationDataChanged(ApplicationData sender, object args)
        {
            roamingCollcetions.RemoveAll(c => !c.TryGetTarget(out var ignore));
            foreach (var item in roamingCollcetions)
            {
                if (item.TryGetTarget(out var target))
                {
                    target.RoamingDataChanged();
                }
            }
        }

        protected virtual void RoamingDataChanged()
        {
            OnPropertyChanged((string)null);
        }

        private static readonly List<WeakReference<SettingCollection>> roamingCollcetions
            = new List<WeakReference<SettingCollection>>();

        public SettingCollection(ApplicationDataContainer container)
        {
            this.Container = container;
            if (container.Locality == ApplicationDataLocality.Roaming)
                roamingCollcetions.Add(new WeakReference<SettingCollection>(this));
        }

        private static ApplicationDataContainer create(ApplicationDataContainer parent, string containerName)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentNullException(nameof(containerName));
            return parent.CreateContainer(containerName, ApplicationDataCreateDisposition.Always);
        }

        public SettingCollection(ApplicationDataContainer parent, string containerName)
            : this(create(parent, containerName)) { }

        private static ApplicationDataContainer create(SettingCollection parent, string containerName)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (parent.Container == null)
                throw new ArgumentException("Container of given SettingCollection is null", nameof(parent));
            return create(parent.Container, containerName);
        }

        public SettingCollection(SettingCollection parent, string containerName)
            : this(create(parent, containerName)) { }

        protected ApplicationDataContainer Container { get; }

        protected T GetFromContainer<T>(SettingProperty<T> property)
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

        protected object GetFromContainer(SettingProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            var p = (ISettingProperty)property;
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

        protected bool SetToContainer<T>(SettingProperty<T> property, T value)
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

        protected bool SetToContainer(SettingProperty property, object value)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            var p = (ISettingProperty)property;
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

        protected void ForceSetToContainer<T>(SettingProperty<T> property, T value)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            var old = GetFromContainer(property);
            setToContainerCore(property, old, value);
        }

        protected void ForceSetToContainer(SettingProperty property, object value)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            var p = (ISettingProperty)property;
            if (!p.IsValueValid(value))
                throw new ArgumentException("Type of value doesn't match property.PropertyType.");
            var old = GetFromContainer(property);
            setToContainerCore(property, old, value);
        }

        private void setToContainerCore<T>(SettingProperty<T> property, T old, T value)
        {
            this.Container.Values[property.Name] = property.ToStorage(value);
            OnPropertyChanged(property.Name);
            property.RaisePropertyChanged(this, old, value);
        }

        private void setToContainerCore(SettingProperty property, object old, object value)
        {
            var p = (ISettingProperty)property;
            this.Container.Values[p.Name] = p.ToStorage(value);
            OnPropertyChanged(p.Name);
            p.RaisePropertyChanged(this, old, value);
        }
    }
}
