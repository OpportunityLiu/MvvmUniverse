using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate bool AsyncPredicate(AsyncCommand command);

    public abstract class AsyncCommand : CommandBase, IAsyncCommand
    {
        #region Factory methods
        public static AsyncCommand Create(AsyncTaskExecutor execute)
            => new AsyncTaskCommand(execute, null);
        public static AsyncCommand Create(AsyncTaskExecutor execute, AsyncPredicate canExecute)
            => new AsyncTaskCommand(execute, canExecute);

        public static AsyncCommand Create(AsyncActionExecutor execute)
            => new AsyncActionCommand(execute, null);
        public static AsyncCommand Create(AsyncActionExecutor execute, AsyncPredicate canExecute)
            => new AsyncActionCommand(execute, canExecute);

        public static AsyncCommand<T> Create<T>(AsyncTaskExecutor<T> execute)
            => new AsyncTaskCommand<T>(execute, null);
        public static AsyncCommand<T> Create<T>(AsyncTaskExecutor<T> execute, AsyncPredicate<T> canExecute)
            => new AsyncTaskCommand<T>(execute, canExecute);

        public static AsyncCommand<T> Create<T>(AsyncActionExecutor<T> execute)
            => new AsyncActionCommand<T>(execute, null);
        public static AsyncCommand<T> Create<T>(AsyncActionExecutor<T> execute, AsyncPredicate<T> canExecute)
            => new AsyncActionCommand<T>(execute, canExecute);
        #endregion Factory methods

        protected AsyncCommand(AsyncPredicate canExecute)
        {
            this.CanExecuteDelegate = canExecute;
        }

        protected AsyncPredicate CanExecuteDelegate { get; }

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
            if (this.CanExecuteDelegate == null)
                return true;
            return this.CanExecuteDelegate.Invoke(this);
        }

        protected override bool OnStarting()
        {
            var r = base.OnStarting();
            if (r)
                IsExecuting = true;
            return r;
        }

        protected override void OnFinished(ExecutedEventArgs e)
        {
            IsExecuting = false;
            base.OnFinished(e);
        }
    }
}
