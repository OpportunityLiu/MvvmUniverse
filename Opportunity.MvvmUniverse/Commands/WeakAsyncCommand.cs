using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public sealed class WeakAsyncCommand : CommandBase
    {
        public static WeakAsyncCommand Create(AsyncAction execute) => new WeakAsyncCommand(execute, null);
        public static WeakAsyncCommand Create(WeakAsyncAction execute) => new WeakAsyncCommand(execute, null);
        public static WeakAsyncCommand Create(AsyncAction execute, Func<bool> canExecute) => new WeakAsyncCommand(execute, canExecute);
        public static WeakAsyncCommand Create(WeakAsyncAction execute, WeakFunc<bool> canExecute) => new WeakAsyncCommand(execute, canExecute);
        public static WeakAsyncCommand<T> Create<T>(AsyncAction<T> execute) => new WeakAsyncCommand<T>(execute, null);
        public static WeakAsyncCommand<T> Create<T>(WeakAsyncAction<T> execute) => new WeakAsyncCommand<T>(execute, null);
        public static WeakAsyncCommand<T> Create<T>(AsyncAction<T> execute, Predicate<T> canExecute) => new WeakAsyncCommand<T>(execute, canExecute);
        public static WeakAsyncCommand<T> Create<T>(WeakAsyncAction<T> execute, WeakPredicate<T> canExecute) => new WeakAsyncCommand<T>(execute, canExecute);

        internal WeakAsyncCommand(AsyncAction execute, Func<bool> canExecute)
        {
            this.execute = new WeakAsyncAction(execute ?? throw new ArgumentNullException(nameof(execute)));
            if (canExecute != null)
                this.canExecute = new WeakFunc<bool>(canExecute);
        }

        internal WeakAsyncCommand(WeakAsyncAction execute, WeakFunc<bool> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        private readonly WeakAsyncAction execute;
        private readonly WeakFunc<bool> canExecute;
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

        protected override bool CanExecuteOverride()
        {
            if (!IsAlive)
                return false;
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
