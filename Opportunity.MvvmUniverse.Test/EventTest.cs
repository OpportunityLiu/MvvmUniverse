using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Opportunity.MvvmUniverse.Test
{
    [TestClass]
    public class EventTest
    {
        [TestMethod]
        public void CreateWeakEvent()
        {
            var wef1 = Assert.ThrowsException<TypeInitializationException>(() => new WeakEvent<Func<int>, int, EventArgs>());
            var wef2 = Assert.ThrowsException<TypeInitializationException>(() => new WeakEvent<Func<object>, int, EventArgs>());
            var wef3 = Assert.ThrowsException<TypeInitializationException>(() => new WeakEvent<Func<object>, object, EventArgs>());
            var wef4 = Assert.ThrowsException<TypeInitializationException>(() => new WeakEvent<Func<object, object>, int, EventArgs>());
            var wef5 = Assert.ThrowsException<TypeInitializationException>(() => new WeakEvent<Func<object, object>, object, EventArgs>());
            var wea1 = Assert.ThrowsException<TypeInitializationException>(() => new WeakEvent<Action<int>, object, EventArgs>());
            var wea2 = Assert.ThrowsException<TypeInitializationException>(() => new WeakEvent<Action, object, EventArgs>());
            var wea3 = Assert.ThrowsException<TypeInitializationException>(() => new WeakEvent<Action<object, object>, int, EventArgs>());
            var wea4 = Assert.ThrowsException<TypeInitializationException>(() => new WeakEvent<Action<object, object>, object, EventArgs>());
            var wea5 = Assert.ThrowsException<TypeInitializationException>(() => new WeakEvent<Action<object, EventArgs>, int, EventArgs>());
            var we1 = Assert.ThrowsException<TypeInitializationException>(() => new WeakEvent<ExecutingEventHandler, object, EventArgs>());
            var we2 = Assert.ThrowsException<TypeInitializationException>(() => new WeakEvent<ExecutingEventHandler, ICommand, EventArgs>());
            var we3 = Assert.ThrowsException<TypeInitializationException>(() => new WeakEvent<ExecutingEventHandler, object, ExecutingEventArgs>());
            var we4 = Assert.ThrowsException<TypeInitializationException>(() => new WeakEvent<ExecutingEventHandler, Command, ExecutingEventArgs>());
            var we5 = new WeakEvent<ExecutingEventHandler, ICommand, ExecutingEventArgs>();
        }

        [TestMethod]
        public void TestDepedencyEvent()
        {
            var eve = new DepedencyEvent<Action<object, int>, object, int>((a, s, e) => a(s, e));
            var r1 = eve.Add((s, e) => { Assert.IsNotNull(s); });
            Assert.AreNotEqual(default, r1);
            var r2 = eve.Add(null);
            Assert.AreEqual(default, r2);
            Assert.AreEqual(1, eve.InvocationListLength);
            eve.Raise(1, 1);
        }
    }
}
