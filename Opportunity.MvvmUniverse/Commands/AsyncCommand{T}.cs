using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate bool AsyncCommandPredicate<T>(AsyncCommand<T> command, T parameter);

    public abstract class AsyncCommand<T> : CommandBase<T>, IAsyncCommand
    {
        protected AsyncCommand(AsyncCommandPredicate<T> canExecute)
        {
            this.canExecute = canExecute;
        }

        private readonly AsyncCommandPredicate<T> canExecute;
        protected AsyncCommandPredicate<T> CanExecuteDelegate => this.canExecute;

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

        protected override bool CanExecuteOverride(T parameter)
        {
            if (this.IsExecuting)
                return false;
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke(this, parameter);
        }

        protected override bool OnStarting(T parameter)
        {
            var r = base.OnStarting(parameter);
            if (r)
                IsExecuting = true;
            return r;
        }

        protected override void OnError(T parameter, Exception error)
        {
            IsExecuting = false;
            base.OnError(parameter, error);
        }

        protected override void OnFinished(T parameter)
        {
            IsExecuting = false;
            base.OnFinished(parameter);
        }
    }
}
