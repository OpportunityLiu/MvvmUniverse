using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace Opportunity.MvvmUniverse.Settings
{
    public class SettingCollection : ObservableObject
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
            RaisePropertyChanged((string)null);
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
                    return deserializeValue<T>(v);
                }
            }
            catch { }
            return property.DefaultValue;
        }

        protected bool SetToContainer<T>(SettingProperty<T> property, T value)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            var old = GetFromContainer(property);
            if (this.Container.Values.ContainsKey(property.Name) && EqualityComparer<T>.Default.Equals(old, value))
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

        private void setToContainerCore<T>(SettingProperty<T> property, T old, T value)
        {
            this.Container.Values[property.Name] = serializeValue(value);
            RaisePropertyChanged(property.Name);
            property.RaisePropertyChanged(this, old, value);
        }

        private static object serializeValue(object value)
        {
            if (value is Enum)
                return value.ToString();
            else
                return value;
        }

        private static T deserializeValue<T>(object value)
        {
            if (default(T) is Enum)
                return (T)Enum.Parse(typeof(T), value.ToString());
            return (T)value;
        }
    }
}
