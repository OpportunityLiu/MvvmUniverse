using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public class AsyncCommand<T> : CommandBase<T>
    {
        public AsyncCommand(AsyncAction<T> execute, Predicate<T> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public AsyncCommand(AsyncAction<T> execute) : this(execute, null)
        {
        }

        private readonly AsyncAction<T> execute;
        private readonly Predicate<T> canExecute;
        private bool executing = false;

        public bool Executing
        {
            get => this.executing;
            protected set => Set(ref this.executing, value);
        }

        protected override bool CanExecuteOverride(T parameter)
        {
            if(this.Executing)
                return false;
            if(this.canExecute == null)
                return true;
            return this.canExecute.Invoke(parameter);
        }

        protected override async void ExecuteImpl(T parameter)
        {
            this.Executing = true;
            try
            {
                await this.execute.Invoke(parameter);
            }
            finally
            {
                this.Executing = false;
            }
        }
    }
}
