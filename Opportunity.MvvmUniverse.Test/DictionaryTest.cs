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
        public void MoveElements()
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

        [DataRow("123", "asfui", DisplayName = "Irrelevant short to long")]
        [DataRow("21365489", "asfui", DisplayName = "Irrelevant long to short")]
        [DataRow("21365", "asfui", DisplayName = "Irrelevant length equles")]
        [DataRow("", "asfui", DisplayName = "Fill")]
        [DataRow("123", "", DisplayName = "Empty")]
        [DataRow("asfasdg", "asfasdg", DisplayName = "Keep")]
        [DataRow("123", "12345", DisplayName = "Insert back")]
        [DataRow("123", "00123", DisplayName = "Insert front")]
        [DataRow("123", "10023", DisplayName = "Insert middle")]
        [DataRow("123", "0010020030", DisplayName = "Insert")]
        [DataRow("12345", "123", DisplayName = "Remove back")]
        [DataRow("00123", "123", DisplayName = "Remove front")]
        [DataRow("10023", "123", DisplayName = "Remove middle")]
        [DataRow("0010020030", "123", DisplayName = "Remove")]
        [DataRow("12345", "12ABC", DisplayName = "Set back")]
        [DataRow("12345", "12A45", DisplayName = "Set middle")]
        [DataRow("12345", "BCA45", DisplayName = "Set front")]
        [DataRow("democrat", "republican", DisplayName = "Common 1")]
        [DataRow("republican", "democrat", DisplayName = "Common 1 Inv")]
        [DataRow("kitten", "sitting", DisplayName = "Common 2")]
        [DataRow("sitting", "kitten", DisplayName = "Common 2 Inv")]
        [DataTestMethod]
        public void Update(string source, string target)
        {

        }
    }
}
