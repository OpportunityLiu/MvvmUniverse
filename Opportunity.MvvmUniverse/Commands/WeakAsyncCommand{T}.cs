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
        internal WeakAsyncCommand(AsyncAction<T> execute, Predicate<T> canExecute)
        {
            this.execute = new WeakAsyncAction<T>(execute ?? throw new ArgumentNullException(nameof(execute)));
            if (canExecute != null)
                this.canExecute = new WeakPredicate<T>(canExecute);
        }

        internal WeakAsyncCommand(WeakAsyncAction<T> execute, WeakPredicate<T> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
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

        protected override async void StartExecution(T parameter)
        {
            this.IsExecuting = true;
            try
            {
                await this.execute.Invoke(parameter);
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
