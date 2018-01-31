using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate bool AsyncPredicate<T>(AsyncCommand<T> command, T parameter);

    public abstract class AsyncCommand<T> : CommandBase<T>, IAsyncCommand
    {
        protected AsyncCommand(AsyncPredicate<T> canExecute)
        {
            this.canExecute = canExecute;
        }

        private readonly AsyncPredicate<T> canExecute;
        protected AsyncPredicate<T> CanExecuteDelegate => this.canExecute;

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

        protected override void OnFinished(ExecutedEventArgs<T> e)
        {
            IsExecuting = false;
            base.OnFinished(e);
        }
    }
}
