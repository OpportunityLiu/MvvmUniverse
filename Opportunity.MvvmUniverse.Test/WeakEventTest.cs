using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Opportunity.MvvmUniverse.Test
{
    [TestClass]
    public class WeakEventTest
    {
        [TestMethod]
        public void Create()
        {
            var wef1 = Assert.ThrowsException<TypeInitializationException>(() => new WeakEvent<Func<int>>());
            var wef2 = Assert.ThrowsException<TypeInitializationException>(() => new WeakEvent<Func<object>>());
            var wef3 = Assert.ThrowsException<TypeInitializationException>(() => new WeakEvent<Func<object, object>>());
            var wea1 = new WeakEvent<Action<int>>();
            var wea2 = new WeakEvent<Action>();
            var we1 = new WeakEvent<Commands.ExecutingEventHandler>();
            var we2 = new WeakEvent<Commands.ExecutingEventHandler<int>>();
            var we3 = new WeakEvent<Commands.ExecutingEventHandler<object>>();
        }
    }
}
