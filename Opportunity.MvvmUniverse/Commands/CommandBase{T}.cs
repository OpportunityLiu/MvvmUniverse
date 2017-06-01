using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class CommandBase<T> : ObservableObject, ICommand
    {
        public event EventHandler CanExecuteChanged;

        private bool isEnabled = true;
        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (Set(ref this.isEnabled, value))
                    RaiseCanExecuteChanged();
            }
        }

        private object tag;
        public object Tag
        {
            get => this.tag;
            set => ForceSet(ref this.tag, value);
        }

        public void RaiseCanExecuteChanged()
        {
            var temp = CanExecuteChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() => temp(this, EventArgs.Empty));
        }

        bool ICommand.CanExecute(object parameter) => CanExecute((T)parameter);

        public bool CanExecute(T parameter)
        {
            if (!IsEnabled)
                return false;
            return CanExecuteOverride(parameter);
        }

        protected virtual bool CanExecuteOverride(T parameter) => true;

        void ICommand.Execute(object parameter) => Execute((T)parameter);

        public bool Execute(T parameter)
        {
            if (CanExecute(parameter))
            {
                ExecuteImpl(parameter);
                return true;
            }
            return false;
        }

        protected abstract void ExecuteImpl(T parameter);
    }
}
