using Opportunity.MvvmUniverse.Collections;
using System;

namespace Opportunity.MvvmUniverse.Views
{
    public static class ViewModelFactory
    {
        private static class TypedStorage<T>
            where T : ViewModelBase
        {
            public static AutoFillCacheStorage<string, T> CacheStorage { get; set; }
        }

        public static AutoFillCacheStorage<string, T> Storage<T>()
            where T : ViewModelBase
        {
            var s = TypedStorage<T>.CacheStorage;
            if (s is null)
                throw new InvalidOperationException("Did not registed.");
            return s;
        }

        public static T Get<T>(string parameter)
            where T : ViewModelBase
        {
            return Storage<T>().GetOrCreateAsync(parameter).GetResults();
        }

        public static T Set<T>(string parameter, T viewModel)
            where T : ViewModelBase
        {
            return Storage<T>()[parameter] = viewModel;
        }

        public static void Register<T>(Func<string, T> activator, int capacity)
            where T : ViewModelBase
            => Register(activator, capacity, false);

        public static void Register<T>(Func<string, T> activator, int capacity, bool throwIfRegistered)
            where T : ViewModelBase
        {
            if (activator is null)
                throw new ArgumentNullException(nameof(activator));
            if (TypedStorage<T>.CacheStorage != null)
            {
                if (throwIfRegistered)
                    throw new InvalidOperationException("Have registed.");
                return;
            }
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