using Opportunity.MvvmUniverse.Collections;
using System;

namespace Opportunity.MvvmUniverse.Views
{
    /// <summary>
    /// Factory class of <see cref="ViewModelBase"/>.
    /// </summary>
    public static class ViewModelFactory
    {
        private static class TypedStorage<T>
            where T : ViewModelBase
        {
            static TypedStorage()
            {
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
            }

            public static AutoFillCacheStorage<string, T> CacheStorage { get; set; }
        }

        /// <summary>
        /// Get storaged view models of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of view model.</typeparam>
        /// <returns>A collection of storaged view models.</returns>
        public static AutoFillCacheStorage<string, T> Storage<T>()
            where T : ViewModelBase
        {
            var s = TypedStorage<T>.CacheStorage;
            if (s is null)
                throw new InvalidOperationException("Did not registed.");
            return s;
        }

        /// <summary>
        /// Get or create view model.
        /// </summary>
        /// <typeparam name="T">Type of view model.</typeparam>
        /// <param name="parameter">Parameter of view model.</param>
        /// <returns>View model of the <paramref name="parameter"/>.</returns>
        public static T Get<T>(string parameter)
            where T : ViewModelBase
        {
            return Storage<T>().GetOrCreateAsync(parameter).GetResults();
        }

        /// <summary>
        /// Set view model of <paramref name="parameter"/>.
        /// </summary>
        /// <typeparam name="T">Type of view model.</typeparam>
        /// <param name="parameter">Parameter of view model.</param>
        /// <param name="viewModel">View model of the <paramref name="parameter"/>.</param>
        /// <returns><paramref name="viewModel"/> itself.</returns>
        public static T Set<T>(string parameter, T viewModel)
            where T : ViewModelBase
        {
            return Storage<T>()[parameter] = viewModel;
        }

        /// <summary>
        /// Register view model to the factory.
        /// </summary>
        /// <typeparam name="T">Type of view model.</typeparam>
        /// <param name="activator">Activator ti create new view models by parameter.</param>
        /// <param name="capacity">Capacity of cached view model.</param>
        public static void Register<T>(Func<string, T> activator, int capacity)
            where T : ViewModelBase
            => Register(activator, capacity, false);

        /// <summary>
        /// Register view model to the factory.
        /// </summary>
        /// <typeparam name="T">Type of view model.</typeparam>
        /// <param name="activator">Activator ti create new view models by parameter.</param>
        public static void Register<T>(Func<string, T> activator)
            where T : ViewModelBase
            => Register(activator, 10);

        /// <summary>
        /// Register view model to the factory.
        /// </summary>
        /// <typeparam name="T">Type of view model.</typeparam>
        /// <param name="activator">Activator ti create new view models by parameter.</param>
        /// <param name="capacity">Capacity of cached view model.</param>
        /// <param name="throwIfRegistered">Will an exception thrown if the view model of <typeparamref name="T"/> has registered.</param>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="throwIfRegistered"/> is <see langword="true"/>,
        /// and the view model of <typeparamref name="T"/> has registered
        /// </exception>
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

        /// <summary>
        /// Unregister view model of the factory.
        /// </summary>
        /// <typeparam name="T">Type of view model.</typeparam>
        public static void Unregister<T>()
            where T : ViewModelBase
        {
            TypedStorage<T>.CacheStorage = null;
        }
    }
}