using System;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Services.Activation
{
    /// <summary>
    /// Handler for <see cref="Activator"/>.
    /// </summary>
    public interface IActivationHandler : IServiceHandler<Activator>
    {
        /// <summary>
        /// Whether activation handled by previous handler should handled by this <see cref="IActivationHandler"/>.
        /// </summary>
        bool HandledToo { get; }

        /// <summary>
        /// Handles activation.
        /// </summary>
        /// <param name="args">Args of activation.</param>
        /// <param name="handled">If the activation has been handled or not.</param>
        /// <returns>Whether activation is handled by this <see cref="IActivationHandler"/>.</returns>
        IAsyncOperation<bool> ActivateAsync(IActivatedEventArgs args, bool handled);
    }
}
