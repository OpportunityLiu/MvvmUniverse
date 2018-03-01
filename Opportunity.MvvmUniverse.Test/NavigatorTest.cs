using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using Opportunity.MvvmUniverse.Commands;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Test
{
    [TestClass]
    public class NavigatorTest
    {
        [TestMethod]
        public void NonUI()
        {
            Assert.ThrowsException<InvalidOperationException>(() => Navigator.GetForCurrentView());
            Assert.ThrowsException<InvalidOperationException>(() => Navigator.GetOrCreateForCurrentView());
            Assert.ThrowsException<InvalidOperationException>(() => Navigator.DestoryForCurrentView());
        }

        [UITestMethod]
        public void UI()
        {
            Assert.IsNull(Navigator.GetForCurrentView());
            var navigator = Navigator.GetOrCreateForCurrentView();
            Assert.IsNotNull(navigator);
            Assert.AreSame(navigator, Navigator.GetForCurrentView());
            Assert.AreSame(navigator, Navigator.GetOrCreateForCurrentView());
            Navigator.DestoryForCurrentView();
            Assert.ThrowsException<InvalidOperationException>(() => navigator.CanGoBack());
            Assert.IsNull(Navigator.GetForCurrentView());
            Assert.IsNotNull(Navigator.GetOrCreateForCurrentView());
            Assert.ThrowsException<InvalidOperationException>(() => navigator.CanGoBack());
            Assert.AreNotSame(navigator, Navigator.GetForCurrentView());
        }
    }
}
