using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate Task AsyncCommandExecutor(AsyncCommand command);
    public delegate bool AsyncCommandPredicate(AsyncCommand command);

    public sealed class AsyncCommand : CommandBase
    {
        public static AsyncCommand Create(AsyncCommandExecutor execute) => new AsyncCommand(execute, null);
        public static AsyncCommand Create(AsyncCommandExecutor execute, AsyncCommandPredicate canExecute) => new AsyncCommand(execute, canExecute);
        public static AsyncCommand<T> Create<T>(AsyncCommandExecutor<T> execute) => new AsyncCommand<T>(execute, null);
        public static AsyncCommand<T> Create<T>(AsyncCommandExecutor<T> execute, AsyncCommandPredicate<T> canExecute) => new AsyncCommand<T>(execute, canExecute);

        internal AsyncCommand(AsyncCommandExecutor execute, AsyncCommandPredicate canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        private readonly AsyncCommandExecutor execute;
        private readonly AsyncCommandPredicate canExecute;
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

        protected override bool CanExecuteOverride()
        {
            if (this.IsExecuting)
                return false;
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke(this);
        }

        protected override async void StartExecution()
        {
            this.IsExecuting = true;
            try
            {
                await this.execute.Invoke(this);
                OnFinished();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                this.IsExecuting = false;
            }
        }
    }
}
