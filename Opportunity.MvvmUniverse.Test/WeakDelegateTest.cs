using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opportunity.MvvmUniverse.Delegates;
using System;

namespace Opportunity.MvvmUniverse.Test
{
    [TestClass]
    public class WeakDelegateTest
    {
        [TestMethod]
        public void WeakActionNormal()
        {
            var i = 1;
            var a = new WeakAction(() => i++);
            a.Invoke();
            Assert.AreEqual(2, i);
        }

        [TestMethod]
        public void WeakActionMulti()
        {
            var i = 1;
            Action act = () => i++;
            act += () => i++;
            Assert.ThrowsException<NotSupportedException>(() => new WeakAction(act));
        }
    }
}