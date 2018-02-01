using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class ObservableCommandBase : ObservableObject, IControllable
    {
        internal ObservableCommandBase() { }

        private bool isEnabled = true;
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
            if (error == null)
                return;
            DispatcherHelper.BeginInvoke(() =>
            {
                if (error is AggregateException ae)
                    throw new AggregateException(ae.InnerExceptions);
                else
                    throw new AggregateException(error);
            });
        }

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Raise <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public virtual void OnCanExecuteChanged()
        {
            var temp = this.CanExecuteChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() => temp(this, EventArgs.Empty));
        }

    }
}
