using Opportunity.Helpers.ObjectModel;
using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace Opportunity.MvvmUniverse.Services.Notification
{
    /// <summary>
    /// Provides view level notification service.
    /// </summary>
    public class Notificator : ServiceBase<Notificator, INotificationHandler>
    {
        /// <summary>
        /// Get <see cref="Notificator"/> of application.
        /// </summary>
        /// <returns><see cref="Notificator"/> of application.</returns>
        public static Notificator GetForViewIndependent() => Singleton.GetOrCreate(() => new Notificator());

        /// <summary>
        /// Get <see cref="Notificator"/> of current view.
        /// </summary>
        /// <returns><see cref="Notificator"/> of current view.</returns>
        public static Notificator GetForCurrentView()
        {
            if (Window.Current == null)
                return null;
            return ThreadLocalSingleton.GetOrCreate(() => new Notificator());
        }

        /// <summary>
        /// Send notification and returns immediately.
        /// </summary>
        /// <param name="data">Data of notificaiton.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
        public void Notify(object data)
        {
            var ignore = NotifyAsync(data);
        }

        /// <summary>
        /// Send notification and wait for results.
        /// </summary>
        /// <param name="data">Data of notificaiton.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
        /// <returns>Whether the notification was handled by any of <see cref="DependencyServiceBase{Notificator, INotificationHandler}.Handlers"/>.</returns>
        public IAsyncOperation<bool> NotifyAsync(object data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));
            if (Handlers.Count == 0)
                return AsyncOperation<bool>.CreateCompleted();
            return AsyncInfo.Run(async token =>
            {
                var t = default(IAsyncOperation<bool>);
                token.Register(() => t?.Cancel());
                for (var i = Handlers.Count - 1; i >= 0; i--)
                {
                    t = Handlers[i].NotifyAsync(data);
                    var r = await t;
                    if (await t)
                        return true;
                    token.ThrowIfCancellationRequested();
                }
                return false;
            });
        }
    }
}
