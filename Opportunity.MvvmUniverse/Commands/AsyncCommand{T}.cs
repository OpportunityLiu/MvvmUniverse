using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class AsyncCommand<T> : CommandBase<T>, IAsyncCommand
    {
        internal AsyncCommand(Predicate<T> canExecute)
        {
            this.canExecute = canExecute;
        }

        private readonly Predicate<T> canExecute;
        protected Predicate<T> CanExecuteDelegate => this.canExecute;

        private bool isExecuting = false;
        public bool IsExecuting
        {
            get => this.isExecuting;
            protected set
            {
                if (Set(ref this.isExecuting, value))
                    OnCanExecuteChanged();
            }
        }

        protected override bool CanExecuteOverride(T parameter)
        {
            if (this.IsExecuting)
                return false;
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke(parameter);
        }
    }
}
