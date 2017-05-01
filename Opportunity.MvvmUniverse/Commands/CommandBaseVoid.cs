using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Opportunity.MvvmUniverse.Helpers;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class CommandBaseVoid : CommandBase, ICommand
    {
        bool ICommand.CanExecute(object parameter) => CanExecute();

        public bool CanExecute()
        {
            if (!IsEnabled)
                return false;
            return CanExecuteOverride();
        }

        protected virtual bool CanExecuteOverride() => true;

        void ICommand.Execute(object parameter) => Execute();

        public bool Execute()
        {
            if (CanExecute())
            {
                ExecuteImpl();
                return true;
            }
            return false;
        }

        protected abstract void ExecuteImpl();
    }
}
