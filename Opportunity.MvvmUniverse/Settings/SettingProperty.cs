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
        internal SettingProperty() { }

        public static SettingProperty<T> Create<T>(string propertyName)
            => new SettingProperty<T>(propertyName);
        public static SettingProperty<T> Create<T>(string propertyName, T defaultValue)
            => new SettingProperty<T>(propertyName, defaultValue);
        public static SettingProperty<T> Create<T>(string propertyName, T defaultValue, SettingPropertyChangedCallback<T> callback) => new SettingProperty<T>(propertyName, defaultValue, callback);
    }

    internal interface ISettingProperty
    {
        string Name { get; }
        Type PropertyType { get; }
        object ToStorage(object value);
        object FromStorage(object value);
        object DefaultValue { get; }
        object GetTypeDefault();
        bool IsValueValid(object value);
        void RaisePropertyChanged(SettingCollection sender, object oldValue, object newValue);
        bool Equals(object value1, object value2);
    }

    public sealed class SettingProperty<T> : SettingProperty, ISettingProperty
    {
        public SettingProperty(string propertyName)
            : this(propertyName, default, null) { }

        internal SettingProperty(string propertyName, T def)
            : this(propertyName, def, null) { }

        internal SettingProperty(string propertyName, SettingPropertyChangedCallback<T> callback)
            : this(propertyName, default, callback) { }

        internal SettingProperty(string propertyName, T def, SettingPropertyChangedCallback<T> callback)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException(nameof(propertyName));
            this.Name = propertyName;
            this.DefaultValue = def;
            this.PropertyChangedCallback = callback;
        }

        public T DefaultValue { get; }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ISettingProperty.DefaultValue => DefaultValue;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IEqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
        public IEqualityComparer<T> EqualityComparer
        {
            get => this.equalityComparer;
            set => this.equalityComparer = value ?? EqualityComparer<T>.Default;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ISerializer<T> serializer = Serializer<T>.Default;
        public ISerializer<T> Serializer
        {
            get => this.serializer;
            set => this.serializer = value ?? Serializer<T>.Default;
        }

        public SettingPropertyChangedCallback<T> PropertyChangedCallback { get; }

        public string Name { get; }

        public Type PropertyType => typeof(T);
        object ISettingProperty.GetTypeDefault() => default(T);
        bool ISettingProperty.IsValueValid(object value)
        {
            if (value is T)
                return true;
            return (value == null && default(T) == null);
        }

        internal void RaisePropertyChanged(SettingCollection sender, T oldValue, T newValue)
        {
            var cb = PropertyChangedCallback;
            if (cb == null)
                return;
            var arg = new SettingPropertyChangedEventArgs<T>(this, oldValue, newValue);
            DispatcherHelper.BeginInvoke(() => cb(sender, arg));
        }
        void ISettingProperty.RaisePropertyChanged(SettingCollection sender, object oldValue, object newValue)
             => RaisePropertyChanged(sender, (T)oldValue, (T)newValue);

        bool ISettingProperty.Equals(object value1, object value2) => this.equalityComparer.Equals((T)value1, (T)value2);
        internal bool Equals(T value1, T value2) => this.equalityComparer.Equals(value1, value2);

        object ISettingProperty.ToStorage(object value) => this.serializer.Serialize((T)value);
        internal object ToStorage(T value) => this.serializer.Serialize(value);

        object ISettingProperty.FromStorage(object value) => this.serializer.Deserialize(value);
        internal T FromStorage(object value) => this.serializer.Deserialize(value);
    }
}
