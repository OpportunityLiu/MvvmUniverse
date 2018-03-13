using System;
using System.Collections.Generic;
using Windows.Storage;

namespace Opportunity.MvvmUniverse.Storage
{
    /// <summary>
    /// Factory methods for <see cref="StorageProperty{T}"/>.
    /// </summary>
    public static class StorageProperty
    {
        /// <summary>
        /// Create local <see cref="StorageProperty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of stored value.</typeparam>
        /// <param name="path">Path of storage.</param>
        /// <param name="equalityComparer"><see cref="IEqualityComparer{T}"/> for values.</param>
        /// <param name="serializer"><see cref="ISerializer{T}"/> for values.</param>
        /// <returns>Created <see cref="StorageProperty{T}"/>.</returns>
        public static StorageProperty<T> CreateLocal<T>(
            string path,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, default, null, null, equalityComparer, serializer);

        /// <summary>
        /// Create local <see cref="StorageProperty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of stored value.</typeparam>
        /// <param name="path">Path of storage.</param>
        /// <param name="callback">Callback for property changed notification.</param>
        /// <param name="equalityComparer"><see cref="IEqualityComparer{T}"/> for values.</param>
        /// <param name="serializer"><see cref="ISerializer{T}"/> for values.</param>
        /// <returns>Created <see cref="StorageProperty{T}"/>.</returns>
        public static StorageProperty<T> CreateLocal<T>(
            string path,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, default, null, callback, equalityComparer, serializer);

        /// <summary>
        /// Create local <see cref="StorageProperty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of stored value.</typeparam>
        /// <param name="path">Path of storage.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <param name="equalityComparer"><see cref="IEqualityComparer{T}"/> for values.</param>
        /// <param name="serializer"><see cref="ISerializer{T}"/> for values.</param>
        /// <returns>Created <see cref="StorageProperty{T}"/>.</returns>
        public static StorageProperty<T> CreateLocal<T>(
            string path,
            T defaultValue,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, defaultValue, null, null, equalityComparer, serializer);

        /// <summary>
        /// Create local <see cref="StorageProperty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of stored value.</typeparam>
        /// <param name="path">Path of storage.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <param name="callback">Callback for property changed notification.</param>
        /// <param name="equalityComparer"><see cref="IEqualityComparer{T}"/> for values.</param>
        /// <param name="serializer"><see cref="ISerializer{T}"/> for values.</param>
        /// <returns>Created <see cref="StorageProperty{T}"/>.</returns>
        public static StorageProperty<T> CreateLocal<T>(
            string path,
            T defaultValue,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, defaultValue, null, callback, equalityComparer, serializer);

        /// <summary>
        /// Create local <see cref="StorageProperty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of stored value.</typeparam>
        /// <param name="path">Path of storage.</param>
        /// <param name="defaultValueCreator">Delegate to create default value.</param>
        /// <param name="equalityComparer"><see cref="IEqualityComparer{T}"/> for values.</param>
        /// <param name="serializer"><see cref="ISerializer{T}"/> for values.</param>
        /// <returns>Created <see cref="StorageProperty{T}"/>.</returns>
        public static StorageProperty<T> CreateLocal<T>(
            string path,
            Func<T> defaultValueCreator,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, default, defaultValueCreator ?? throw new ArgumentNullException(nameof(defaultValueCreator)), null, equalityComparer, serializer);

        /// <summary>
        /// Create local <see cref="StorageProperty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of stored value.</typeparam>
        /// <param name="path">Path of storage.</param>
        /// <param name="defaultValueCreator">Delegate to create default value.</param>
        /// <param name="callback">Callback for property changed notification.</param>
        /// <param name="equalityComparer"><see cref="IEqualityComparer{T}"/> for values.</param>
        /// <param name="serializer"><see cref="ISerializer{T}"/> for values.</param>
        /// <returns>Created <see cref="StorageProperty{T}"/>.</returns>
        public static StorageProperty<T> CreateLocal<T>(
            string path,
            Func<T> defaultValueCreator,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Local, path, default, defaultValueCreator ?? throw new ArgumentNullException(nameof(defaultValueCreator)), callback, equalityComparer, serializer);

        /// <summary>
        /// Create roaming <see cref="StorageProperty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of stored value.</typeparam>
        /// <param name="path">Path of storage.</param>
        /// <param name="equalityComparer"><see cref="IEqualityComparer{T}"/> for values.</param>
        /// <param name="serializer"><see cref="ISerializer{T}"/> for values.</param>
        /// <returns>Created <see cref="StorageProperty{T}"/>.</returns>
        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, default, null, null, equalityComparer, serializer);

        /// <summary>
        /// Create roaming <see cref="StorageProperty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of stored value.</typeparam>
        /// <param name="path">Path of storage.</param>
        /// <param name="callback">Callback for property changed notification.</param>
        /// <param name="equalityComparer"><see cref="IEqualityComparer{T}"/> for values.</param>
        /// <param name="serializer"><see cref="ISerializer{T}"/> for values.</param>
        /// <returns>Created <see cref="StorageProperty{T}"/>.</returns>
        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, default, null, callback, equalityComparer, serializer);

        /// <summary>
        /// Create roaming <see cref="StorageProperty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of stored value.</typeparam>
        /// <param name="path">Path of storage.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <param name="equalityComparer"><see cref="IEqualityComparer{T}"/> for values.</param>
        /// <param name="serializer"><see cref="ISerializer{T}"/> for values.</param>
        /// <returns>Created <see cref="StorageProperty{T}"/>.</returns>
        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            T defaultValue,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, defaultValue, null, null, equalityComparer, serializer);

        /// <summary>
        /// Create roaming <see cref="StorageProperty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of stored value.</typeparam>
        /// <param name="path">Path of storage.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <param name="callback">Callback for property changed notification.</param>
        /// <param name="equalityComparer"><see cref="IEqualityComparer{T}"/> for values.</param>
        /// <param name="serializer"><see cref="ISerializer{T}"/> for values.</param>
        /// <returns>Created <see cref="StorageProperty{T}"/>.</returns>
        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            T defaultValue,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, defaultValue, null, callback, equalityComparer, serializer);

        /// <summary>
        /// Create roaming <see cref="StorageProperty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of stored value.</typeparam>
        /// <param name="path">Path of storage.</param>
        /// <param name="defaultValueCreator">Delegate to create default value.</param>
        /// <param name="equalityComparer"><see cref="IEqualityComparer{T}"/> for values.</param>
        /// <param name="serializer"><see cref="ISerializer{T}"/> for values.</param>
        /// <returns>Created <see cref="StorageProperty{T}"/>.</returns>
        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            Func<T> defaultValueCreator,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, default, defaultValueCreator ?? throw new ArgumentNullException(nameof(defaultValueCreator)), null, equalityComparer, serializer);

        /// <summary>
        /// Create roaming <see cref="StorageProperty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of stored value.</typeparam>
        /// <param name="path">Path of storage.</param>
        /// <param name="defaultValueCreator">Delegate to create default value.</param>
        /// <param name="callback">Callback for property changed notification.</param>
        /// <param name="equalityComparer"><see cref="IEqualityComparer{T}"/> for values.</param>
        /// <param name="serializer"><see cref="ISerializer{T}"/> for values.</param>
        /// <returns>Created <see cref="StorageProperty{T}"/>.</returns>
        public static StorageProperty<T> CreateRoaming<T>(
            string path,
            Func<T> defaultValueCreator,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(ApplicationDataLocality.Roaming, path, default, defaultValueCreator ?? throw new ArgumentNullException(nameof(defaultValueCreator)), callback, equalityComparer, serializer);

        /// <summary>
        /// Create <see cref="StorageProperty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of stored value.</typeparam>
        /// <param name="locality">Locality of storage.</param>
        /// <param name="path">Path of storage.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <param name="callback">Callback for property changed notification.</param>
        /// <param name="equalityComparer"><see cref="IEqualityComparer{T}"/> for values.</param>
        /// <param name="serializer"><see cref="ISerializer{T}"/> for values.</param>
        /// <returns>Created <see cref="StorageProperty{T}"/>.</returns>
        public static StorageProperty<T> Create<T>(
            ApplicationDataLocality locality, string path,
            T defaultValue,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(locality, path, defaultValue, null, callback, equalityComparer, serializer);

        /// <summary>
        /// Create <see cref="StorageProperty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of stored value.</typeparam>
        /// <param name="locality">Locality of storage.</param>
        /// <param name="path">Path of storage.</param>
        /// <param name="defaultValueCreator">Delegate to create default value.</param>
        /// <param name="callback">Callback for property changed notification.</param>
        /// <param name="equalityComparer"><see cref="IEqualityComparer{T}"/> for values.</param>
        /// <param name="serializer"><see cref="ISerializer{T}"/> for values.</param>
        /// <returns>Created <see cref="StorageProperty{T}"/>.</returns>
        public static StorageProperty<T> Create<T>(
            ApplicationDataLocality locality, string path,
            Func<T> defaultValueCreator,
            StoragePropertyChangedCallback<T> callback,
            IEqualityComparer<T> equalityComparer = null,
            ISerializer<T> serializer = null)
            => new StorageProperty<T>(locality, path, default, defaultValueCreator ?? throw new ArgumentNullException(nameof(defaultValueCreator)), callback, equalityComparer, serializer);
    }
}
