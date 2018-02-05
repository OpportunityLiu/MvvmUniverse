using System;

namespace Opportunity.MvvmUniverse.Storage
{
    public delegate void StoragePropertyChangedCallback<T>(StorageProperty<T> sender, StoragePropertyChangedEventArgs<T> e);

    public sealed class StoragePropertyChangedEventArgs<T> : EventArgs
    {
        internal StoragePropertyChangedEventArgs(T oldValue, T newValue)
        {
            this.NewValue = newValue;
            this.OldValue = oldValue;
        }

        public T NewValue { get; }
        public T OldValue { get; }
    }
}
