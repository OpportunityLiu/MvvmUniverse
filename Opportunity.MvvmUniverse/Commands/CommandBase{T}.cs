using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Opportunity.MvvmUniverse.Helpers;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class CommandBase<T> : CommandBase, ICommand
    {

        protected static T Cast(object obj)
        {
            if (obj is T p)
                return p;
            else
                return default(T);
        }

        bool ICommand.CanExecute(object parameter) => CanExecute(Cast(parameter));

        public bool CanExecute(T parameter)
        {
            if (!IsEnabled)
                return false;
            return CanExecuteOverride(parameter);
        }
        protected virtual bool CanExecuteOverride(T parameter) => true;

        void ICommand.Execute(object parameter) => Execute(Cast(parameter));

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
