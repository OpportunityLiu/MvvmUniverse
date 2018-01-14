using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public sealed class AsyncCommand : CommandBase
    {
        public static AsyncCommand Create(AsyncAction execute) => new AsyncCommand(execute, null);
        public static AsyncCommand Create(AsyncAction execute, Func<bool> canExecute) => new AsyncCommand(execute, canExecute);
        public static AsyncCommand<T> Create<T>(AsyncAction<T> execute) => new AsyncCommand<T>(execute, null);
        public static AsyncCommand<T> Create<T>(AsyncAction<T> execute, Predicate<T> canExecute) => new AsyncCommand<T>(execute, canExecute);

        internal AsyncCommand(AsyncAction execute, Func<bool> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        private readonly AsyncAction execute;
        private readonly Func<bool> canExecute;
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
            return this.canExecute.Invoke();
        }

        protected override async void StartExecution()
        {
            this.IsExecuting = true;
            try
            {
                await this.execute.Invoke();
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
