using System;

namespace Opportunity.MvvmUniverse.Storage
{
    public delegate void StoragePropertyChangedCallback<T>(StorageObject sender, StoragePropertyChangedEventArgs<T> e);

    public sealed class StoragePropertyChangedEventArgs<T> : EventArgs
    {
        internal StoragePropertyChangedEventArgs(StorageProperty<T> prop, T oldValue, T newValue)
        {
            this.NewValue = newValue;
            this.OldValue = oldValue;
            this.Property = prop;
        }

        public T NewValue { get; }
        public T OldValue { get; }
        public StorageProperty<T> Property { get; }
    }
}
