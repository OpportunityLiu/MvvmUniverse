using System;
using System.Collections.Generic;
using Windows.Storage;

namespace Opportunity.MvvmUniverse.Storage
{
    public static class StorageProperty
    {
        public static StorageProperty<T> CreateLocal<T>(
            string path,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, default, null, null, equalityComparer, serializer);

        public static StorageProperty<T> CreateLocal<T>(
            string path,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, default, null, callback, equalityComparer, serializer);

        public static StorageProperty<T> CreateLocal<T>(
            string path,
            T defaultValue,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, defaultValue, null, null, equalityComparer, serializer);

        public static StorageProperty<T> CreateLocal<T>(
            string path,
            T defaultValue,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, defaultValue, null, callback, equalityComparer, serializer);

        public static StorageProperty<T> CreateLocal<T>(
            string path,
            Func<T> defaultValueCreator,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, default, defaultValueCreator ?? throw new ArgumentNullException(nameof(defaultValueCreator)), null, equalityComparer, serializer);

        public static StorageProperty<T> CreateLocal<T>(
            string path,
            Func<T> defaultValueCreator,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, default, defaultValueCreator ?? throw new ArgumentNullException(nameof(defaultValueCreator)), callback, equalityComparer, serializer);

        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, default, null, null, equalityComparer, serializer);

        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, default, null, callback, equalityComparer, serializer);

        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            T defaultValue,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, defaultValue, null, null, equalityComparer, serializer);

        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            T defaultValue,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, defaultValue, null, callback, equalityComparer, serializer);

        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            Func<T> defaultValueCreator,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, default, defaultValueCreator ?? throw new ArgumentNullException(nameof(defaultValueCreator)), null, equalityComparer, serializer);

        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            Func<T> defaultValueCreator,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, default, defaultValueCreator ?? throw new ArgumentNullException(nameof(defaultValueCreator)), callback, equalityComparer, serializer);

        public static StorageProperty<T> Create<T>(
            ApplicationDataLocality locality, string path,
            T defaultValue,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(locality, path, defaultValue, null, callback, equalityComparer, serializer);

        public static StorageProperty<T> Create<T>(
            ApplicationDataLocality locality, string path,
            Func<T> defaultValueCreator,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(locality, path, default, defaultValueCreator ?? throw new ArgumentNullException(nameof(defaultValueCreator)), callback, equalityComparer, serializer);
    }
}
