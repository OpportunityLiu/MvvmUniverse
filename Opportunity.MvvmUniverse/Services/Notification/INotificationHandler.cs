using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Services.Notification
{
    /// <summary>
    /// Handler for <see cref="Notificator"/>.
    /// </summary>
    public interface INotificationHandler : IServiceHandler<Notificator>
    {
        /// <summary>
        /// Handles notification.
        /// </summary>
        /// <param name="data">Data of notificaiton.</param>
        /// <returns>Whether notification is handled by this <see cref="INotificationHandler"/>.</returns>
        IAsyncOperation<bool> NotifyAsync(object data);
    }
}
