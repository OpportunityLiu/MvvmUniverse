using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Opportunity.MvvmUniverse.Test
{
    [TestClass]
    public class CacheTest
    {
        [TestMethod]
        public void Random()
        {
            var r = new Random();
            var cache = new CacheStorage<byte, int>(128);
            Assert.AreEqual(128, cache.Capacity);
            var buf = new byte[1000];
            for (var i = 0; i < 100; i++)
            {
                r.NextBytes(buf);
                foreach (var item in buf)
                {
                    cache[item] = item;
                }
                r.NextBytes(buf);
                foreach (var item in buf)
                {
                    if (cache.TryGetValue(item, out var value))
                        Assert.AreEqual(item, value);
                }
            }
            checkCount(cache, 128);
            cache.Clear();
            checkCount(cache, 0);
            for (var i = 0; i < 100; i++)
            {
                r.NextBytes(buf);
                foreach (var item in buf)
                {
                    cache[item] = item;
                }
                r.NextBytes(buf);
                foreach (var item in buf)
                {
                    if (cache.TryGetValue(item, out var value))
                        Assert.AreEqual(item, value);
                }
            }
            checkCount(cache, 128);
        }

        [TestMethod]
        public void AutoFill()
        {
            var r = new Random();
            var cache = AutoFillCacheStorage.Create<byte, int>(i => i, 128);
            Assert.AreEqual(128, cache.Capacity);
            var buf = new byte[1000];
            for (var i = 0; i < 100; i++)
            {
                r.NextBytes(buf);
                foreach (var item in buf)
                {
                    Assert.AreEqual(item, cache.GetOrCreateAsync(item).GetResults());
                }
            }
            checkCount(cache, 128);
            cache.Clear();
            checkCount(cache, 0);
            for (var i = 0; i < 100; i++)
            {
                r.NextBytes(buf);
                foreach (var item in buf)
                {
                    Assert.AreEqual(item, cache.GetOrCreateAsync(item).GetResults());
                }
            }
            checkCount(cache, 128);
        }

        [TestMethod]
        public void AutoFillAsync()
        {
            var r = new Random();
            var cache = AutoFillCacheStorage.Create<byte, int>(i => AsyncOperation<int>.CreateCompleted(i), 128);
            Assert.AreEqual(128, cache.Capacity);
            var buf = new byte[1000];
            for (var i = 0; i < 100; i++)
            {
                r.NextBytes(buf);
                foreach (var item in buf)
                {
                    Assert.AreEqual(item, cache.GetOrCreateAsync(item).GetResults());
                }
            }
            checkCount(cache, 128);
            cache.Clear();
            checkCount(cache, 0);
            for (var i = 0; i < 100; i++)
            {
                r.NextBytes(buf);
                foreach (var item in buf)
                {
                    Assert.AreEqual(item, cache.GetOrCreateAsync(item).GetResults());
                }
            }
            checkCount(cache, 128);
        }

        [TestMethod]
        public void Full()
        {
            var r = new Random();
            var cache = new CacheStorage<byte, int>(256);
            Assert.AreEqual(256, cache.Capacity);
            for (var i = 0; i < 256; i++)
            {
                cache[(byte)i] = i;
            }
            var buf = new byte[1000];
            for (var i = 0; i < 100; i++)
            {
                r.NextBytes(buf);
                foreach (var item in buf)
                {
                    Assert.IsTrue(cache.TryGetValue(item, out var value));
                    Assert.AreEqual(item, value);
                }
            }
            checkCount(cache, 256);
            cache.Clear();
            checkCount(cache, 0);
            for (var i = 0; i < 256; i++)
            {
                cache[(byte)i] = i;
            }
            for (var i = 0; i < 100; i++)
            {
                r.NextBytes(buf);
                foreach (var item in buf)
                {
                    Assert.IsTrue(cache.TryGetValue(item, out var value));
                    Assert.AreEqual(item, value);
                }
            }
            checkCount(cache, 256);
        }

        [TestMethod]
        public void Remove()
        {
            var cache = new CacheStorage<byte, int>(5)
            {
                [0] = 0,
                [1] = 1,
                [2] = 2,
            };
            checkCount(cache, 3);
            Assert.IsFalse(cache.Remove(12));
            checkCount(cache, 3);
            Assert.IsTrue(cache.Remove(1));
            checkCount(cache, 2);
            Assert.IsTrue(cache.ContainsKey(0));
            Assert.IsFalse(cache.ContainsKey(1));
            Assert.IsTrue(cache.ContainsKey(2));
        }

        private void checkCount<TK, TV>(CacheStorage<TK, TV> storage, int expectedCount)
        {
            Assert.AreEqual(expectedCount, storage.Count);
            Assert.AreEqual(expectedCount, storage.Keys.Count);
            Assert.AreEqual(expectedCount, storage.ToArray().Length);
        }
    }
}
