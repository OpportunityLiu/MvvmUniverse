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

        protected T GetFromContainer<T>(T @default, [CallerMemberName]string key = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            try
            {
                if (TryGetValue(key, out var v))
                {
                    if (@default is Enum)
                        return (T)Enum.Parse(typeof(T), v.ToString());
                    return (T)v;
                }
            }
            catch { }
            return @default;
        }

        protected bool SetToContainer<T>(T value, [CallerMemberName]string key = null)
        {
            if (ContainsKey(key) && EqualityComparer<T>.Default.Equals(GetFromContainer(value, key), value))
                return false;
            ForceSetToContainer(value, key);
            return true;
        }

        protected void ForceSetToContainer<T>(T value, [CallerMemberName]string key = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (value is Enum)
                this[key] = value.ToString();
            else
                this[key] = value;
            RaisePropertyChanged(key);
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
