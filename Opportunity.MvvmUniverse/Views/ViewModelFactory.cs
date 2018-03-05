using Opportunity.MvvmUniverse.Collections;
using System;

namespace Opportunity.MvvmUniverse.Views
{
    public static class ViewModelFactory
    {
        private static class TypedStorage<T>
            where T : ViewModelBase
        {
            public static AutoFillCacheStorage<string, T> CacheStorage;
        }

        public static AutoFillCacheStorage<string, T> Storage<T>()
            where T : ViewModelBase
            => TypedStorage<T>.CacheStorage;

        public static void Register<T>(Func<string, T> activator, int capacity)
            where T : ViewModelBase
        {
            if (activator == null)
                throw new ArgumentNullException(nameof(activator));
            if (Storage<T>() != null)
                throw new InvalidOperationException("Have registed.");
            TypedStorage<T>.CacheStorage = AutoFillCacheStorage.Create(activator, capacity);
        }

        public static void Register<T>(Func<string, T> activator)
            where T : ViewModelBase
            => Register(activator, 10);

        public static void Unregister<T>()
            where T : ViewModelBase
        {
            TypedStorage<T>.CacheStorage = null;
        }
    }
}