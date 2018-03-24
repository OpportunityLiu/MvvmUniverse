using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Specialized;
using Windows.Foundation;
using Opportunity.Helpers.Universal.AsyncHelpers;
using System.Runtime.InteropServices.WindowsRuntime;
using Opportunity.MvvmUniverse.Commands;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using Windows.ApplicationModel.Activation;

namespace Opportunity.MvvmUniverse.Services.Activation
{
    /// <summary>
    /// Provides view level navigation service.
    /// </summary>
    public sealed class Activator : ObservableObject, IService<IActivationHandler>
    {
        /// <summary>
        /// The singlelon of <see cref="Activator"/>.
        /// </summary>
        public static Activator Current { get; } = new Activator();

        private Activator()
        {
            this.handlers = new ServiceHandlerCollection<Activator, IActivationHandler>(this);
        }

        private readonly ServiceHandlerCollection<Activator, IActivationHandler> handlers;
        /// <inheritdoc />
        /// <summary>
        /// Handlers handles navigation methods.
        /// </summary>
        /// <remarks>
        /// Handlers with greater index will be used first.
        /// </remarks>
        public IList<IActivationHandler> Handlers => this.handlers;

        void IService<IActivationHandler>.UpdateProperties() { }

        /// <summary>
        /// Handles activation.
        /// </summary>
        /// <param name="args">Args of activation.</param>
        /// <returns>Whether activation is handled.</returns>
        public IAsyncOperation<bool> ActivateAsync(IActivatedEventArgs args)
        {
            return AsyncInfo.Run(async token =>
            {
                var handled = false;
                for (var i = Handlers.Count - 1; i >= 0; i--)
                {
                    var handler = Handlers[i];
                    if (!handled)
                    {
                        handled = await Handlers[i].ActivateAsync(args, false);
                    }
                    else if (handler.HandledToo)
                    {
                        await Handlers[i].ActivateAsync(args, true);
                    }
                }
                return handled;
            });
        }
    }
}
