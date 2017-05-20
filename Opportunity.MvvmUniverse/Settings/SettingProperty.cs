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
            if (PropertyChangedCallback == null)
                return;
            DispatcherHelper.BeginInvoke(() => PropertyChangedCallback(sender, new SettingPropertyChangedEventArgs<T>(this, oldValue, newValue)));
        }
    }
}
