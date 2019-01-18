using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
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

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object tag;
        /// <summary>
        /// A tag associated with this object.
        /// </summary>
        public object Tag
        {
            get => this.tag;
            set => ForceSet(ref this.tag, value);
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
        /// Call <see cref="ObservableObject.OnObjectReset()"/> and <see cref="OnCanExecuteChanged()"/>.
        /// </summary>
        public override void OnObjectReset()
        {
            base.OnObjectReset();
            OnCanExecuteChanged();
        }

        /// <summary>
        /// Raise <see cref="CanExecuteChanged"/> event
        /// if <see cref="ObservableObject.NotificationSuspending"/> is <see langword="false"/>.
        /// </summary>
        public virtual void OnCanExecuteChanged()
        {
            if (NotificationSuspending)
                return;
            var ignore = this.canExecuteChanged.RaiseAsync(this, EventArgs.Empty);
        }
    }
}
