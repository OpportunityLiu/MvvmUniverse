using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate Task AsyncCommandExecutor<T>(AsyncCommand<T> command, T parameter);
    public delegate bool AsyncCommandPredicate<T>(AsyncCommand<T> command, T parameter);

    public sealed class AsyncCommand<T> : CommandBase<T>
    {
        internal AsyncCommand(AsyncCommandExecutor<T> execute, AsyncCommandPredicate<T> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        private readonly AsyncCommandExecutor<T> execute;
        private readonly AsyncCommandPredicate<T> canExecute;
        private bool isExecuting = false;

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
            if (this.IsExecuting)
                return false;
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke(this, parameter);
        }

        protected override async void StartExecution(T parameter)
        {
            this.IsExecuting = true;
            try
            {
                await this.execute.Invoke(this, parameter);
                OnFinished(parameter);
            }
            catch (Exception ex)
            {
                OnError(parameter, ex);
            }
            finally
            {
                this.IsExecuting = false;
            }
        }
    }
}
