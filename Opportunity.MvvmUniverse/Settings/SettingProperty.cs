using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace Opportunity.MvvmUniverse.Settings
{
    public abstract class SettingProperty
    {
        internal SettingProperty(string name, Type propertyType)
        {
            this.Name = name;
            this.PropertyType = propertyType;
        }

        public static SettingProperty<T> Create<T>(string propertyName)
            => Create(propertyName, default(T), null);
        public static SettingProperty<T> Create<T>(string propertyName, T defaultValue)
            => Create(propertyName, defaultValue, null);
        public static SettingProperty<T> Create<T>(string propertyName, T defaultValue, SettingPropertyChangedCallback<T> callback)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException(nameof(propertyName));
            return new SettingProperty<T>(propertyName, defaultValue, callback);
        }

        public string Name { get; }
        public Type PropertyType { get; }

        internal abstract object GetDefault();
        internal abstract object GetTypeDefault();
        internal abstract bool TestValue(object value);
        internal abstract void RaisePropertyChanged(SettingCollection sender, object oldValue, object newValue);
        internal abstract bool Equals(object value1, object value2);
    }

    public sealed class SettingProperty<T> : SettingProperty
    {
        private static readonly EqualityComparer<T> equalityComparer = getComparer();
        private static EqualityComparer<T> getComparer()
        {
            return EqualityComparer<T>.Default;
        }

        internal SettingProperty(string name, T def, SettingPropertyChangedCallback<T> callback)
            : base(name, typeof(T))
        {
            this.DefaultValue = def;
            this.PropertyChangedCallback = callback;
        }

        public T DefaultValue { get; }

        public SettingPropertyChangedCallback<T> PropertyChangedCallback { get; }

        internal void RaisePropertyChanged(SettingCollection sender, T oldValue, T newValue)
        {
            var cb = PropertyChangedCallback;
            if (cb == null)
                return;
            var arg = new SettingPropertyChangedEventArgs<T>(this, oldValue, newValue);
            DispatcherHelper.BeginInvoke(() => cb(sender, arg));
        }

        internal override bool Equals(object value1, object value2)
            => equalityComparer.Equals((T)value1, (T)value2);
        internal override object GetDefault() => DefaultValue;
        internal override object GetTypeDefault() => default(T);
        internal override void RaisePropertyChanged(SettingCollection sender, object oldValue, object newValue)
            => RaisePropertyChanged(sender, (T)oldValue, (T)newValue);
        internal override bool TestValue(object value)
        {
            if (value is T)
                return true;
            return (value == null && default(T) == null);
        }
    }
}
