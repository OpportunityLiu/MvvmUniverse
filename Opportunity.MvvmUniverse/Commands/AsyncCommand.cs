using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate bool AsyncCommandPredicate(AsyncCommand command);

    public abstract class AsyncCommand : CommandBase, IAsyncCommand
    {
        public static AsyncCommand Create(AsyncTaskCommandExecutor execute)
            => new AsyncTaskCommand(execute, null);
        public static AsyncCommand Create(AsyncTaskCommandExecutor execute, AsyncCommandPredicate canExecute)
            => new AsyncTaskCommand(execute, canExecute);

        public static AsyncCommand Create(AsyncActionCommandExecutor execute)
            => new AsyncActionCommand(execute, null);
        public static AsyncCommand Create(AsyncActionCommandExecutor execute, AsyncCommandPredicate canExecute)
            => new AsyncActionCommand(execute, canExecute);

        public static AsyncCommand<T> Create<T>(AsyncTaskCommandExecutor<T> execute)
            => new AsyncTaskCommand<T>(execute, null);
        public static AsyncCommand<T> Create<T>(AsyncTaskCommandExecutor<T> execute, AsyncCommandPredicate<T> canExecute)
            => new AsyncTaskCommand<T>(execute, canExecute);

        public static AsyncCommand<T> Create<T>(AsyncActionCommandExecutor<T> execute)
            => new AsyncActionCommand<T>(execute, null);
        public static AsyncCommand<T> Create<T>(AsyncActionCommandExecutor<T> execute, AsyncCommandPredicate<T> canExecute)
            => new AsyncActionCommand<T>(execute, canExecute);

        protected AsyncCommand(AsyncCommandPredicate canExecute)
        {
            this.canExecute = canExecute;
        }

        private readonly AsyncCommandPredicate canExecute;
        protected AsyncCommandPredicate CanExecuteDelegate => this.canExecute;

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
            return this.canExecute.Invoke(this);
        }

        protected override bool OnStarting()
        {
            var r = base.OnStarting();
            if (r)
                IsExecuting = true;
            return r;
        }

        protected override void OnError(Exception error)
        {
            IsExecuting = false;
            base.OnError(error);
        }

        protected override void OnFinished()
        {
            IsExecuting = false;
            base.OnFinished();
        }
    }
}
