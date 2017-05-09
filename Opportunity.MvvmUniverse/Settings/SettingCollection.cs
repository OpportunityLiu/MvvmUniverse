using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace Opportunity.MvvmUniverse.Settings
{
    public class SettingCollection : ObservableDictionary<string, object>
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
                    target.Sync(true);
                }
            }
        }

        private static readonly List<WeakReference<SettingCollection>> roamingCollcetions
            = new List<WeakReference<SettingCollection>>();

        protected void Sync(bool containerToCollection)
        {
            if (containerToCollection)
            {
                sync(this.Container.Values, this);
            }
            else
            {
                sync(this, this.Container.Values);
            }
        }

        private static void sync(IDictionary<string, object> from, IDictionary<string, object> to)
        {
            var removeKeys = new List<string>();
            foreach (var item in to.Keys)
            {
                if (!from.ContainsKey(item))
                    removeKeys.Add(item);
            }
            foreach (var item in removeKeys)
            {
                to.Remove(item);
            }
            foreach (var item in from)
            {
                to[item.Key] = item.Value;
            }
        }

        public SettingCollection(ApplicationDataContainer container)
            : base((container ?? throw new ArgumentNullException(nameof(container))).Values)
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
                if (TryGetValue(property.Name, out var v))
                {
                    if (property.DefaultValue is Enum)
                        return (T)Enum.Parse(typeof(T), v.ToString());
                    return (T)v;
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
            if (ContainsKey(property.Name) && EqualityComparer<T>.Default.Equals(old, value))
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
            if (old is Enum)
                this[property.Name] = value.ToString();
            else
                this[property.Name] = value;
            RaisePropertyChanged(property.Name);
            property.RaisePropertyChanged(this, old, value);
        }

        protected override void InsertItem(string key, object value, int index)
        {
            base.InsertItem(key, value, index);
            Container.Values[key] = value;
        }

        protected override void SetItem(string key, object value)
        {
            base.SetItem(key, value);
            Container.Values[key] = value;
        }

        protected override void RemoveItem(string key)
        {
            base.RemoveItem(key);
            Container.Values.Remove(key);
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            Container.Values.Clear();
        }
    }
}
