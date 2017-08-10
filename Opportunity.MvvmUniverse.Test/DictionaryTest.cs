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
            Assert.ThrowsException<ArgumentNullException>(() => new ObservableDictionary<string, int>((IEqualityComparer<string>)null));
            var dic6 = new ObservableDictionary<string, int>((IDictionary<string, int>)null);
            Assert.AreEqual(0, dic6.Count);
            var dic7 = new ObservableDictionary<string, int>(new Dictionary<string, int> { [""] = 1 });
            Assert.AreEqual(1, dic7.Count);
            Assert.AreEqual(1, dic7[""]);
        }

        [TestMethod]
        public void AddElements()
        {
            var dic = new ObservableDictionary<int, int>();
            dic.Add(0, 0);
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
        }

        [TestMethod]
        public void RemoveElements()
        {
            var dic = new ObservableDictionary<int, int>();
            dic.Add(0, 0);
            dic.Add(1, 1);
            dic.Add(2, 2);
            dic.Add(3, 3);
            dic.Add(4, 4);
            dic.Add(5, 5);
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
            Assert.AreEqual(3, dic.IndexOfKey(5));
            Assert.AreEqual(3, dic.IndexOfValue(5));
        }

        [TestMethod]
        public void MoveElements()
        {
            var dic = new ObservableDictionary<int, int>();
            dic.Add(0, 0);
            dic.Add(1, 1);
            dic.Add(2, 2);
            dic.Add(3, 3);
            dic.Add(4, 4);
            dic.Add(5, 5);
            dic.Move(0, 1);
            Assert.AreEqual(1, dic.ItemAt(0).Key);
            Assert.AreEqual(0, dic.ItemAt(1).Key);
            Assert.AreEqual(2, dic.ItemAt(2).Key);
            dic.Move(0, 0);
            Assert.AreEqual(0, dic.ItemAt(0).Key);
            Assert.AreEqual(1, dic.ItemAt(1).Key);
            Assert.AreEqual(2, dic.ItemAt(2).Key);
            dic.Move(0, 4);
            Assert.AreEqual(0, dic.ItemAt(4).Key);
            Assert.AreEqual(4, dic.ItemAt(3).Key);
            Assert.AreEqual(5, dic.ItemAt(5).Key);
            dic.Move(0, 0);
            Assert.AreEqual(0, dic.ItemAt(0).Key);
            Assert.AreEqual(1, dic.ItemAt(1).Key);
            Assert.AreEqual(2, dic.ItemAt(2).Key);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => dic.Move(0, -1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => dic.Move(0, 6));
            Assert.ThrowsException<KeyNotFoundException>(() => dic.Move(6, 0));
        }
    }
}
