using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace Opportunity.MvvmUniverse.Collections
{
    public class CacheStorage<TKey, TCache>
    {
        public CacheStorage(Func<TKey, IAsyncOperation<TCache>> loader)
            : this(loader, 100) { }

        public CacheStorage(Func<TKey, IAsyncOperation<TCache>> loader, int maxCount)
            : this(loader, maxCount, null) { }

        public CacheStorage(Func<TKey, IAsyncOperation<TCache>> loader, int maxCount, IEqualityComparer<TKey> comparer)
            : this(maxCount, comparer)
        {
            this.asyncLoader = loader;
        }

        public CacheStorage(Func<TKey, TCache> loader)
            : this(loader, 100, null) { }

        public CacheStorage(Func<TKey, TCache> loader, int maxCount)
            : this(loader, maxCount, null) { }

        public CacheStorage(Func<TKey, TCache> loader, int maxCount, IEqualityComparer<TKey> comparer)
            : this(maxCount, comparer)
        {
            this.loader = loader;
        }

        private CacheStorage(int maxCount, IEqualityComparer<TKey> comparer)
        {
            this.MaxCount = maxCount;
            this.cacheDictionary = new Dictionary<TKey, TCache>(maxCount, comparer);
            this.cacheQueue = new List<TKey>(maxCount);
        }

        private readonly List<TKey> cacheQueue;
        private readonly Dictionary<TKey, TCache> cacheDictionary;

        private readonly Func<TKey, IAsyncOperation<TCache>> asyncLoader;
        private readonly Func<TKey, TCache> loader;

        public int MaxCount
        {
            get; set;
        }

        public void Add(TKey key, TCache value)
        {
            if (!this.cacheDictionary.ContainsKey(key))
                this.cacheQueue.Add(key);
            this.cacheDictionary[key] = value;
        }

        public IAsyncOperation<TCache> GetAsync(TKey key)
        {
            if (this.TryGet(key, out var r))
                return AsyncWrapper.CreateCompleted(r);
            EnsureCapacity();
            this.cacheQueue.Add(key);
            if (this.asyncLoader == null)
            {
                var result = this.loader(key);
                this.cacheDictionary[key] = result;
                return AsyncWrapper.CreateCompleted(result);
            }
            return Run(async token =>
            {
                return this.cacheDictionary[key] = await this.asyncLoader(key);
            });
        }

        public TCache Get(TKey key)
        {
            if (this.TryGet(key, out var r))
                return r;
            EnsureCapacity();
            this.cacheQueue.Add(key);
            if (this.asyncLoader != null)
            {
                var load = this.asyncLoader(key);
                if (load.Status == AsyncStatus.Completed)
                    return this.cacheDictionary[key] = load.GetResults();
                else
                    return this.cacheDictionary[key] = this.asyncLoader(key).AsTask().Result;
            }
            else
                return this.cacheDictionary[key] = this.loader(key);
        }

        public bool ContainsKey(TKey key)
        {
            return this.cacheDictionary.ContainsKey(key);
        }

        public bool ContainsValue(TCache value)
        {
            return this.cacheDictionary.ContainsValue(value);
        }

        public bool TryGet(TKey key, out TCache value)
        {
            var r = this.cacheDictionary.TryGetValue(key, out value);
            if (r)
            {
                var index = this.cacheQueue.FindIndex(k => this.cacheDictionary.Comparer.Equals(k, key));
                if (index >= 0)
                {
                    this.cacheQueue.RemoveAt(index);
                    this.cacheQueue.Add(key);
                }
            }
            return r;
        }

        public void EnsureCapacity()
        {
            var overflow = this.cacheQueue.Count - this.MaxCount;
            for (var i = 0; i < overflow; i++)
                this.cacheDictionary.Remove(this.cacheQueue[i]);
            if (overflow > 0)
                this.cacheQueue.RemoveRange(0, overflow);
        }
    }

}
