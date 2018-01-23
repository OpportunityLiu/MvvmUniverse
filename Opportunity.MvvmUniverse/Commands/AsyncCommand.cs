using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class AsyncCommand : CommandBase, IAsyncCommand
    {
        public static AsyncCommand Create(AsyncAction execute)
            => new AsyncTaskCommand(execute, null);
        public static AsyncCommand Create(AsyncAction execute, Func<bool> canExecute)
            => new AsyncTaskCommand(execute, canExecute);

        public static AsyncCommand Create(Func<IAsyncAction> execute)
            => new AsyncActionCommand(execute, null);
        public static AsyncCommand Create(Func<IAsyncAction> execute, Func<bool> canExecute)
            => new AsyncActionCommand(execute, canExecute);

        public static AsyncCommand<T> Create<T>(AsyncAction<T> execute)
            => new AsyncTaskCommand<T>(execute, null);
        public static AsyncCommand<T> Create<T>(AsyncAction<T> execute, Predicate<T> canExecute)
            => new AsyncTaskCommand<T>(execute, canExecute);

        public static AsyncCommand<T> Create<T>(Func<T, IAsyncAction> execute)
            => new AsyncActionCommand<T>(execute, null);
        public static AsyncCommand<T> Create<T>(Func<T, IAsyncAction> execute, Predicate<T> canExecute)
            => new AsyncActionCommand<T>(execute, canExecute);

        internal AsyncCommand(Func<bool> canExecute)
        {
            this.canExecute = canExecute;
        }

        private readonly Func<bool> canExecute;
        protected Func<bool> CanExecuteDelegate => this.canExecute;

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

        protected override bool CanExecuteOverride()
        {
            if (this.IsExecuting)
                return false;
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke();
        }
    }
}
