using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Opportunity.MvvmUniverse.Settings
{
    public abstract class SettingProperty
    {
        public static SettingProperty<T> Register<T>(string propertyName, Type ownerType, T defaultValue, SettingPropertyChangedCallback<T> propertyChangedCallback)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException(nameof(propertyName));
            if (ownerType == null)
                throw new ArgumentNullException(nameof(ownerType));
            if (ownerType.GetTypeInfo().IsSubclassOf(typeof(SettingCollection)))
                throw new ArgumentException("ownerType must be subclass of SettingCollection", nameof(ownerType));
            var key = new SettingPropertyKey(propertyName, ownerType);
            if (propDic.ContainsKey(key))
                throw new ArgumentException("Property with same name and owner class has been registed");
            var value = new SettingProperty<T>(propertyName, ownerType, defaultValue, propertyChangedCallback);
            lock (propDic)
            {
                if (propDic.ContainsKey(key))
                    throw new ArgumentException("Property with same name and owner class has been registed");
                propDic.Add(key, value);
            }
            return value;
        }

        private struct SettingPropertyKey : IEquatable<SettingPropertyKey>
        {
            public SettingPropertyKey(string name, Type owner)
            {
                this.Name = name;
                this.OwnerType = owner;
            }

            public string Name { get; }
            public Type OwnerType { get; }

            public bool Equals(SettingPropertyKey other)
            {
                return this.Name == other.Name && this.OwnerType == other.OwnerType;
            }

            public override bool Equals(object obj)
            {
                if (obj is SettingPropertyKey p)
                {
                    return this.Equals(p);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode() ^ OwnerType.GetHashCode();
            }
        }

        private static Dictionary<SettingPropertyKey, SettingProperty> propDic = new Dictionary<SettingPropertyKey, SettingProperty>();

        internal SettingProperty(string name, Type owner, Type pType)
        {
            this.Name = name;
            this.OwnerType = owner;
            this.PropertyType = pType;
        }

        public string Name { get; }
        public Type OwnerType { get; }
        public Type PropertyType { get; }
        public object DefaultValue { get; }
        protected abstract object GetDefaultValue();
    }

    public sealed class SettingProperty<T> : SettingProperty
    {
        internal SettingProperty(string name, Type owner, T def, SettingPropertyChangedCallback<T> callback)
            : base(name, owner, typeof(T))
        {
            this.DefaultValue = def;
            this.PropertyChangedCallback = callback;
        }

        public new T DefaultValue { get; }
        protected override object GetDefaultValue() => DefaultValue;

        public SettingPropertyChangedCallback<T> PropertyChangedCallback { get; }

        internal void RaisePropertyChanged(SettingCollection sender, T oldValue, T newValue)
        {
            if (PropertyChangedCallback == null)
                return;
            DispatcherHelper.BeginInvokeOnUIThread(() => PropertyChangedCallback(sender, new SettingPropertyChangedEventArgs<T>(this, oldValue, newValue)));
        }
    }

    public delegate void SettingPropertyChangedCallback<T>(SettingCollection sender, SettingPropertyChangedEventArgs<T> e);

    public sealed class SettingPropertyChangedEventArgs<T>
    {
        internal SettingPropertyChangedEventArgs(SettingProperty prop, T oldValue, T newValue)
        {
            this.NewValue = newValue;
            this.OldValue = oldValue;
            this.Property = prop;
        }

        public T NewValue { get; }
        public T OldValue { get; }
        public SettingProperty Property { get; }
    }
}
