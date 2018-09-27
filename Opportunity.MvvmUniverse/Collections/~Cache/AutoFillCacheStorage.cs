using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// A <see cref="CacheStorage{TKey, TCache}"/> that will create value automatically.
    /// </summary>
    /// <typeparam name="TKey">Key type of cache.</typeparam>
    /// <typeparam name="TCache">Value type of cache.</typeparam>
    public abstract class AutoFillCacheStorage<TKey, TCache> : CacheStorage<TKey, TCache>
    {
        /// <summary>
        /// Create new instance of <see cref="AutoFillCacheStorage{TKey, TCache}"/>.
        /// </summary>
        public AutoFillCacheStorage() { }

        /// <summary>
        /// Create new instance of <see cref="AutoFillCacheStorage{TKey, TCache}"/>.
        /// </summary>
        /// <param name="capacity">Capacity of stroage.</param>
        public AutoFillCacheStorage(int capacity)
            : base(capacity) { }

        /// <summary>
        /// Create new instance of <see cref="AutoFillCacheStorage{TKey, TCache}"/>.
        /// </summary>
        /// <param name="capacity">Capacity of stroage.</param>
        /// <param name="comparer"><see cref="IEqualityComparer{T}"/> for key comparsion.</param>
        public AutoFillCacheStorage(int capacity, IEqualityComparer<TKey> comparer)
            : base(capacity, comparer) { }

        /// <summary>
        /// Get or create data of given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key of cached data.</param>
        /// <returns>Cached data of <paramref name="key"/>, 
        /// if not found in the cache storage, <see cref="CreateAsync(TKey)"/> will be invoked to create one.</returns>
        public IAsyncOperation<TCache> GetOrCreateAsync(TKey key)
        {
            if (TryGetValue(key, out var r))
                return AsyncOperation<TCache>.CreateCompleted(r);
            var task = CreateAsync(key);
            switch (task.Status)
            {
            case AsyncStatus.Completed:
                r = task.GetResults();
                Add(key, r);
                return task;
            case AsyncStatus.Started:
                return AsyncInfo.Run(async token =>
                {
                    var result = await task;
                    Add(key, result);
                    return result;
                });
            default:
                return task;
            }
        }

        /// <summary>
        /// Create cached data of given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key of cached data.</param>
        /// <returns>Cached data of given <paramref name="key"/>.</returns>
        protected abstract IAsyncOperation<TCache> CreateAsync(TKey key);
    }

    /// <summary>
    /// Factory class for <see cref="AutoFillCacheStorage{TKey, TCache}"/>.
    /// </summary>
    public static class AutoFillCacheStorage
    {
        /// <summary>
        /// Create new instance of <see cref="AutoFillCacheStorage{TKey, TCache}"/>.
        /// </summary>
        /// <typeparam name="TKey">Key type of cache.</typeparam>
        /// <typeparam name="TCache">Value type of cache.</typeparam>
        /// <param name="cacheCreator">Implemetation of <see cref="AutoFillCacheStorage{TKey, TCache}.CreateAsync(TKey)"/></param>
        /// <returns>New instance of <see cref="AutoFillCacheStorage{TKey, TCache}"/>.</returns>
        public static AutoFillCacheStorage<TKey, TCache> Create<TKey, TCache>(Func<TKey, IAsyncOperation<TCache>> cacheCreator)
            => new AsyncDelegateAutoFillCacheStorage<TKey, TCache>(cacheCreator);
        /// <summary>
        /// Create new instance of <see cref="AutoFillCacheStorage{TKey, TCache}"/>.
        /// </summary>
        /// <typeparam name="TKey">Key type of cache.</typeparam>
        /// <typeparam name="TCache">Value type of cache.</typeparam>
        /// <param name="cacheCreator">Implemetation of <see cref="AutoFillCacheStorage{TKey, TCache}.CreateAsync(TKey)"/></param>
        /// <param name="capacity">Capacity of stroage.</param>
        /// <returns>New instance of <see cref="AutoFillCacheStorage{TKey, TCache}"/>.</returns>
        public static AutoFillCacheStorage<TKey, TCache> Create<TKey, TCache>(Func<TKey, IAsyncOperation<TCache>> cacheCreator, int capacity)
            => new AsyncDelegateAutoFillCacheStorage<TKey, TCache>(cacheCreator, capacity);
        /// <summary>
        /// Create new instance of <see cref="AutoFillCacheStorage{TKey, TCache}"/>.
        /// </summary>
        /// <typeparam name="TKey">Key type of cache.</typeparam>
        /// <typeparam name="TCache">Value type of cache.</typeparam>
        /// <param name="cacheCreator">Implemetation of <see cref="AutoFillCacheStorage{TKey, TCache}.CreateAsync(TKey)"/></param>
        /// <param name="capacity">Capacity of stroage.</param>
        /// <param name="comparer"><see cref="IEqualityComparer{T}"/> for key comparsion.</param>
        /// <returns>New instance of <see cref="AutoFillCacheStorage{TKey, TCache}"/>.</returns>
        public static AutoFillCacheStorage<TKey, TCache> Create<TKey, TCache>(Func<TKey, IAsyncOperation<TCache>> cacheCreator, int capacity, IEqualityComparer<TKey> comparer)
            => new AsyncDelegateAutoFillCacheStorage<TKey, TCache>(cacheCreator, capacity, comparer);

        /// <summary>
        /// Create new instance of <see cref="AutoFillCacheStorage{TKey, TCache}"/>.
        /// </summary>
        /// <typeparam name="TKey">Key type of cache.</typeparam>
        /// <typeparam name="TCache">Value type of cache.</typeparam>
        /// <param name="cacheCreator">Implemetation of <see cref="AutoFillCacheStorage{TKey, TCache}.CreateAsync(TKey)"/></param>
        /// <returns>New instance of <see cref="AutoFillCacheStorage{TKey, TCache}"/>.</returns>
        public static AutoFillCacheStorage<TKey, TCache> Create<TKey, TCache>(Func<TKey, TCache> cacheCreator)
            => new DelegateAutoFillCacheStorage<TKey, TCache>(cacheCreator);
        /// <summary>
        /// Create new instance of <see cref="AutoFillCacheStorage{TKey, TCache}"/>.
        /// </summary>
        /// <typeparam name="TKey">Key type of cache.</typeparam>
        /// <typeparam name="TCache">Value type of cache.</typeparam>
        /// <param name="cacheCreator">Implemetation of <see cref="AutoFillCacheStorage{TKey, TCache}.CreateAsync(TKey)"/></param>
        /// <param name="capacity">Capacity of stroage.</param>
        /// <returns>New instance of <see cref="AutoFillCacheStorage{TKey, TCache}"/>.</returns>
        public static AutoFillCacheStorage<TKey, TCache> Create<TKey, TCache>(Func<TKey, TCache> cacheCreator, int capacity)
            => new DelegateAutoFillCacheStorage<TKey, TCache>(cacheCreator, capacity);
        /// <summary>
        /// Create new instance of <see cref="AutoFillCacheStorage{TKey, TCache}"/>.
        /// </summary>
        /// <typeparam name="TKey">Key type of cache.</typeparam>
        /// <typeparam name="TCache">Value type of cache.</typeparam>
        /// <param name="cacheCreator">Implemetation of <see cref="AutoFillCacheStorage{TKey, TCache}.CreateAsync(TKey)"/></param>
        /// <param name="capacity">Capacity of stroage.</param>
        /// <param name="comparer"><see cref="IEqualityComparer{T}"/> for key comparsion.</param>
        /// <returns>New instance of <see cref="AutoFillCacheStorage{TKey, TCache}"/>.</returns>
        public static AutoFillCacheStorage<TKey, TCache> Create<TKey, TCache>(Func<TKey, TCache> cacheCreator, int capacity, IEqualityComparer<TKey> comparer)
            => new DelegateAutoFillCacheStorage<TKey, TCache>(cacheCreator, capacity, comparer);

        private sealed class AsyncDelegateAutoFillCacheStorage<TKey, TCache> : AutoFillCacheStorage<TKey, TCache>
        {
            public AsyncDelegateAutoFillCacheStorage(Func<TKey, IAsyncOperation<TCache>> creator)
            {
                this.creator = creator ?? throw new ArgumentNullException(nameof(creator));
            }

            public AsyncDelegateAutoFillCacheStorage(Func<TKey, IAsyncOperation<TCache>> creator, int capacity)
                : base(capacity)
            {
                this.creator = creator ?? throw new ArgumentNullException(nameof(creator));
            }

            public AsyncDelegateAutoFillCacheStorage(Func<TKey, IAsyncOperation<TCache>> creator, int capacity, IEqualityComparer<TKey> comparer)
                : base(capacity, comparer)
            {
                this.creator = creator ?? throw new ArgumentNullException(nameof(creator));
            }

            private readonly Func<TKey, IAsyncOperation<TCache>> creator;
            protected override IAsyncOperation<TCache> CreateAsync(TKey key)
            {
                try
                {
                    return this.creator(key);
                }
                catch (Exception ex)
                {
                    return AsyncOperation<TCache>.CreateFault(ex);
                }
            }
        }

        private sealed class DelegateAutoFillCacheStorage<TKey, TCache> : AutoFillCacheStorage<TKey, TCache>
        {
            public DelegateAutoFillCacheStorage(Func<TKey, TCache> creator)
            {
                this.creator = creator ?? throw new ArgumentNullException(nameof(creator));
            }

            public DelegateAutoFillCacheStorage(Func<TKey, TCache> creator, int capacity)
                : base(capacity)
            {
                this.creator = creator ?? throw new ArgumentNullException(nameof(creator));
            }

            public DelegateAutoFillCacheStorage(Func<TKey, TCache> creator, int capacity, IEqualityComparer<TKey> comparer)
                : base(capacity, comparer)
            {
                this.creator = creator ?? throw new ArgumentNullException(nameof(creator));
            }

            private readonly Func<TKey, TCache> creator;
            protected override IAsyncOperation<TCache> CreateAsync(TKey key)
            {
                try
                {
                    return AsyncOperation<TCache>.CreateCompleted(this.creator(key));
                }
                catch (Exception ex)
                {
                    return AsyncOperation<TCache>.CreateFault(ex);
                }
            }
        }
    }
}
