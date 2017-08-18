using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Opportunity.MvvmUniverse.Test
{
    [TestClass]
    public class ListTest
    {
        [DataRow(5, "123", "asfui", DisplayName = "Irrelevant short to long")]
        [DataRow(8, "21365489", "asfui", DisplayName = "Irrelevant 2 long to short")]
        [DataRow(5, "21365", "asfui", DisplayName = "Irrelevant 2 length equles")]
        [DataRow(5, "", "asfui", DisplayName = "Fill")]
        [DataRow(3, "123", "", DisplayName = "Empty")]
        [DataRow(0, "asfasdg", "asfasdg", DisplayName = "Keep")]
        [DataRow(2, "123", "12345", DisplayName = "Insert back")]
        [DataRow(2, "123", "00123", DisplayName = "Insert front")]
        [DataRow(2, "123", "10023", DisplayName = "Insert middle")]
        [DataRow(7, "123", "0010020030", DisplayName = "Insert")]
        [DataRow(2, "12345", "123", DisplayName = "Remove back")]
        [DataRow(2, "00123", "123", DisplayName = "Remove front")]
        [DataRow(2, "10023", "123", DisplayName = "Remove middle")]
        [DataRow(7, "0010020030", "123", DisplayName = "Remove")]
        [DataRow(3, "12345", "12ABC", DisplayName = "Set back")]
        [DataRow(1, "12345", "12A45", DisplayName = "Set middle")]
        [DataRow(3, "12345", "BCA45", DisplayName = "Set front")]
        [DataRow(8, "democrat", "republican", DisplayName = "Common 1")]
        [DataRow(8, "republican", "democrat", DisplayName = "Common 1 Inv")]
        [DataRow(3, "kitten", "sitting", DisplayName = "Common 2")]
        [DataRow(3, "sitting", "kitten", DisplayName = "Common 2 Inv")]
        [DataTestMethod]
        public void Update(int med, string source, string target)
        {
            var l = new CharList(source);
            var targetArray = target.ToCharArray();
            var m = l.Update(targetArray, EqualityComparer<char>.Default, Assert.AreEqual);
            Assert.AreEqual(med, m);
            CollectionAssert.AreEqual(targetArray, l);
            l.Clear();

            l.AddRange(source);
            l.Update(targetArray);
            CollectionAssert.AreEqual(targetArray, l);
            l.Clear();
        }

        public class CharList : ObservableList<char>
        {
            public CharList()
            {
            }

            public CharList(IEnumerable<char> items) : base(items)
            {
            }

            public override string ToString()
            {
                return new string(this.ToArray());
            }
        }
    }
}
