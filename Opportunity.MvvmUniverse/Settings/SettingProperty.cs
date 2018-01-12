using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Opportunity.MvvmUniverse.Settings
{
    public sealed class SettingProperty<T>
    {
        public SettingProperty(string name, Type owner)
            : this(name, owner, default(T), null)
        {
        }

        public SettingProperty(string name, Type owner, T def)
            : this(name, owner, def, null)
        {
        }

        public SettingProperty(string name, Type owner, SettingPropertyChangedCallback<T> callback)
            : this(name, owner, default(T), callback)
        {
        }

        public SettingProperty(string name, Type owner, T def, SettingPropertyChangedCallback<T> callback)
        {
            this.Name = name;
            this.OwnerType = owner;
            this.PropertyType = typeof(T);
            this.DefaultValue = def;
            this.PropertyChangedCallback = callback;
        }

        public string Name { get; }
        public Type OwnerType { get; }
        public Type PropertyType { get; }
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
    }
}
