using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public sealed class WeakAsyncCommand<T> : CommandBase<T>
    {
        public WeakAsyncCommand(WeakAsyncAction<T> execute, WeakPredicate<T> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public WeakAsyncCommand(WeakAsyncAction<T> execute) : this(execute, null)
        {
        }

        private readonly WeakAsyncAction<T> execute;
        private readonly WeakPredicate<T> canExecute;
        private bool isExecuting = false;

        public bool IsAlive => this.execute.IsAlive && (this.canExecute?.IsAlive == true);

        public bool IsExecuting
        {
            get => this.isExecuting;
            private set
            {
                if (Set(ref this.isExecuting, value))
                    OnCanExecuteChanged();
            }
        }

        protected override bool CanExecuteOverride(T parameter)
        {
            if (!IsAlive)
                return false;
            if (this.IsExecuting)
                return false;
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke(parameter);
        }

        protected override async void ExecuteImpl(T parameter)
        {
            this.IsExecuting = true;
            try
            {
                await this.execute.Invoke(parameter);
            }
            finally
            {
                this.IsExecuting = false;
            }
        }
    }
}
