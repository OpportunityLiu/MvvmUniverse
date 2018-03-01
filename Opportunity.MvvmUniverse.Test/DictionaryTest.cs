using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Test
{
    [TestClass]
    public class DictionaryTest
    {
        [TestMethod]
        public void Basic()
        {
            var dic1 = new ObservableDictionary<int, int>();
            var dic2 = new ObservableDictionary<object, int>();
            var dic3 = new ObservableDictionary<string, string>();
            var dic4 = new ObservableDictionary<string, object>();
            Assert.AreEqual(EqualityComparer<string>.Default, dic4.Comparer);
            var dic5 = new ObservableDictionary<string, int>(StringComparer.CurrentCulture);
            Assert.AreEqual(StringComparer.CurrentCulture, dic5.Comparer);
            var dic6 = new ObservableDictionary<string, int>((IDictionary<string, int>)null);
            Assert.AreEqual(0, dic6.Count);
            var dic7 = new ObservableDictionary<string, int>(new Dictionary<string, int> { [""] = 1 });
            Assert.AreEqual(1, dic7.Count);
            Assert.AreEqual(1, dic7[""]);
        }

        [TestMethod]
        public void AddElements()
        {
            var dic = new ObservableDictionary<int, int>
            {
                { 0, 0 },
            };
            Assert.AreEqual(0, dic[0]);
            Assert.ThrowsException<ArgumentException>(() => dic.Add(0, 1));
            dic[0] = 1;
            Assert.AreEqual(1, dic[0]);
            Assert.AreEqual(1, dic.Count);
            dic[1] = 1;
            Assert.AreEqual(1, dic[1]);
            Assert.AreEqual(2, dic.Count);
            Assert.ThrowsException<ArgumentException>(() => dic.Insert(0, 0, 0));
            dic.Insert(0, 2, 2);
            Assert.AreEqual(2, dic.ItemAt(0).Key);
            Assert.AreEqual(0, dic.ItemAt(1).Key);
            Assert.AreEqual(1, dic.ItemAt(2).Key);
            Assert.AreEqual(2, dic.ItemAt(0).Value);
            Assert.AreEqual(1, dic.ItemAt(1).Value);
            Assert.AreEqual(1, dic.ItemAt(2).Value);
        }

        [TestMethod]
        public void RemoveElements()
        {
            var dic = new ObservableDictionary<int, int>
            {
                { 0, 0 },
                { 1, 1 },
                { 2, 2 },
                { 3, 3 },
                { 4, 4 },
                { 5, 5 },
            };
            Assert.AreEqual(6, dic.Count);
            Assert.AreEqual(true, dic.Remove(0));
            Assert.AreEqual(5, dic.Count);
            Assert.AreEqual(false, dic.Remove(0));
            dic.RemoveAt(2);
            Assert.AreEqual(4, dic.Count);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => dic.RemoveAt(-1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => dic.RemoveAt(4));
            Assert.ThrowsException<KeyNotFoundException>(() => dic[3]);
            Assert.AreEqual(4, dic.ItemAt(2).Key);
            Assert.AreEqual(4, dic.ItemAt(2).Value);
            Assert.AreEqual(5, dic[5]);
            Assert.AreEqual(3, dic.Keys.IndexOf(5));
            Assert.AreEqual(3, dic.Values.IndexOf(5));
        }

        [TestMethod]
        public void Update()
        {
            var dic = new ObservableDictionary<string, string>
            {
                ["a"] = "1",
                ["b"] = "2",
                ["c"] = "3",
                ["d"] = "4",
                ["e"] = "5",
            };
            {
                var shot = new Dictionary<string, string>
                {
                    ["a"] = "2",
                    ["e"] = "52",
                    ["b"] = "325",
                    ["d"] = "1",
                };
                dic.Update(shot);
                CollectionAssert.AreEqual(shot, dic);
            }
            {
                var shot = new Dictionary<string, string>
                {
                    ["a"] = "2",
                    ["b"] = "3",
                    ["c"] = "4",
                    ["d"] = "1",
                    ["f"] = "dd1",
                    ["e"] = "5",
                };
                dic.Update(shot);
                CollectionAssert.AreEqual(shot, dic);
            }
            {
                var shot = new Dictionary<string, string>
                {
                    ["f"] = "dd1",
                    ["e"] = "5",
                    ["c"] = "4",
                    ["d"] = "1",
                    ["a"] = "2",
                    ["b"] = "3",
                };
                dic.Update(shot);
                CollectionAssert.AreEqual(shot, dic);
            }
            {
                var shot = new Dictionary<string, string>
                {
                };
                dic.Update(shot);
                CollectionAssert.AreEqual(shot, dic);
            }
            {
                var shot = new Dictionary<string, string>
                {
                    ["a"] = "2",
                    ["b"] = "3",
                    ["c"] = "4",
                    ["d"] = "1",
                    ["e"] = "5",
                };
                dic.Update(shot);
                CollectionAssert.AreEqual(shot, dic);
            }
        }
    }
}
