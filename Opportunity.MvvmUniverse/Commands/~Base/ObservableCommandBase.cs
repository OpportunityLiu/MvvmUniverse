using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Base class of all commands.
    /// </summary>
    public abstract class ObservableCommandBase : ObservableObject, IControllable
    {
        internal ObservableCommandBase() { }

        private bool isEnabled = true;
        /// <summary>
        /// Overall switch of command.
        /// </summary>
        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (Set(ref this.isEnabled, value))
                    OnCanExecuteChanged();
            }
        }

        private object tag;
        /// <summary>
        /// A tag associated with this object.
        /// </summary>
        public object Tag
        {
            get => this.tag;
            set => ForceSet(ref this.tag, value);
        }

        internal static void ThrowUnhandledError(Exception error)
        {
            if (error is null)
                return;
            var d = CoreApplication.MainView?.Dispatcher;
            if (d is null)
            {
                if (error is AggregateException ae)
                    throw new AggregateException(ae.InnerExceptions);
                else
                    throw new AggregateException(error);
            }
            else
                d.Begin(() =>
                {
                    if (error is AggregateException ae)
                        throw new AggregateException(ae.InnerExceptions);
                    else
                        throw new AggregateException(error);
                });
        }

        private readonly DepedencyEvent<EventHandler, ObservableCommandBase, EventArgs> canExecuteChanged
            = new DepedencyEvent<EventHandler, ObservableCommandBase, EventArgs>((h, s, e) => h(s, e));
        /// <summary>
        /// Raise when return value of <see cref="System.Windows.Input.ICommand.CanExecute(object)"/> changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => this.canExecuteChanged.Add(value);
            remove => this.canExecuteChanged.Remove(value);
        }

        /// <summary>
        /// Raise <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public virtual void OnCanExecuteChanged()
        {
            this.canExecuteChanged.Raise(this, EventArgs.Empty);
        }
    }
}
