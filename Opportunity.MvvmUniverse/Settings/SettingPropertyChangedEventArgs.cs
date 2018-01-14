using System;

namespace Opportunity.MvvmUniverse.Settings
{
    public delegate void SettingPropertyChangedCallback<T>(SettingCollection sender, SettingPropertyChangedEventArgs<T> e);

    public sealed class SettingPropertyChangedEventArgs<T> : EventArgs
    {
        internal SettingPropertyChangedEventArgs(SettingProperty<T> prop, T oldValue, T newValue)
        {
            this.NewValue = newValue;
            this.OldValue = oldValue;
            this.Property = prop;
        }

        public T NewValue { get; }
        public T OldValue { get; }
        public SettingProperty<T> Property { get; }
    }
}
