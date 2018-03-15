using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace Opportunity.MvvmUniverse.Services.Notification
{
    /// <summary>
    ///  Provides view level notification service.
    /// </summary>
    public class Notificator : DependencyObject, IService<INotificationHandler>
    {
        /// <summary>
        /// Get <see cref="Notificator"/> of current view.
        /// </summary>
        /// <returns><see cref="Notificator"/> of current view.</returns>
        public static Notificator GetForCurrentView()
        {
            var notificator = ViewIndependentSingleton<Notificator>.Value;
            if (notificator != null)
                return notificator;
            if (Window.Current == null)
                return null;
            notificator = new Notificator();
            ViewIndependentSingleton<Notificator>.Value = notificator;
            return notificator;
        }

        private Notificator()
        {
            var h = new ServiceHandlerCollection<Notificator, INotificationHandler>(this);
            this.Handlers = h;
        }

        /// <summary>
        /// Handlers handles notifications.
        /// </summary>
        /// <remarks>
        /// Handlers with greater index will be used first.
        /// </remarks>
        public IList<INotificationHandler> Handlers { get; }

        void IService<INotificationHandler>.UpdateProperties() { }

        public bool Notify(string category, object data)
        {
            for (var i = Handlers.Count - 1; i >= 0; i--)
            {
                if (Handlers[i].Notify(category, data))
                    return true;
            }
            return false;
        }

        public IAsyncOperation<NotificationResult> NotifyAsync(string category, object data)
        {
            if (Handlers.Count == 0)
                return AsyncOperation<NotificationResult>.CreateCompleted();
            return AsyncInfo.Run(async token =>
            {
                var t = default(IAsyncOperation<NotificationResult>);
                token.Register(() => t?.Cancel());
                for (var i = Handlers.Count - 1; i >= 0; i--)
                {
                    t = Handlers[i].NotifyAsync(category, data);
                    var r = await t;
                    if (r != NotificationResult.Unhandled)
                        return r;
                    token.ThrowIfCancellationRequested();
                }
                return NotificationResult.Unhandled;
            });
        }
    }

    /// <summary>
    /// Handler for <see cref="Notificator"/>.
    /// </summary>
    public interface INotificationHandler : IServiceHandler<Notificator>
    {
        bool Notify(string category, object data);

        IAsyncOperation<NotificationResult> NotifyAsync(string category, object data);
    }

    public enum NotificationResult
    {
        Unhandled = 0,
        Positive = 1,
        Negetive = -1,
    }
}
