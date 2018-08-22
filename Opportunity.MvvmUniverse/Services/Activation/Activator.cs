using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse.Collections;
using Opportunity.MvvmUniverse.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Opportunity.MvvmUniverse.Services.Activation
{
    /// <summary>
    /// Provides view level navigation service.
    /// </summary>
    public sealed class Activator : ServiceBase<Activator, IActivationHandler>
    {
        /// <summary>
        /// The singlelon of <see cref="Activator"/>.
        /// </summary>
        public static Activator Current { get; } = new Activator();

        private Activator() { }

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
