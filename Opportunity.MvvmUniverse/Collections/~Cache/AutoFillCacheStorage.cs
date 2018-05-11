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
    public abstract class AutoFillCacheStorage<TKey, TCache> : CacheStorage<TKey, TCache>
    {
        public AutoFillCacheStorage() { }

        public AutoFillCacheStorage(int capacity)
            : base(capacity) { }

        public AutoFillCacheStorage(int capacity, IEqualityComparer<TKey> comparer)
            : base(capacity, comparer) { }

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

        protected abstract IAsyncOperation<TCache> CreateAsync(TKey key);
    }

    public static class AutoFillCacheStorage
    {
        public static AutoFillCacheStorage<TKey, TCache> Create<TKey, TCache>(Func<TKey, IAsyncOperation<TCache>> cacheCreator)
            => new AsyncDelegateAutoFillCacheStorage<TKey, TCache>(cacheCreator);
        public static AutoFillCacheStorage<TKey, TCache> Create<TKey, TCache>(Func<TKey, IAsyncOperation<TCache>> cacheCreator, int capacity)
            => new AsyncDelegateAutoFillCacheStorage<TKey, TCache>(cacheCreator, capacity);
        public static AutoFillCacheStorage<TKey, TCache> Create<TKey, TCache>(Func<TKey, IAsyncOperation<TCache>> cacheCreator, int capacity, IEqualityComparer<TKey> comparer)
            => new AsyncDelegateAutoFillCacheStorage<TKey, TCache>(cacheCreator, capacity, comparer);

        public static AutoFillCacheStorage<TKey, TCache> Create<TKey, TCache>(Func<TKey, TCache> cacheCreator)
            => new DelegateAutoFillCacheStorage<TKey, TCache>(cacheCreator);
        public static AutoFillCacheStorage<TKey, TCache> Create<TKey, TCache>(Func<TKey, TCache> cacheCreator, int capacity)
            => new DelegateAutoFillCacheStorage<TKey, TCache>(cacheCreator, capacity);
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
