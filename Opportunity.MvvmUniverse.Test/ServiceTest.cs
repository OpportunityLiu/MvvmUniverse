using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse.Collections;
using Opportunity.MvvmUniverse.Services;
using Opportunity.MvvmUniverse.Services.Activation;
using Opportunity.MvvmUniverse.Services.Navigation;
using Opportunity.MvvmUniverse.Services.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Activator = Opportunity.MvvmUniverse.Services.Activation.Activator;

namespace Opportunity.MvvmUniverse.Test
{
    [TestClass]
    public class ServiceTest
    {
        [TestMethod]
        public async Task ActivationService()
        {
            var s = Activator.Current;
            Assert.AreSame(s, Activator.Current);
            Assert.IsFalse(await s.ActivateAsync(null));

            Assert.IsNotNull(Notificator.GetForViewIndependent());
            Assert.AreSame(Notificator.GetForViewIndependent(), Notificator.GetForViewIndependent());
        }

        [TestMethod]
        public void DepServiceNonUI()
        {
            Assert.IsNull(Navigator.GetForCurrentView());
            Assert.ThrowsException<Exception>(() => Navigator.GetOrCreateForCurrentView());
            Assert.IsFalse(Navigator.DestoryForCurrentView());

            Assert.IsNull(Notificator.GetForCurrentView());
        }

        [UITestMethod]
        public void NavigatorService()
        {
            Assert.IsNull(Navigator.GetForCurrentView());
            var navigator = Navigator.GetOrCreateForCurrentView();
            Assert.IsNotNull(navigator);
            Assert.AreSame(navigator, Navigator.GetForCurrentView());
            Assert.AreSame(navigator, Navigator.GetOrCreateForCurrentView());
            Navigator.DestoryForCurrentView();

            Assert.IsFalse(navigator.CanGoBack);
            Assert.IsNull(Navigator.GetForCurrentView());
            Assert.IsNotNull(Navigator.GetOrCreateForCurrentView());
            Assert.IsFalse(navigator.CanGoBack);
            Assert.AreNotSame(navigator, Navigator.GetForCurrentView());
        }

        [UITestMethod]
        public void NotificatorService()
        {
            Assert.IsNotNull(Notificator.GetForCurrentView());
            var no = Notificator.GetForCurrentView();
            Assert.AreSame(no, Notificator.GetForCurrentView());

            Assert.ThrowsException<ArgumentNullException>(() => no.Notify(null));
            Assert.IsFalse(no.NotifyAsync(new object()).GetResults());
        }
    }
}
