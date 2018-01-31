using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class CommandBaseBase : ObservableObject, IControllableCommand
    {
        internal CommandBaseBase() { }

        public event EventHandler CanExecuteChanged;

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
        public object Tag
        {
            get => this.tag;
            set => ForceSet(ref this.tag, value);
        }

        internal void ThrowUnhandledError(Exception error)
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

        public virtual void OnCanExecuteChanged()
        {
            var temp = CanExecuteChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() => temp(this, EventArgs.Empty));
        }
    }
}
